using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ConditionResource(Widget resourceTypeLabel, bool skipIdPopulation = true)
    : AlternatingBackgroundColumnResourceBase<ConditionResource>, IResourceWidget
{
    public static string ResourceType => "Condition";
    public static bool HasBorderedContainer(Widget widget) => false;

    public ConditionResource() : this(new LocalizedLabel("condition"))
    {
    }

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator navigator)
    {
        if (navigator.EvaluateCondition("f:code"))
        {
            return new ResourceSummaryModel
            {
                Value = new ChangeContext(navigator, "f:code", new CodeableConcept()),
            };
        }

        if (navigator.EvaluateCondition("f:stage"))
        {
            var stageNode = navigator.SelectSingleNode("f:stage");

            if (stageNode.EvaluateCondition("f:type"))
            {
                return new ResourceSummaryModel
                {
                    Value = new ChangeContext(stageNode, "f:type", new CodeableConcept()),
                };
            }

            if (stageNode.EvaluateCondition("f:summary"))
            {
                return new ResourceSummaryModel
                {
                    Value = new ChangeContext(stageNode, "f:summary", new CodeableConcept()),
                };
            }
        }

        if (navigator.EvaluateCondition("f:evidence"))
        {
            var evidenceNode = navigator.SelectSingleNode("f:evidence");

            if (evidenceNode.EvaluateCondition("f:code"))
            {
                return new ResourceSummaryModel
                {
                    Value = new ChangeContext(evidenceNode, "f:code", new CodeableConcept()),
                };
            }
        }

        if (navigator.EvaluateCondition("f:bodySite"))
        {
            return new ResourceSummaryModel
            {
                Value = new ChangeContext(navigator, "f:bodySite", new CodeableConcept()),
            };
        }

        return null;
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<ConditionInfrequentProperties>(navigator);

        var resultWidget = new Concat(
            [
                new Row(
                    [
                        new Heading(
                            [
                                new Optional(
                                    "f:code",
                                    new TextContainer(
                                        TextStyle.Bold,
                                        new CodeableConcept(),
                                        optionalClass: "blue-color"
                                    ),
                                    new ConstantText(" "),
                                    new TextContainer(
                                        TextStyle.Regular,
                                        [
                                            new ConstantText("("), resourceTypeLabel, new ConstantText(")"),
                                        ],
                                        optionalClass: "text-gray-600"
                                    )
                                ).Else(
                                    new TextContainer(TextStyle.Bold, resourceTypeLabel, optionalClass: "blue-color")
                                ),
                            ],
                            HeadingSize.H5,
                            "m-0"
                        ),
                        new NarrativeModal(alignRight: false),
                    ],
                    flexContainerClasses: "gap-1 align-items-center",
                    idSource: skipIdPopulation ? null : new IdentifierSource(navigator),
                    flexWrap: false
                ),
                new Column(
                    [
                        new Row(
                            [
                                new If(
                                    _ => infrequentProperties.Contains(ConditionInfrequentProperties.Category),
                                    new HideableDetails(
                                        new NameValuePair(
                                            [new LocalizedLabel("condition.category")],
                                            [new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()])],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )
                                ),
                                new If(
                                    _ => infrequentProperties.Contains(ConditionInfrequentProperties.Onset),
                                    new HideableDetails(
                                        new NameValuePair(
                                            [new EhdsiDisplayLabel(LabelCodes.OnsetDate)],
                                            [new Chronometry("onset")],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )
                                ),
                                new If(
                                    _ => infrequentProperties.Contains(ConditionInfrequentProperties.Abatement),
                                    new HideableDetails(
                                        new NameValuePair(
                                            [new LocalizedLabel("condition.abatement")],
                                            [new Chronometry("abatement")],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )
                                ),
                                infrequentProperties.Optional(ConditionInfrequentProperties.BodySite,
                                    new HideableDetails(
                                        new NameValuePair(
                                            new EhdsiDisplayLabel(LabelCodes.BodySite),
                                            new CodeableConcept(),
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )
                                ),

                                infrequentProperties.Optional(ConditionInfrequentProperties.BodySiteExtension,
                                    new HideableDetails(
                                        new NameValuePair(
                                            new EhdsiDisplayLabel(LabelCodes.BodySite),
                                            ShowSingleReference.WithDefaultDisplayHandler(
                                                nav =>
                                                [
                                                    new Container(
                                                        [
                                                            new BodyStructure(
                                                                NameValuePair.NameValuePairStyle.Secondary
                                                            ),
                                                        ],
                                                        idSource: nav
                                                    ),
                                                ],
                                                "f:valueReference"
                                            ),
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )
                                ),
                                new If(
                                    _ => infrequentProperties.Contains(ConditionInfrequentProperties.ClinicalStatus),
                                    new HideableDetails(
                                        new NameValuePair(
                                            [new EhdsiDisplayLabel(LabelCodes.ClinicalStatus)],
                                            [
                                                new CommaSeparatedBuilder(
                                                    "f:clinicalStatus",
                                                    _ =>
                                                    [
                                                        new CodeableConceptIconTooltip(
                                                            new EhdsiDisplayLabel(LabelCodes.ClinicalStatus)
                                                        )
                                                    ]
                                                ),
                                            ],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )
                                ),
                                new If(
                                    _ => infrequentProperties.Contains(ConditionInfrequentProperties.Severity),
                                    new HideableDetails(
                                        new NameValuePair(
                                            [new EhdsiDisplayLabel(LabelCodes.Severity)],
                                            [
                                                new CommaSeparatedBuilder(
                                                    "f:severity",
                                                    _ =>
                                                    [
                                                        new CodeableConceptIconTooltip(
                                                            new EhdsiDisplayLabel(LabelCodes.Severity)
                                                        )
                                                    ]
                                                )
                                            ],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )
                                ),
                            ],
                            flexContainerClasses: "column-gap-6 row-gap-1"
                        ),
                        new Condition("f:text", new NarrativeCollapser()),
                    ],
                    flexContainerClasses: "px-2 gap-1"
                ),
            ]
        );

        return await resultWidget.Render(navigator, renderer, context);
    }
}

public enum ConditionInfrequentProperties
{
    Category,
    BodySite,

    [Extension("http://hl7.org/fhir/StructureDefinition/bodySite")]
    BodySiteExtension,
    [OpenType("onset")] Onset,
    [OpenType("abatement")] Abatement,
    [EnumValueSet("")] ClinicalStatus,
    [EnumValueSet("")] Severity,
    Text,
}