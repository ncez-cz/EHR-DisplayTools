using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Coding(string? text = null, bool hideSystem = false, string? preferredCodeSystemOverride = null) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var lang = context.Language.Primary.Code;
        var shortLang = context.Language.Primary.ShortCode;

        if (IsDataAbsent(navigator, ".."))
        {
            return await new AbsentData("..").Render(navigator, renderer, context);
        }

        var fallback = text ?? navigator.SelectSingleNode("f:display/@value").Node?.Value;
        var code = navigator.SelectSingleNode("f:code/@value").Node?.Value;
        var codeSystem = navigator.SelectSingleNode("f:system/@value").Node?.Value;

        if (!context.PreferTranslationsFromDocument)
        {
            if (fallback == null && code == null)
            {
                return new RenderResult("Neplatná hodnota",
                [
                    new ParseError
                    {
                        Kind = ErrorKind.InvalidValue,
                        Message = "CodingIps must have display or code attribute",
                        Path = navigator.GetFullPath(),
                        Severity = ErrorSeverity.Warning,
                    },
                ]);
            }

            var widget = new CodedValue(
                code,
                codeSystem,
                fallback ?? code,
                displayCodeSystem: !hideSystem && fallback == null,
                displayCodeSystemOnFallbackOnly: true,
                preferredCodeSystemOverride: preferredCodeSystemOverride
            );

            return await widget.Render(navigator, renderer, context);
        }

        // First, try translations from extensions
        var extension = navigator
            .SelectAllNodes("f:display/f:extension[@url='http://hl7.org/fhir/StructureDefinition/translation']")
            .FirstOrDefault(x =>
                x.EvaluateCondition(
                    $"f:extension[@url='lang' and (f:valueCode/@value='{lang}' or f:valueCode/@value='{shortLang}')]"));
        var translation = extension?.SelectSingleNode("f:extension[@url='content']");
        if (translation?.Node != null)
        {
            return await new OpenTypeElement(null).Render(translation, renderer, context); // string | markdown
        }

        // Next, try checking if the current resource has the correct language
        var resourceHasLanguage = navigator.EvaluateCondition("ancestor::f:resource/*/f:language");
        var resourceHasCorrectLanguage =
            navigator.EvaluateCondition($"ancestor::f:resource/*/f:language[@value='{lang}' or @value='{shortLang}']");

        // Next, try checking if the current resource has no language, but the document has the right language
        var documentHasCorrectLanguage =
            navigator.EvaluateCondition($"/f:Bundle/f:language[@value='{lang}' or @value='{shortLang}']");

        if (resourceHasCorrectLanguage || (!resourceHasLanguage && documentHasCorrectLanguage))
        {
            if (fallback != null)
            {
                return fallback;
            }
        }

        // Otherwise, try termx or fall back to text / code+system
        if (fallback == null && code == null)
        {
            return new RenderResult("Neplatná hodnota",
            [
                new ParseError
                {
                    Kind = ErrorKind.InvalidValue,
                    Message = "CodingIps must have display or code attribute",
                    Path = navigator.GetFullPath(),
                    Severity = ErrorSeverity.Warning,
                },
            ]);
        }

        return await new CodedValue(
                code,
                codeSystem,
                fallback ?? code,
                displayCodeSystem: !hideSystem && fallback == null,
                displayCodeSystemOnFallbackOnly: true
            )
            .Render(navigator, renderer, context);
    }
}