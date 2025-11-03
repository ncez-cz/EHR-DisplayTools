using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

public class AnyResource(
    List<XmlDocumentNavigator> items,
    string? resourceType,
    bool displayResourceType = true
) : Widget
{
    public AnyResource(
        XmlDocumentNavigator item,
        string? resourceType = null,
        bool displayResourceType = true
    ) : this(
        [item],
        resourceType ?? item.Node?.Name,
        displayResourceType
    )
    {
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (!string.IsNullOrEmpty(resourceType) &&
            SupportedResourceProvider.SupportedResources.TryGetValue(resourceType, out var descriptor))
        {
            var displayTitle = descriptor.RequiresExternalTitle || displayResourceType;
            var widgets = descriptor.Instantiate(items);

            Widget result = displayTitle
                ? new Section(
                    ".",
                    null,
                    title: [new LocalNodeName(resourceType, items.Count > 1)],
                    content: widgets,
                    isCollapsed: false
                )
                : new Concat(widgets);

            return result.Render(navigator, renderer, context);
        }

        var fallback = items.Select(x =>
            new Row(
                [
                    new ChangeContext(
                        x,
                        new ConstantText($"{resourceType}/"),
                        new Text("f:id/@value"),
                        new NarrativeModal()
                    ),
                ],
                flexContainerClasses: "align-items-center justify-content-between",
                idSource: x
            )
        ).ToList<Widget>();

        var widget =
            new Section(
                ".",
                null,
                title: [new LocalNodeName(resourceType, items.Count > 1)],
                content: fallback
            );

        return widget.Render(navigator, renderer, context);
    }
}