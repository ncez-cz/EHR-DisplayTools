using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Timeline(IEnumerable<Widget> cards, string? cssClass = null, bool vertical = true)
    : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var widget = new Container(cards.ToArray(), ContainerType.Div,
            "timeline " + (vertical ? "timeline-vertical " : "timeline-horizontal ") + cssClass);

        return await widget.Render(navigator, renderer, context);
    }
}