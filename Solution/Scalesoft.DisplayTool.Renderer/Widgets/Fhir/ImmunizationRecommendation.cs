using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ImmunizationRecommendation : Widget
{
    public static string ResourceType => "ImmunizationRecommendation";
    
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
            new NameValuePair([new DisplayLabel(LabelCodes.Date)], [new ChangeContext("f:date", new ShowDateTime())]),
            new Heading([new ConstantText("Doporučení")], HeadingSize.H5),
            new ConcatBuilder("f:recommendation", (_, _) =>
            [
                new Card(null, new Container([
                    new Optional("f:vaccineCode", new NameValuePair([new ConstantText("Typ vakcíny")], [
                        new CommaSeparatedBuilder(".", _ =>
                        [
                            new CodeableConcept(),
                        ])
                    ])),
                    new Optional("f:targetDisease",
                        new NameValuePair([new ConstantText("Očkování proti")], [new CodeableConcept()])),
                    new ChangeContext("f:forecastStatus",
                        new NameValuePair([new ConstantText("Stav pacienta s ohledem na očkovací protokol")],
                            [new CodeableConcept()])),
                    new Condition("f:forecastReason",
                        new NameValuePair([new ConstantText("Důvod stavu pacienta s ohledem na očkovací protokol")],
                            [new CommaSeparatedBuilder("f:forecastReason", _ => [new CodeableConcept()]),])),
                    new Optional("f:dateCriterion", new ConcatBuilder(".", _ =>
                    [
                        new NameValuePair([
                            new DisplayLabel(LabelCodes.Date),
                            new ConstantText(" ("),
                            new ChangeContext("f:code", new CodeableConcept()),
                            new ConstantText(")"),
                        ], [new ChangeContext("f:value", new ShowDateTime())]),
                    ])),
                    new Optional("f:description", new NameValuePair([new ConstantText("Popis")], [new Text("@value")])),
                    new Optional("f:series",
                        new NameValuePair([new ConstantText("Název očkovací řady")], [new Text("@value")])),
                    new Condition("f:doseNumberPositiveInt | f:doseNumberString",
                        new NameValuePair([new DisplayLabel(LabelCodes.DoseNumber)],
                            [new OpenTypeElement(null, "doseNumber")])), // positiveInt | string
                    new Condition("f:seriesDosesPositiveInt | f:seriesDosesString",
                        new NameValuePair([new ConstantText("Celkový počet dávek")],
                            [new OpenTypeElement(null, "seriesDoses")])), // positiveInt | string
                    new Optional("f:supportingImmunization",
                        new Heading([new ConstantText("Minulá očkování")], HeadingSize.H5, customClass: "ms-4"),
                        new ConcatBuilder(".", _ =>
                        [
                            ShowSingleReference.WithDefaultDisplayHandler(x =>
                                [new AnyResource([x], x.Node?.Name, displayResourceType: false)]) // Immunization | ImmunizationEvaluation
                        ])),
                    new Optional("f:supportingPatientInformation",
                        new Heading([new ConstantText("Pozorování pacienta")], HeadingSize.H6),
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