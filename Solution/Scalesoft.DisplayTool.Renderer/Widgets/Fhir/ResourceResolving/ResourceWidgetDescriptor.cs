using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

public class ResourceWidgetDescriptor
{
    public required bool RequiresExternalTitle { get; init; }
    public required InstantiateDelegate Instantiate { get; init; }
}

public delegate List<Widget> InstantiateDelegate(List<XmlDocumentNavigator> input);