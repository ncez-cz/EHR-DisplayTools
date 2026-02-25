using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class NarrativeCollapser(string narrativeXPath = "f:text") : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var showNarrativeByDefault = NarrativeUtils.ShowNarrativeByDefault(navigator.SelectSingleNode("f:text"));

        var narrativeCollapser =
            new Collapser(
                [new EhdsiDisplayLabel(LabelCodes.OriginalNarrative)],
                [new Narrative(narrativeXPath)],
                !showNarrativeByDefault,
                customClass: "narrative-print-collapser " + (showNarrativeByDefault ? "d-flex" : "")
            );

        Widget widget =
            showNarrativeByDefault
                ? narrativeCollapser
                : new HideableDetails(ContainerType.Div,
                    narrativeCollapser
                );

        return widget.Render(navigator, renderer, context);
    }
}