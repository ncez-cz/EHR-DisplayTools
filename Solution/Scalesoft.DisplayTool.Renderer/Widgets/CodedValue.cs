using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class CodedValue(
    string? code,
    string? codeSystem,
    string? fallbackValue = null,
    bool displayCodeSystem = false,
    bool displayCodeSystemOnFallbackOnly = false,
    bool isValueSet = false,
    string? preferredCodeSystemOverride = null
) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var language = context.Language.Primary.Code;
        var fallbackLanguage = context.Language.Fallback.Code;

        if (code == null || (codeSystem == null && preferredCodeSystemOverride == null))
        {
            return fallbackValue ?? code ?? string.Empty;
        }

        string? appliedCodeSystem = null;
        string? translated = null;
        if (!string.IsNullOrWhiteSpace(preferredCodeSystemOverride))
        {
            appliedCodeSystem = preferredCodeSystemOverride;
            translated = await context.TranslatorWrapper.GetCodedValue(
                code,
                appliedCodeSystem,
                language,
                fallbackLanguage,
                isValueSet);
        }

        if (string.IsNullOrWhiteSpace(translated) && !string.IsNullOrEmpty(codeSystem))
        {
            appliedCodeSystem = codeSystem;
            translated = await context.TranslatorWrapper.GetCodedValue(
                code,
                appliedCodeSystem,
                language,
                fallbackLanguage,
                isValueSet);
        }

        var value = translated ?? fallbackValue ?? string.Empty;

        if (context.RenderMode == RenderMode.Documentation)
        {
            value = navigator.GetFullPath();
        }

        List<Widget> widgets =
        [
            new ConstantText(value),
        ];

        if (!displayCodeSystem)
        {
            return await widgets.RenderConcatenatedResult(navigator, renderer, context);
        }

        if (!displayCodeSystemOnFallbackOnly ||
            (string.IsNullOrEmpty(translated) && !string.IsNullOrEmpty(fallbackValue)))
        {
            if (context.RenderMode != RenderMode.Documentation)
            {
                widgets.Add(new HideableDetails(ContainerType.Span, new ConstantText($" ({appliedCodeSystem})")));
            }
        }

        return await widgets.RenderConcatenatedResult(navigator, renderer, context);
    }
}