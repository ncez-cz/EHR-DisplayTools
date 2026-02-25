using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaMockCodedValue(string codeSelect, string codeSystem) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        CodedValue widget;
        if (string.IsNullOrEmpty(codeSelect))
        {
            widget = new CodedValue(codeSelect, codeSystem);

            return await widget.Render(navigator, renderer, context);
        }

        var code = new Text(codeSelect);
        var codeRendered = await code.Render(navigator, renderer, context);
        if (codeRendered.HasErrors)
        {
            return codeRendered;
        }

        widget = new CodedValue(codeRendered.Content, codeSystem, codeRendered.Content);

        return await widget.Render(navigator, renderer, context);
    }
}