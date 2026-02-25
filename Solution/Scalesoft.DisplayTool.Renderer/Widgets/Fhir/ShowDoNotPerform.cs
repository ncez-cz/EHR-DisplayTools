using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowDoNotPerform : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer, RenderContext context)
    {
        var widget = new ShowBoolean(new NullWidget(),
            new TextContainer(TextStyle.Bold, [new LocalizedLabel("general.do-not-perform")]), "f:doNotPerform");


        return widget.Render(navigator, renderer, context);
    }
}