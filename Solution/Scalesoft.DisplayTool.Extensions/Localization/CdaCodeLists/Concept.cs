using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.Extensions.Localization.CdaCodeLists;

public record Concept
{
    public required string Code;
    public required string System;
    public List<string> ValueSets = [];
    public LocalizedValue Translations = [];
    public Dictionary<string, LocalizedValue> Properties = [];
}