using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert;

public class RiskAssessments(bool skipIdPopulation = true)
    : AlternatingBackgroundColumnResourceBase<RiskAssessments>, IResourceWidget
{
    public static string ResourceType => "RiskAssessment";

    public static bool HasBorderedContainer(Widget widget) => false;

    public RiskAssessments() : this(true)
    {
    }

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        var summaryItems = new List<Widget>();
        var condition = ReferenceHandler.GetSingleNodeNavigatorFromReference(item, "f:condition", ".");
        if (condition != null)
        {
            var conditionSummary = ConditionResource.RenderSummary(condition);
            if (conditionSummary != null)
            {
                summaryItems.Add(conditionSummary.Value);
            }
        }

        var predictionOutcomes = item.SelectAllNodes("f:prediction/f:outcome").ToList();
        if (summaryItems.Count != 0)
        {
            if (predictionOutcomes.Count != 0)
            {
                summaryItems.Add(new ConstantText(" - "));
            }
            else
            {
                summaryItems.Add(new ConstantText(" - "));
                summaryItems.Add(new LocalizedLabel("risk-assessment.risk-label"));
            }

            var predictionOutcome = predictionOutcomes[0];
            summaryItems.Add(new ChangeContext(predictionOutcome, new CodeableConcept()));
            if (predictionOutcomes.Count != 1)
            {
                summaryItems.Add(new ConstantText($" (+ {predictionOutcomes.Count - 1})"));
            }
        }

        if (summaryItems.Count == 0)
        {
            return null;
        }

        return new ResourceSummaryModel
        {
            Value = new Container(summaryItems, ContainerType.Span),
        };
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<RiskAssessmentInfrequentProperties>(navigator);

        var nameValuePairs = new FlexList([
            infrequentProperties.Condition(RiskAssessmentInfrequentProperties.Prediction,
                new ConcatBuilder("f:prediction", (_, _, nav) =>
                {
                    var predictionInfrequentProps = InfrequentProperties.Evaluate<PredictionInfrequentProperties>(nav);
                    return
                    [
                        new NameValuePair(
                            [new LocalizedLabel("risk-assessment.prediction")],
                            [
                                predictionInfrequentProps.Optional(PredictionInfrequentProperties.Outcome,
                                    new NameValuePair(
                                        new LocalizedLabel("risk-assessment.outcome"),
                                        new CodeableConcept(),
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        direction: FlexDirection.Row
                                    )),
                                predictionInfrequentProps.Condition(PredictionInfrequentProperties.Probability,
                                    new NameValuePair(
                                        new LocalizedLabel("risk-assessment.probability"),
                                        new OpenTypeElement(null, "probability"), // decimal | Range
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        direction: FlexDirection.Row
                                    )),
                                predictionInfrequentProps.Optional(PredictionInfrequentProperties.QualitativeRisk,
                                    new NameValuePair(
                                        new LocalizedLabel("risk-assessment.qualitativeRisk"),
                                        new CodeableConcept(),
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        direction: FlexDirection.Row
                                    )),
                                predictionInfrequentProps.Optional(PredictionInfrequentProperties.RelativeRisk,
                                    new NameValuePair(
                                        new LocalizedLabel("risk-assessment.relativeRisk"),
                                        new ShowDecimal(),
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        direction: FlexDirection.Row
                                    )),
                                predictionInfrequentProps.Condition(PredictionInfrequentProperties.When,
                                    new NameValuePair(
                                        new LocalizedLabel("risk-assessment.when"),
                                        new OpenTypeElement(null, "when"), // Period | Range
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        direction: FlexDirection.Row
                                    )),
                                predictionInfrequentProps.Optional(PredictionInfrequentProperties.Rationale,
                                    new NameValuePair(
                                        new LocalizedLabel("risk-assessment.rationale"),
                                        new Text("@value"),
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        direction: FlexDirection.Row
                                    )),
                            ],
                            style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Column
                        ),
                    ];
                })),
            infrequentProperties.Optional(RiskAssessmentInfrequentProperties.Method,
                new NameValuePair(
                    new LocalizedLabel("risk-assessment.method"),
                    new CodeableConcept(),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            new If(_ => infrequentProperties.ContainsAnyOf(
                    RiskAssessmentInfrequentProperties.ReasonCode,
                    RiskAssessmentInfrequentProperties.ReasonReference
                ),
                new NameValuePair(
                    [new LocalizedLabel("risk-assessment.reasonCode")],
                    [
                        new CommaSeparatedBuilder("f:reasonCode", _ => [new CodeableConcept()]),
                        new Condition("f:reasonCode and f:reasonReference", new ConstantText(", ")),
                        new ConcatBuilder("f:reasonReference", _ => [new AnyReferenceNamingWidget()]),
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            infrequentProperties.Optional(RiskAssessmentInfrequentProperties.Performer,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        Direction = FlexDirection.Column,
                        Style = NameValuePair.NameValuePairStyle.Primary,
                        LabelOverride = new LocalizedLabel("risk-assessment.performer"),
                    }
                )
            ),
            infrequentProperties.Optional(RiskAssessmentInfrequentProperties.Subject,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        Direction = FlexDirection.Column,
                        Style = NameValuePair.NameValuePairStyle.Primary,
                        LabelOverride = new LocalizedLabel("risk-assessment.subject"),
                    }
                )),
            infrequentProperties.Optional(RiskAssessmentInfrequentProperties.Parent,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        Direction = FlexDirection.Column,
                        Style = NameValuePair.NameValuePairStyle.Primary,
                        LabelOverride = new LocalizedLabel("risk-assessment.parent"),
                    }
                )),
            infrequentProperties.Optional(RiskAssessmentInfrequentProperties.BasedOn,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        Direction = FlexDirection.Column,
                        Style = NameValuePair.NameValuePairStyle.Primary,
                        LabelOverride = new LocalizedLabel("risk-assessment.basedOn"),
                    }
                )),
            infrequentProperties.Condition(RiskAssessmentInfrequentProperties.Occurence,
                new NameValuePair(
                    new LocalizedLabel("risk-assessment.occurence"),
                    new OpenTypeElement(null, "occurence"), // decimal | Range
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            new If(_ => infrequentProperties.Contains(RiskAssessmentInfrequentProperties.Basis),
                new NameValuePair(
                    new LocalizedLabel("risk-assessment.basis"),
                    new ConcatBuilder("f:basis", _ => [new AnyReferenceNamingWidget()], new LineBreak()),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            infrequentProperties.Optional(RiskAssessmentInfrequentProperties.Mitigation,
                new NameValuePair([new LocalizedLabel("risk-assessment.mitigation")], [
                        new Text("@value"),
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            infrequentProperties.Optional(RiskAssessmentInfrequentProperties.Encounter,
                new HideableDetails(
                    new NameValuePair(
                        [new LocalizedLabel("node-names.Encounter")],
                        [new AnyReferenceNamingWidget()],
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    )
                )),
        ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1");

        var resultWidget = new Concat([
            new Row([
                    new Heading([
                        new Container([
                            new TextContainer(TextStyle.Bold,
                            [
                                new Choose([
                                    new When("f:code and f:condition", new Container([
                                        new ChangeContext("f:condition", new AnyReferenceNamingWidget()),
                                        new ConstantText(" - "),
                                        new ChangeContext("f:code", new CodeableConcept()),
                                    ], ContainerType.Span)),
                                    new When("f:condition and not(f:code)",
                                        new ChangeContext("f:condition", new AnyReferenceNamingWidget())),
                                    new When("f:code and not(f:condition)",
                                        new ChangeContext("f:code", new CodeableConcept())),
                                ], new LocalizedLabel("risk-assessment")),
                            ]),
                        ], optionalClass: "blue-color d-flex align-items-center"),
                    ], HeadingSize.H5, customClass: "m-0"),
                    new EnumIconTooltip("f:status", "http://hl7.org/fhir/observation-status",
                        new EhdsiDisplayLabel(LabelCodes.Status)),
                    new NarrativeModal(alignRight: false),
                ], flexContainerClasses: "gap-1 align-items-center", flexWrap: false,
                idSource: skipIdPopulation ? null : new IdentifierSource(navigator)),
            new FlexList([
                nameValuePairs,
                ThematicBreak.SurroundedThematicBreak(infrequentProperties, [
                    RiskAssessmentInfrequentProperties.Condition,
                    RiskAssessmentInfrequentProperties.Method,
                    RiskAssessmentInfrequentProperties.ReasonCode,
                    RiskAssessmentInfrequentProperties.ReasonReference,
                    RiskAssessmentInfrequentProperties.Performer,
                    RiskAssessmentInfrequentProperties.Subject,
                    RiskAssessmentInfrequentProperties.Parent,
                    RiskAssessmentInfrequentProperties.BasedOn,
                    RiskAssessmentInfrequentProperties.Prediction,
                    RiskAssessmentInfrequentProperties.Occurence,
                    RiskAssessmentInfrequentProperties.Basis,
                    RiskAssessmentInfrequentProperties.Mitigation,
                    RiskAssessmentInfrequentProperties.Encounter,
                ], [
                    RiskAssessmentInfrequentProperties.Note,
                    RiskAssessmentInfrequentProperties.Text,
                ]),
                new Condition("f:note", new NameValuePair([new LocalizedLabel("risk-assessment.note")],
                [
                    new CommaSeparatedBuilder("f:note",
                        _ => [new ShowAnnotationCompact()])
                ], style: NameValuePair.NameValuePairStyle.Secondary)),
                new Condition("f:text", new NarrativeCollapser()),
            ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1")
        ]);

        return resultWidget.Render(navigator, renderer, context);
    }

    public enum RiskAssessmentInfrequentProperties
    {
        Condition,
        Method,
        ReasonCode,
        ReasonReference,
        Performer,
        Parent,
        [HiddenRedundantSubjectDisplayType] Subject,
        BasedOn,
        Basis,
        Mitigation,
        Note,
        [HiddenInSimpleMode] Encounter,
        Text,
        [OpenType("occurence")] Occurence,
        Prediction,
    }

    public enum PredictionInfrequentProperties
    {
        Outcome,
        QualitativeRisk,
        RelativeRisk,
        Rationale,
        [OpenType("probability")] Probability,
        [OpenType("when")] When,
    }
}