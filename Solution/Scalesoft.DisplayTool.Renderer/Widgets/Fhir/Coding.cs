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
                        Message = "Coding must have display or code attribute",
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

        // Try to apply translations from extensions
        var displayNav = navigator.SelectSingleNode("f:display");
        Widget? fallbackWidget = null;
        if (fallback != null)
        {
            fallbackWidget = new ConstantText(fallback);
        }

        var translatedWidget =
            TranslationExtensionUtils.TranslateWithHierarchy(displayNav, lang, shortLang, fallbackWidget);
        if (translatedWidget != null)
        {
            return await translatedWidget.Render(navigator, renderer, context);
        }

        // Otherwise, try termx or fall back to text / code+system
        if (fallback == null && code == null)
        {
            return new RenderResult("Neplatná hodnota",
            [
                new ParseError
                {
                    Kind = ErrorKind.InvalidValue,
                    Message = "Coding must have display or code attribute",
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