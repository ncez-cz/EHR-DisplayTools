namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Sections;

public enum PredefinedSectionType
{
    AnyResource,
    Summary,
    Ignore,
}

public enum SectionType
{
    AnyResource = PredefinedSectionType.AnyResource,
    Summary = PredefinedSectionType.Summary,
    Ignore = PredefinedSectionType.Ignore,
    Custom,
}