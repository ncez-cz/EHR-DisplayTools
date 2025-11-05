using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ConditionResource(Widget resourceTypeLabel)
    : AlternatingBackgroundColumnResourceBase<ConditionResource>, IResourceWidget
{
    public static string ResourceType => "Condition";

    public ConditionResource() : this(new ConstantText("Problém"))
    {
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<ConditionInfrequentProperties>([navigator]);

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
                    flexContainerClasses: "gap-1 align-items-center"
                ),
                new Column(
                    [
                        new Row(
                            [
                                new If(
                                    _ => infrequentProperties.Contains(ConditionInfrequentProperties.Category),
                                    new HideableDetails(
                                        new NameValuePair(
                                            [new ConstantText("Typ problému")],
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
                                            [new DisplayLabel(LabelCodes.OnsetDate)],
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
                                            [new ConstantText("Datum vyřešení/remise")],
                                            [new Chronometry("abatement")],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )
                                ),
                                InfrequentProperties.Optional(
                                    infrequentProperties,
                                    ConditionInfrequentProperties.BodySite,
                                    new HideableDetails(
                                        new NameValuePair(
                                            new DisplayLabel(LabelCodes.BodySite),
                                            new CodeableConcept(),
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )
                                ),

                                InfrequentProperties.Optional(
                                    infrequentProperties,
                                    ConditionInfrequentProperties.BodySiteExtension,
                                    new HideableDetails(
                                        new NameValuePair(
                                            new DisplayLabel(LabelCodes.BodySite),
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
                                            [new DisplayLabel(LabelCodes.ClinicalStatus)],
                                            [
                                                new CommaSeparatedBuilder(
                                                    "f:clinicalStatus",
                                                    _ =>
                                                    [
                                                        new CodeableConceptIconTooltip(
                                                            new DisplayLabel(LabelCodes.ClinicalStatus)
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
                                            [new DisplayLabel(LabelCodes.Severity)],
                                            [
                                                new CommaSeparatedBuilder(
                                                    "f:severity",
                                                    _ =>
                                                    [
                                                        new CodeableConceptIconTooltip(
                                                            new DisplayLabel(LabelCodes.Severity)
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