using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ModifierExtensionCheck : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (navigator.EvaluateCondition("//f:modifierExtension"))
        {
            var result = await new Widgets.Alert(
                new LocalizedLabel(
                    "general.unkown-modifier-message"
                ),
                Severity.Error
            ).Render(navigator, renderer, context);

            result.Errors.Add(new ParseError
            {
                Kind = ErrorKind.InvalidValue, Message = "Unknown modifierExtension encountered.",
                Path = "//f:modifierExtension", Severity = ErrorSeverity.Warning
            });
            return result;
        }

        return string.Empty;
    }
}