namespace Scalesoft.DisplayTool.Extensions.Localization.CdaCodeLists;

public interface ITranslationsStorage
{
    /// <summary>
    /// Begins a batch write operation. Returns a batch writer that accumulates changes.
    /// </summary>
    IBatchWriter BeginBatch();

    /// <summary>
    /// Gets a concept by code and system.
    /// </summary>
    Concept? GetConceptByCodeAndSystem(string code, string system);

    /// <summary>
    /// Gets a concept by code and valueset (searches across all systems in the valueset).
    /// </summary>
    Concept? GetConceptByCodeAndValueSet(string code, string valueSet);

    /// <summary>
    /// Gets all concepts across all systems in a value set.
    /// </summary>
    IEnumerable<Concept> GetValueSet(string valueSet);

    /// <summary>
    /// Gets the timestamp of the last initialization.
    /// </summary>
    DateTime? GetLastInitializationTime();

    /// <summary>
    /// Sets the timestamp of the last initialization.
    /// </summary>
    void SetLastInitializationTime(DateTime timestamp);

    /// <summary>
    /// Clears all data from storage (concepts, value sets, and metadata).
    /// </summary>
    void Clear();
}

public interface IBatchWriter : IDisposable
{
    /// <summary>
    /// Adds a concept to the batch. Will automatically flush every 100 entries.
    /// </summary>
    void AddConcept(Concept concept);

    /// <summary>
    /// Manually flushes the current batch to storage.
    /// </summary>
    void Flush();
}