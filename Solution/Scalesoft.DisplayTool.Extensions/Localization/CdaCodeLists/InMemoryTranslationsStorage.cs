namespace Scalesoft.DisplayTool.Extensions.Localization.CdaCodeLists;

public class InMemoryTranslationsStorage : ITranslationsStorage
{
    private readonly Dictionary<string, CodeSystemMap> m_codeSystems = new();
    private readonly Dictionary<string, List<string>> m_valueSetSystemsMap = new();
    private readonly object m_lock = new();
    private DateTime? m_lastInitializationTime;

    public IBatchWriter BeginBatch()
    {
        return new InMemoryBatchWriter(this);
    }

    public Concept? GetConceptByCodeAndSystem(string code, string system)
    {
        return m_codeSystems.TryGetValue(system, out var codeSystem) ? codeSystem.GetValueOrDefault(code) : null;
    }

    public Concept? GetConceptByCodeAndValueSet(string code, string valueSet)
    {
        if (!m_valueSetSystemsMap.TryGetValue(valueSet, out var systems))
        {
            return null;
        }

        foreach (var system in systems)
        {
            if (!m_codeSystems.TryGetValue(system, out var codeSystem))
            {
                continue;
            }

            var concept = codeSystem.GetValueOrDefault(code);
            if (concept != null && concept.ValueSets.Contains(valueSet))
            {
                return concept;
            }
        }

        return null;
    }

    public IReadOnlyDictionary<string, Concept> GetCodeSystem(string system)
    {
        if (m_codeSystems.TryGetValue(system, out var codeSystem))
        {
            return codeSystem;
        }

        return new Dictionary<string, Concept>();
    }

    public IEnumerable<Concept> GetValueSet(string valueSet)
    {
        if (m_valueSetSystemsMap.TryGetValue(valueSet, out var systems))
        {
            var concepts = new List<Concept>();
            foreach (var system in systems)
            {
                if (m_codeSystems.TryGetValue(system, out var codeSystem))
                {
                    concepts.AddRange(codeSystem.Values.Where(c => c.ValueSets.Contains(valueSet)));
                }
            }

            return concepts;
        }

        return Enumerable.Empty<Concept>();
    }

    public DateTime? GetLastInitializationTime()
    {
        return m_lastInitializationTime;
    }

    public void SetLastInitializationTime(DateTime timestamp)
    {
        lock (m_lock)
        {
            m_lastInitializationTime = timestamp;
        }
    }

    public void Clear()
    {
        lock (m_lock)
        {
            m_codeSystems.Clear();
            m_valueSetSystemsMap.Clear();
            m_lastInitializationTime = null;
        }
    }

    private class InMemoryBatchWriter : IBatchWriter
    {
        private readonly InMemoryTranslationsStorage m_storage;

        public InMemoryBatchWriter(InMemoryTranslationsStorage storage)
        {
            m_storage = storage;
        }

        public void AddConcept(Concept concept)
        {
            // Write immediately to memory - no need for batching in RAM
            lock (m_storage.m_lock)
            {
                // Ensure code system exists
                if (!m_storage.m_codeSystems.TryGetValue(concept.System, out var codeSystem))
                {
                    codeSystem = new CodeSystemMap();
                    m_storage.m_codeSystems[concept.System] = codeSystem;
                }

                // Add concept to code system
                codeSystem[concept.Code] = concept;

                // Update value set mappings
                foreach (var valueSet in concept.ValueSets)
                {
                    if (!m_storage.m_valueSetSystemsMap.TryGetValue(valueSet, out var systems))
                    {
                        systems = [];
                        m_storage.m_valueSetSystemsMap[valueSet] = systems;
                    }

                    if (!systems.Contains(concept.System))
                    {
                        systems.Add(concept.System);
                    }
                }
            }
        }

        public void Flush()
        {
            // Not applicable for in-memory storage
        }

        public void Dispose()
        {
            // Not applicable for in-memory storage
        }
    }
}

public class CodeSystemMap : Dictionary<string, Concept>;