using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowAnnotationCompact : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        // ignore author, time
        var widget = new Container([new Markdown("f:text/@value")], ContainerType.Span,
            idSource: navigator); // TODO consider rendering markdown #DT-202

        return widget.Render(navigator, renderer, context);
    }
}