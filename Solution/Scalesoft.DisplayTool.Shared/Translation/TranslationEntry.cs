namespace Scalesoft.DisplayTool.Shared.Translation;

public record TranslationEntry
{
    public required string Code;
    public required string System;
    public LocalizedValue Translations = [];
    public Dictionary<string, LocalizedValue> Properties = [];
}

public class LocalizedValue : Dictionary<string, string>;