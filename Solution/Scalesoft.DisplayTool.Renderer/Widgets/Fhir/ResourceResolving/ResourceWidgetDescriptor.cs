using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

public class ResourceWidgetDescriptor
{
    public required bool RequiresExternalTitle { get; init; }

    public required HasBorderedContainerDelegate HasBorderedContainer { get; init; }

    public required InstantiateDelegate Instantiate { get; init; }
    public required RenderSummaryDelegate RenderSummary { get; init; }
}

public delegate List<Widget> InstantiateDelegate(List<XmlDocumentNavigator> input);

public delegate ResourceSummaryModel? RenderSummaryDelegate(XmlDocumentNavigator input);

public delegate bool HasBorderedContainerDelegate(Widget input);