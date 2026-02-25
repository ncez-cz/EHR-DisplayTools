using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowQuantity(string path = ".", bool showUnit = true) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (context.RenderMode == RenderMode.Documentation)
        {
            return navigator.SelectSingleNode(path).GetFullPath();
        }

        var lang = context.Language.Primary.Code;
        var shortLang = context.Language.Primary.ShortCode;

        List<ParseError> errors = [];

        var unitString = navigator.SelectSingleNode($"{path}/f:unit/@value").Node?.Value;
        var unitSystem = navigator.SelectSingleNode($"{path}/f:system/@value").Node?.Value;
        var unitCode = navigator.SelectSingleNode($"{path}/f:code/@value").Node?.Value;

        if (IsDataAbsent(navigator, path))
        {
            return await new AbsentData(path).Render(navigator, renderer, context);
        }

        Widget? unitWidget = null;
        if (showUnit)
        {
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

            if (unitWidget == null && unitCode != null)
            {
                unitWidget = new CodedValue(unitCode, unitSystem, unitCode);
            }

            if (unitWidget == null)
            {
                errors.Add(new ParseError
                {
                    Kind = ErrorKind.MissingValue,
                    Message = "Missing unit",
                    Path = navigator.GetFullPath(),
                    Severity = ErrorSeverity.Warning,
                });
            }
        }

        List<Widget> widgets =
        [
            new ChangeContext(path, new Optional("f:comparator",
                    new EnumLabel(".", "http://hl7.org/fhir/ValueSet/quantity-comparator")),
                new Optional("f:value", new ShowDecimal())),
        ];
        if (showUnit)
        {
            widgets.Add(new ChangeContext(path, new ConstantText(" "), new If(
                _ => unitWidget != null && !(unitString == "1" || unitCode == "1"), unitWidget!)));
        }

        widgets.Add(new UncertaintyExtensions());

        var result = await widgets.RenderConcatenatedResult(navigator, renderer, context);
        errors.AddRange(result.Errors);
        if (errors.MaxSeverity() >= ErrorSeverity.Fatal || !result.HasValue)
        {
            return errors;
        }

        return new RenderResult(result.Content, errors);
    }
}