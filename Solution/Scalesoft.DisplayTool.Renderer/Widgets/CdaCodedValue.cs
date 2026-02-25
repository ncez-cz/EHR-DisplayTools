using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class CdaCodedValue(string fileName, string code, string codeSystem) : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
        navigator.Variables.TryGetValue(fileName, out var fileNameVal);
        navigator.Variables.TryGetValue(code, out var codeVal);
        navigator.Variables.TryGetValue(codeSystem, out var codeSystemVal);
        if (fileNameVal is not string file || codeVal is not string code1 || codeSystemVal is not string codeSystem1)
        {
            throw new InvalidOperationException("Expected string variables for reader languages");
        }

        var widget = new CodedValue(code1, codeSystem1, string.Empty);
        return widget.Render(navigator, renderer, context);
    }
}
