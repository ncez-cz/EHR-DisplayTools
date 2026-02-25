using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaCodedValue(string codeSelect, string? codeSystem, string? codeSystemSelect = null) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        string? codeSystemVal = codeSystem;
        if (!string.IsNullOrEmpty(codeSystemSelect))
        {
            var codeSystemTextWidget = new Text(codeSelect);
            var codeSystemWidgetRendered = await codeSystemTextWidget.Render(navigator, renderer, context);
            if (codeSystemWidgetRendered.HasErrors)
            {
                return codeSystemWidgetRendered;
            }

            codeSystemVal = codeSystemWidgetRendered.Content;
        }

        CodedValue widget;
        if (string.IsNullOrEmpty(codeSelect))
        {
            widget = new CodedValue(codeSelect, codeSystemVal);

            return await widget.Render(navigator, renderer, context);
        }

        var code = new Text(codeSelect);
        var codeRendered = await code.Render(navigator, renderer, context);
        if (codeRendered.HasErrors)
        {
            return codeRendered;
        }

        widget = new CodedValue(codeRendered.Content, codeSystemVal, codeRendered.Content, displayCodeSystem: true,
            displayCodeSystemOnFallbackOnly: true);

        return await widget.Render(navigator, renderer, context);
    }
}