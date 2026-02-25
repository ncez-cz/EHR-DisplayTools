using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Coverages : AlternatingBackgroundColumnResourceBase<Coverages>, IResourceWidget
{
    public static string ResourceType => "Coverage";
    public static bool HasBorderedContainer(Widget widget) => false;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        // two key elements of the coverage resource are the beneficiary (in most cases the current patient) and the payor. Leave the beneficiary out of the summary
        if (item.EvaluateCondition("f:payor"))
        {
            var payorNavs = ReferenceHandler.GetContentFromReferences(item, "f:payor");
            var summaries = payorNavs.Select(ReferenceHandler.GetResourceSummary).WhereNotNull().ToList();
            if (summaries.Count > 0)
            {
                var joinedSummaries = summaries.Select(x => x.Value).Intersperse(new ConstantText(", ")).ToList();
                return new ResourceSummaryModel
                {
                    Value =
                        new Container([
                            new LocalizedLabel("coverage.beneficiary-label"), new ConstantText(": "),
                            ..joinedSummaries,
                        ], ContainerType.Span),
                };
            }
        }

        return null;
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<CoverageInfrequentProperties>(navigator);

        Widget[] output =
        [
            new Column(
            [
                new Row([
                    new Heading(
                    [
                        new TextContainer(TextStyle.Bold,
                            [new ChangeContext("f:payor", new AnyReferenceNamingWidget())]),
                    ], HeadingSize.H5, customClass: "m-0 blue-color"),
                    new EnumIconTooltip("f:status",
                        "http://hl7.org/fhir/ValueSet/fm-status",
                        new EhdsiDisplayLabel(LabelCodes.Status)),
                    new NarrativeModal(alignRight: false),
                ], flexWrap: false, flexContainerClasses: "align-items-center gap-2"),
                new Row([
                    new HideableDetails(
                        new AnyReferenceNamingWidget("f:beneficiary",
                            widgetModel: new ReferenceNamingWidgetModel
                            {
                                Type = ReferenceNamingWidgetType.NameValuePair,
                                LabelOverride = new LocalizedLabel("coverage.beneficiary"),
                                Direction = FlexDirection.Column,
                                Style = NameValuePair.NameValuePairStyle.Primary,
                            }
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.Identifier,
                        new NameValuePair(
                            new LocalizedLabel("coverage.identifier"),
                            new ShowIdentifier(),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Primary
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.Type,
                        new HideableDetails(
                            new NameValuePair(
                                new LocalizedLabel("coverage.costToBeneficiary.type"),
                                new CodeableConcept(),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.PolicyHolder,
                        new HideableDetails(
                            new AnyReferenceNamingWidget(
                                widgetModel: new ReferenceNamingWidgetModel
                                {
                                    Type = ReferenceNamingWidgetType.NameValuePair,
                                    LabelOverride = new LocalizedLabel("coverage.policyHolder"),
                                    Direction = FlexDirection.Column,
                                    Style = NameValuePair.NameValuePairStyle.Primary,
                                }
                            )
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.Subscriber,
                        new HideableDetails(
                            new AnyReferenceNamingWidget(
                                widgetModel: new ReferenceNamingWidgetModel
                                {
                                    Type = ReferenceNamingWidgetType.NameValuePair,
                                    LabelOverride = new LocalizedLabel("coverage.subscriber"),
                                    Direction = FlexDirection.Column,
                                    Style = NameValuePair.NameValuePairStyle.Primary,
                                }
                            )
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.SubscriberId,
                        new HideableDetails(
                            new NameValuePair(
                                new LocalizedLabel("coverage.subscriberId"),
                                new Text("@value"),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.Dependent,
                        new HideableDetails(
                            new NameValuePair(
                                new LocalizedLabel("coverage.dependent"),
                                new Text("@value"),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.Relationship,
                        new HideableDetails(
                            new NameValuePair(
                                new LocalizedLabel("coverage.relationship"),
                                new CodeableConcept(),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.Period,
                        new HideableDetails(
                            new NameValuePair(
                                new LocalizedLabel("coverage.period"),
                                new ShowPeriod(),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.Order,
                        new HideableDetails(
                            new NameValuePair(
                                new LocalizedLabel("coverage.order"),
                                new Text("@value"),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.Network,
                        new HideableDetails(
                            new NameValuePair(
                                new LocalizedLabel("coverage.network"),
                                new Text("@value"),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.Subrogation,
                        new HideableDetails(
                            new NameValuePair(
                                new LocalizedLabel("coverage.subrogation"),
                                new ShowBoolean(
                                    new EhdsiDisplayLabel(LabelCodes.No),
                                    new EhdsiDisplayLabel(LabelCodes.Yes)
                                ),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.Contract,
                        new HideableDetails(
                            new AnyReferenceNamingWidget(
                                widgetModel: new ReferenceNamingWidgetModel
                                {
                                    Type = ReferenceNamingWidgetType.NameValuePair,
                                    LabelOverride = new LocalizedLabel("coverage.contract"),
                                    Direction = FlexDirection.Column,
                                    Style = NameValuePair.NameValuePairStyle.Primary,
                                }
                            )
                        )
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.Class,
                        items =>
                        [
                            new HideableDetails(
                                new NameValuePair(
                                    new TextContainer(TextStyle.Bold, new LocalizedLabel("coverage.class")),
                                    new Container([
                                        new ListBuilder(items, FlexDirection.Row, (_, _) =>
                                        [
                                            new NameValuePair(
                                                [new ChangeContext("f:type", new CodeableConcept())],
                                                [new Text("f:value/@value")],
                                                direction: FlexDirection.Row
                                            ),
                                        ], flexContainerClasses: "flex-wrap gap-1"),
                                    ]),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary
                                )
                            ),
                        ]
                    ),
                    infrequentProperties.Optional(CoverageInfrequentProperties.CostToBeneficiary,
                        items =>
                        [
                            new HideableDetails(
                                new NameValuePair(
                                    new TextContainer(TextStyle.Bold,
                                        new LocalizedLabel("coverage.costToBeneficiary-plural")),
                                    new ListBuilder(items, FlexDirection.Column, (_, _) =>
                                    [
                                        new Row([
                                            new Optional("f:type",
                                                new NameValuePair([new LocalizedLabel("coverage.type")],
                                                    [new CodeableConcept()])),
                                            new NameValuePair([new LocalizedLabel("coverage.costToBeneficiary.value")],
                                                [new OpenTypeElement(null)]), // 	SimpleQuantity | Money
                                            new ListBuilder("f:exception", FlexDirection.Column, _ =>
                                            [
                                                new TextContainer(TextStyle.Regular, [
                                                    new LocalizedLabel("coverage.costToBeneficiary.exception"),
                                                    new Optional("f:period",
                                                        new LocalizedLabel(
                                                            "coverage.costToBeneficiary.exception.period"),
                                                        new ShowPeriod()),
                                                    new LocalizedLabel("coverage.costToBeneficiary.exception.type"),
                                                    new ChangeContext("f:type", new CodeableConcept()),
                                                ]),
                                            ], flexContainerClasses: "gap-0"),
                                        ], flexContainerClasses: "row-gap-0 column-gap-5"),
                                    ]),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary
                                )
                            ),
                        ]
                    ),
                ], flexContainerClasses: "column-gap-6 row-gap-1"),
            ], flexContainerClasses: "gap-1"),
        ];

        return output.RenderConcatenatedResult(navigator, renderer, context);
    }

    private enum CoverageInfrequentProperties
    {
        Identifier,
        Type,
        PolicyHolder,
        Subscriber,
        SubscriberId,
        Dependent,
        Relationship,
        Period,
        Class,
        Order,
        Network,
        CostToBeneficiary,
        Subrogation,
        Contract,
    }
}