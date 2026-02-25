using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowContactDetail(string xpath) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var widget = new ConcatBuilder(xpath, _ =>
        [
            new Container([
                new Optional("f:name",
                    new NameValuePair(
                        new LocalizedLabel("general.name"),
                        new Text("@value")
                    )
                ),
                new ShowContactPoint(),
            ], ContainerType.Span, optionalClass: "name-value-pair-wrapper w-fit-content"),
        ]);


        return widget.Render(navigator, renderer, context);
    }
}