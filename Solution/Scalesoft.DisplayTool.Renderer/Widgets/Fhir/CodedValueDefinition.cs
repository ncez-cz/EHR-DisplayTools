namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public record CodedValueDefinition(string Code, string CodeSystem)
{
    public string Code { get; } = Code;
    public string CodeSystem { get; } = CodeSystem;

    public static CodedValueDefinition LoincValue(string code) => new CodedValueDefinition(code, "http://loinc.org");
}