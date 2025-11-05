using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

public class AnyReferenceNamingWidget(
    string path = ".",
    bool showOptionalDetails = false,
    Widget? customFallbackName = null,
    ReferenceNamingWidgetModel? widgetModel = null
) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        return ReferenceHandler
            .BuildAnyReferencesNaming(navigator, path, context, renderer, showOptionalDetails: showOptionalDetails,
                customFallbackName: customFallbackName, widgetModel)
            .Render(navigator, renderer, context);
    }
}