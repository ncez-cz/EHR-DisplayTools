using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class LocalizedLabel(string code) : Widget
{
    public const string Url = "https://ncez.mzcr.cz/terminology/CodeSystem/displaytool-label";

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        return await GetTranslatedValue(code, context);
    }

    public static async Task<string> GetTranslatedValue(string code, RenderContext context)
    {
        var translated = await context.TranslatorWrapper.GetCodedValue(
            code,
            Url,
            context.Language.Primary.Code,
            context.Language.Fallback.Code
        );

        return translated ?? code;
    }
}