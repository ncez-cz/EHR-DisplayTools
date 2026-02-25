using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Sections;

public class SectionDefinition
{
    public SectionDefinition(
        string? code,
        SectionType type, 
        Severity? severity = null,
        SectionBuilder? customBuilder = null)
    {
        Code = code;
        Type = type;
        Severity = severity;
        CustomBuilder = customBuilder;
    }
    
    public string? Code { get; }
    
    public SectionType Type { get; } 

    public SectionBuilder? CustomBuilder { get; }

    public Severity? Severity { get; }
}