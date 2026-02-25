using LiteDB;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.Extensions.Localization.CdaCodeLists;

public class LiteDbTranslationsStorage : ITranslationsStorage, IDisposable
{
    private readonly LiteDatabase m_database;
    private readonly ILiteCollection<ConceptDocument> m_concepts;
    private readonly ILiteCollection<ValueSetSystemMap> m_valueSetMaps;
    private readonly ILiteCollection<MetadataDocument> m_metadata;

    public LiteDbTranslationsStorage(string databasePath)
    {
        m_database = new LiteDatabase(databasePath);
        m_concepts = m_database.GetCollection<ConceptDocument>("concepts");
        m_valueSetMaps = m_database.GetCollection<ValueSetSystemMap>("valueSetMaps");
        m_metadata = m_database.GetCollection<MetadataDocument>("metadata");

        // Create indexes for efficient lookups
        m_concepts.EnsureIndex(x => x.Id);
        m_concepts.EnsureIndex(x => x.System);
        m_valueSetMaps.EnsureIndex(x => x.ValueSet);
    }

    public IBatchWriter BeginBatch()
    {
        return new LiteDbBatchWriter(this);
    }

    public Concept? GetConceptByCodeAndSystem(string code, string system)
    {
        var id = GetConceptId(code, system);
        var doc = m_concepts.FindById(id);
        return doc?.ToConcept();
    }

    public Concept? GetConceptByCodeAndValueSet(string code, string valueSet)
    {
        var mapDoc = m_valueSetMaps.FindById(valueSet);
        if (mapDoc == null)
        {
            return null;
        }

        foreach (var system in mapDoc.Systems)
        {
            var concept = GetConceptByCodeAndSystem(code, system);
            if (concept != null && concept.ValueSets.Contains(valueSet))
            {
                return concept;
            }
        }

        return null;
    }

    public IEnumerable<Concept> GetValueSet(string valueSet)
    {
        var mapDoc = m_valueSetMaps.FindById(valueSet);
        if (mapDoc == null)
        {
            return [];
        }

        var concepts = new List<Concept>();
        foreach (var system in mapDoc.Systems)
        {
            var systemConcepts = m_concepts.Find(x => x.System == system);
            concepts.AddRange(systemConcepts.Select(x => x.ToConcept()).Where(c => c.ValueSets.Contains(valueSet)));
        }

        return concepts;
    }

    public DateTime? GetLastInitializationTime()
    {
        var metadata = m_metadata.FindById("lastInitialization");
        return metadata?.Timestamp;
    }

    public void SetLastInitializationTime(DateTime timestamp)
    {
        m_metadata.Upsert(
            new MetadataDocument
            {
                Id = "lastInitialization",
                Timestamp = timestamp,
            }
        );
    }

    public void Clear()
    {
        m_concepts.DeleteAll();
        m_valueSetMaps.DeleteAll();
        m_metadata.DeleteAll();
    }

    public void Dispose()
    {
        m_database.Dispose();
        GC.SuppressFinalize(this);
    }

    private static string GetConceptId(string code, string system)
    {
        return $"{system}|{code}";
    }

    private class LiteDbBatchWriter : IBatchWriter
    {
        private readonly LiteDbTranslationsStorage m_storage;
        private readonly List<ConceptDocument> m_conceptBatch = [];
        private readonly Dictionary<string, HashSet<string>> m_valueSetSystemsBatch = new();
        private const int BatchSize = 100;

        public LiteDbBatchWriter(LiteDbTranslationsStorage storage)
        {
            m_storage = storage;
        }

        public void AddConcept(Concept concept)
        {
            var doc = ConceptDocument.FromConcept(concept);
            m_conceptBatch.Add(doc);

            // Track value set to system mappings
            foreach (var valueSet in concept.ValueSets)
            {
                if (!m_valueSetSystemsBatch.TryGetValue(valueSet, out var systems))
                {
                    systems = [];
                    m_valueSetSystemsBatch[valueSet] = systems;
                }

                systems.Add(concept.System);
            }

            // Auto-flush every 100 entries
            if (m_conceptBatch.Count >= BatchSize)
            {
                Flush();
            }
        }

        public void Flush()
        {
            if (m_conceptBatch.Count > 0)
            {
                m_storage.m_concepts.Upsert(m_conceptBatch);
                m_conceptBatch.Clear();
            }

            // Update value set mappings
            foreach (var (valueSet, systems) in m_valueSetSystemsBatch)
            {
                var existing = m_storage.m_valueSetMaps.FindById(valueSet);
                if (existing != null)
                {
                    // Merge with existing systems
                    foreach (var system in systems)
                    {
                        if (!existing.Systems.Contains(system))
                        {
                            existing.Systems.Add(system);
                        }
                    }

                    m_storage.m_valueSetMaps.Update(existing);
                }
                else
                {
                    m_storage.m_valueSetMaps.Insert(
                        new ValueSetSystemMap
                        {
                            ValueSet = valueSet,
                            Systems = systems.ToList()
                        }
                    );
                }
            }

            m_valueSetSystemsBatch.Clear();
        }

        public void Dispose()
        {
            Flush();
        }
    }

    // Document classes for LiteDB
    private class ConceptDocument
    {
        public string Id { get; init; } = null!;
        public string Code { get; init; } = null!;
        public string System { get; init; } = null!;
        public List<string> ValueSets { get; init; } = [];
        public Dictionary<string, string> Translations { get; init; } = new();
        public Dictionary<string, Dictionary<string, string>> Properties { get; init; } = new();

        public static ConceptDocument FromConcept(Concept concept)
        {
            return new ConceptDocument
            {
                Id = GetConceptId(concept.Code, concept.System),
                Code = concept.Code,
                System = concept.System,
                ValueSets = [..concept.ValueSets],
                Translations = concept.Translations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Properties = concept.Properties.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToDictionary(p => p.Key, p => p.Value)
                )
            };
        }

        public Concept ToConcept()
        {
            var translations = new LocalizedValue();
            foreach (var (key, value) in Translations)
            {
                translations[key] = value;
            }

            var properties = new Dictionary<string, LocalizedValue>();
            foreach (var (propKey, propValue) in Properties)
            {
                var localizedValue = new LocalizedValue();
                foreach (var (lang, value) in propValue)
                {
                    localizedValue[lang] = value;
                }

                properties[propKey] = localizedValue;
            }

            return new Concept
            {
                Code = Code,
                System = System,
                ValueSets = [..ValueSets],
                Translations = translations,
                Properties = properties,
            };
        }
    }

    private class ValueSetSystemMap
    {
        [BsonId] public string ValueSet { get; init; } = null!;
        public List<string> Systems { get; init; } = [];
    }

    private class MetadataDocument
    {
        public string Id { get; init; } = null!;
        public DateTime Timestamp { get; init; }
    }
}