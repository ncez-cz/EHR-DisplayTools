using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class EhdsiDisplayLabel (string code) : Widget
{
    public override async Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
        var translated = await context.TranslatorWrapper.GetCodedValue(
            code,
            "1.3.6.1.4.1.12559.11.10.1.3.1.44.4",
            context.Language.Primary.Code,
            context.Language.Fallback.Code);

        return translated ?? code;
    }
}
