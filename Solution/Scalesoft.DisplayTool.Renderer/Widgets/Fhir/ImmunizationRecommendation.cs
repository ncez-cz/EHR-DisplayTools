using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ImmunizationRecommendation : SequentialResourceBase<ImmunizationRecommendation>, IResourceWidget
{
    public static string ResourceType => "ImmunizationRecommendation";
    public static bool HasBorderedContainer(Widget widget) => true;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        // The resource may contain multiple recommendations. Each recommendation may contain multiple vaccine codes. Each vaccine code may contain a rather long name.
        // Skip rendering the summary due to the above and rely on the fallback.
        return null;
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var cardContent = new List<Widget>
        {
            // id must be rendered by the parent class
            // implicitRules just contains a URL to a set of rules, and has little value to the end user 
            // if (navigator.EvaluateCondition("f:implicitRules"))
            // {
            //     cardContent.Add(new NameValuePair([new ConstantText("Dodatečná pravidla")], [new Text("f:implicitRules/@value")]));
            // }
            // ignore language - not in key elements
            // ignore identifier
            // ignore authority - not in key elements
            // ignore patient
            new NameValuePair([new EhdsiDisplayLabel(LabelCodes.Date)],
                [new ChangeContext("f:date", new ShowDateTime())]),
            new Heading([new LocalizedLabel("immunization-recommendation.recommendation")], HeadingSize.H5),
            new ConcatBuilder("f:recommendation", (_, _) =>
            [
                new Card(null, new Container([
                    new Optional("f:vaccineCode", new NameValuePair(
                        [new LocalizedLabel("immunization-recommendation.vaccineCode")], [
                            new CommaSeparatedBuilder(".", _ =>
                            [
                                new CodeableConcept(),
                            ])
                        ])),
                    new Optional("f:targetDisease",
                        new NameValuePair([new LocalizedLabel("immunization-recommendation.targetDisease")],
                            [new CodeableConcept()])),
                    new ChangeContext("f:forecastStatus",
                        new NameValuePair([new LocalizedLabel("immunization-recommendation.forecastStatus")],
                            [new CodeableConcept()])),
                    new Condition("f:forecastReason",
                        new NameValuePair([new LocalizedLabel("immunization-recommendation.forecastReason")],
                            [new CommaSeparatedBuilder("f:forecastReason", _ => [new CodeableConcept()]),])),
                    new Optional("f:dateCriterion", new ConcatBuilder(".", _ =>
                    [
                        new NameValuePair([
                            new EhdsiDisplayLabel(LabelCodes.Date),
                            new ConstantText(" ("),
                            new ChangeContext("f:code", new CodeableConcept()),
                            new ConstantText(")"),
                        ], [new ChangeContext("f:value", new ShowDateTime())]),
                    ])),
                    new Optional("f:description",
                        new NameValuePair([new LocalizedLabel("immunization-recommendation.description")],
                            [new Text("@value")])),
                    new Optional("f:series",
                        new NameValuePair([new LocalizedLabel("immunization-recommendation.series")],
                            [new Text("@value")])),
                    new Condition("f:doseNumberPositiveInt | f:doseNumberString",
                        new NameValuePair([new EhdsiDisplayLabel(LabelCodes.DoseNumber)],
                            [new OpenTypeElement(null, "doseNumber")])), // positiveInt | string
                    new Condition("f:seriesDosesPositiveInt | f:seriesDosesString",
                        new NameValuePair([new LocalizedLabel("immunization-recommendation.seriesDoses")],
                            [new OpenTypeElement(null, "seriesDoses")])), // positiveInt | string
                    new Optional("f:supportingImmunization",
                        new Heading([new LocalizedLabel("immunization-recommendation.supportingImmunization")],
                            HeadingSize.H5, customClass: "ms-4"),
                        new ConcatBuilder(".", _ =>
                        [
                            ShowSingleReference.WithDefaultDisplayHandler(x =>
                            [
                                new AnyResource([x], x.Node?.Name, displayResourceType: false)
                            ]) // Immunization | ImmunizationEvaluation
                        ])),
                    new Optional("f:supportingPatientInformation",
                        new Heading([new LocalizedLabel("immunization-recommendation.supportingPatientInformation")],
                            HeadingSize.H6),
                        new ItemListBuilder(".", ItemListType.Unordered, _ =>
                        [
                            ShowSingleReference.WithDefaultDisplayHandler(x =>
                                [new AnyResource([x], x.Node?.Name, displayResourceType: false)]) // Resource
                        ])),
                ])),
            ]),
        };

        if (navigator.EvaluateCondition("f:text"))
        {
            cardContent.Add(new LineBreak());
            cardContent.Add(new NarrativeCollapser());
        }

        var widget = new Card(null, new Container(cardContent));

        return widget.Render(navigator, renderer, context);
    }
}