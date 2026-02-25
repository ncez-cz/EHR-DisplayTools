using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowQuantityUnit(string path = ".") : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var unitString = navigator.SelectSingleNode($"{path}/f:unit/@value").Node?.Value;
        var unitSystem = navigator.SelectSingleNode($"{path}/f:system/@value").Node?.Value;
        var unitCode = navigator.SelectSingleNode($"{path}/f:code/@value").Node?.Value;

        if (context.RenderMode == RenderMode.Documentation)
        {
            return string.IsNullOrEmpty(unitCode)
                ? navigator.SelectSingleNode($"{path}/f:unit/@value").GetFullPath()
                : navigator.SelectSingleNode($"{path}/f:code/@value").GetFullPath();
        }

        var lang = context.Language.Primary.Code;
        var shortLang = context.Language.Primary.ShortCode;
        List<ParseError> errors = [];

        Widget? unitWidget = null;
        if (context.PreferTranslationsFromDocument)
        {
            // Try to apply translations from extensions
            var displayNav = navigator.SelectSingleNode($"{path}/f:unit");
            Widget? fallbackWidget = null;
            if (unitString != null)
            {
                fallbackWidget = new ConstantText(unitString);
            }

            var translatedWidget =
                TranslationExtensionUtils.TranslateWithHierarchy(displayNav, lang, shortLang, fallbackWidget);
            if (translatedWidget != null)
            {
                unitWidget = translatedWidget;
            }
        }

        if ((unitCode == "1" || unitString == "1") && unitWidget == null)
        {
            return "-"; // Quantity explicitly specifies that there is no unit 
        }

        if (unitWidget == null && unitCode != null)
        {
            unitWidget = new CodedValue(unitCode, unitSystem, unitCode);
        }

        if (unitWidget == null)
        {
            return string.Empty;
        }

        var result = await unitWidget.Render(navigator, renderer, context);

        errors.AddRange(result.Errors);
        if (errors.MaxSeverity() >= ErrorSeverity.Fatal || !result.HasValue)
        {
            return errors;
        }

        return new RenderResult(result.Content, errors);
    }
}