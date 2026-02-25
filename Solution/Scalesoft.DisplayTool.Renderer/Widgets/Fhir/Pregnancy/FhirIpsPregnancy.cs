using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Pregnancy;

public class FhirIpsPregnancy(List<XmlDocumentNavigator> items) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var currentPregnancyEvents = items.Where(x =>
                x.EvaluateCondition(
                    "f:code/f:coding/f:system[@value='http://loinc.org'] and f:code/f:coding/f:code[@value='82810-3']"))
            .ToList();
        var historicalPregnancyEvents = items.Except(currentPregnancyEvents).ToList();

        var currentPregnanciesTable = new Container([
            new TextContainer(TextStyle.Bold, [new EhdsiDisplayLabel(LabelCodes.CurrentPregnancyStatus)]),
            ..currentPregnancyEvents.Select(x => new ChangeContext(x, new ObservationCard())),
        ], ContainerType.Div, "resource-container");

        var historicalPregnanciesTable = new Container([
            new TextContainer(TextStyle.Bold, [new EhdsiDisplayLabel(LabelCodes.HistoryOfPreviousPregnancies)]),
            ..historicalPregnancyEvents.Select(x => new ChangeContext(x, new ObservationCard())),
        ], ContainerType.Div, "resource-container");

        Widget[] widgetTree =
        [
            currentPregnanciesTable,
            historicalPregnanciesTable,
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}