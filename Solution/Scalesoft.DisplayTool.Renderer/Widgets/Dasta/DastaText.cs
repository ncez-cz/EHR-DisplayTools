using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaText(string select) : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
        var widget = new ChangeContext(select, 
        [
            // ignore dsip:autor
            new Text("dsip:ptext"),
            // TODO consider implementing ktext,priloha #DT-183
            // ignore dsip:ktext - text file content or base64-encoded binary file content (RTF, HTML, XML, PDF...)
            // ignore dsip:priloha - relative paths or URLs       
        ]);
        
        return widget.Render(navigator, renderer, context);
    }
}
