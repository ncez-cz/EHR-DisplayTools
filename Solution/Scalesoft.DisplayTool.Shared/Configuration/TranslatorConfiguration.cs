namespace Scalesoft.DisplayTool.Shared.Configuration;

public class TranslatorConfiguration
{
    public required TranslatorType Type { get; set; }
    
    public TermxTranslatorConfiguration? TermxTranslator { get; set; }
    
    public LocalTranslatorConfiguration? LocalTranslator { get; set; }
    
    public Dictionary<string, string>? KnownOidMappings { get; set; }
}

public class LocalTranslatorConfiguration
{
    public required StorageType StorageType { get; set; }

    public string? DatabaseConnectionString { get; set; }
}

public class TermxTranslatorConfiguration
{
    public string? BaseUrl { get; set; }
}

public enum TranslatorType
{
    LocalTranslator,
    TermxTranslator,
}

public enum StorageType
{
    InMemory,
    LiteDb,
}