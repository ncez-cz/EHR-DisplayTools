using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicationResources;

public class MedicationStatementCard : AlternatingBackgroundColumnResourceBase<MedicationStatementCard>, IResourceWidget
{
    public static string ResourceType => "MedicationStatement";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;


    public static bool HasBorderedContainer(Widget widget) => false;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator navigator)
    {
        if (navigator.EvaluateCondition("f:medicationCodeableConcept"))
        {
            return new ResourceSummaryModel
            {
                Value = new ChangeContext(navigator, "f:medicationCodeableConcept", new CodeableConcept()),
            };
        }

        if (navigator.EvaluateCondition("f:medicationReference"))
        {
            var medicationNavs = ReferenceHandler.GetReferencesWithContent(navigator, "f:medicationReference");
            if (medicationNavs.Count == 1)
            {
                var medicationResource = medicationNavs.First().Value;
                if (medicationResource.EvaluateCondition("f:code"))
                {
                    return new ResourceSummaryModel
                    {
                        Value = new TextContainer(TextStyle.Muted,
                            new ChangeContext(medicationResource.SelectSingleNode("f:code"), new CodeableConcept())),
                    };
                }
            }

            var medicationDisplays =
                ReferenceHandler.GetReferencesWithDisplayValue(navigator, "f:medicationReference");
            if (medicationDisplays.Count == 1)
            {
                var medicationDisplay = medicationDisplays.First();
                if (medicationDisplay.EvaluateCondition("f:display/@value"))
                {
                    return new ResourceSummaryModel
                    {
                        Value = new ChangeContext(medicationDisplay, new Text("f:display/@value")),
                    };
                }
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
        var infrequentProperties = InfrequentProperties.Evaluate<MedicationStatementInfrequentProperties>(navigator);

        var medicationStatementDetailsRow =
            new ChangeContext(navigator,
                new If(_ => infrequentProperties.ContainsAnyOf(
                        MedicationStatementInfrequentProperties.ReasonCode,
                        MedicationStatementInfrequentProperties.ReasonReference),
                    new NameValuePair(
                        [new EhdsiDisplayLabel(LabelCodes.MedicationReason)],
                        [
                            new CommaSeparatedBuilder("f:reasonCode",
                                _ => [new CodeableConcept()]),
                            new Condition("f:reasonCode and f:reasonReference", new ConstantText(", ")),
                            new ConcatBuilder("f:reasonReference",
                                _ => [new AnyReferenceNamingWidget()])
                        ],
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    )
                ),
                new If(
                    _ => infrequentProperties.Contains(
                        MedicationStatementInfrequentProperties.Effective),
                    new NameValuePair(
                        new EhdsiDisplayLabel(LabelCodes.TreatmentDuration),
                        new Chronometry("effective"),
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    )
                )
            );

        var medicationStatementDetailsRowAnyDisplayed = infrequentProperties.ContainsAnyOf(
            MedicationStatementInfrequentProperties.ReasonCode,
            MedicationStatementInfrequentProperties.ReasonReference, MedicationStatementInfrequentProperties.Effective);

        var medicationNameValuePairs =
            new Concat([
                new Optional("f:manufacturer",
                    new HideableDetails(new NameValuePair(
                        new LocalizedLabel("medication.manufacturer"),
                        new AnyReferenceNamingWidget(),
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    ))
                ),
                new Optional("f:code",
                    new NameValuePair(
                        new LocalizedLabel("medication.code"),
                        new CodeableConcept(),
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    )
                ),
                new Optional("f:form",
                    new NameValuePair(
                        new LocalizedLabel("medication.form"),
                        new CodeableConcept(),
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    )
                ),
                new Condition("f:ingredient",
                    new ConcatBuilder("f:ingredient", _ =>
                    [
                        new NameValuePair(
                            [new LocalizedLabel("medication.ingredient")],
                            [
                                new Condition("f:itemCodeableConcept or f:itemReference", new NameValuePair([
                                        new LocalizedLabel("medication.ingredient.item")
                                    ], [
                                        new CommaSeparatedBuilder("f:itemCodeableConcept",
                                            _ => [new CodeableConcept()]),
                                        new Condition("f:itemCodeableConcept and f:itemReference",
                                            new ConstantText(", ")),
                                        new CommaSeparatedBuilder("f:itemReference",
                                            _ => [new AnyReferenceNamingWidget()])
                                    ], direction: FlexDirection.Row,
                                    style: NameValuePair.NameValuePairStyle.Secondary)),
                                new Optional("f:strength",
                                    new NameValuePair([new LocalizedLabel("medication.ingredient.strength")],
                                        [new ShowRatio()], direction: FlexDirection.Row,
                                        style: NameValuePair.NameValuePairStyle.Secondary)),
                            ],
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Primary
                        )
                    ])
                ),
                new Optional("f:amount",
                    new HideableDetails(new NameValuePair(
                        new LocalizedLabel("medication.amount"),
                        new ShowRatio(),
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    ))
                ),
                new Optional("f:batch/f:lotNumber",
                    new HideableDetails(new NameValuePair(
                        new LocalizedLabel("medication.batch.lotNumber"),
                        new Text("@value"),
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    ))
                ),
                new Optional("f:batch/f:expirationDate",
                    new HideableDetails(new NameValuePair(
                        new LocalizedLabel("medication.batch.expirationDate"),
                        new ShowDateTime(),
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    ))
                ),
            ]);

        Widget[] output =
        [
            new FlexList(
                [
                    new Choose([
                        new When("f:medicationCodeableConcept",
                            new MedicationBlock(
                                new TextContainer(TextStyle.Bold,
                                    [new ChangeContext("f:medicationCodeableConcept", new CodeableConcept())]),
                                [medicationStatementDetailsRow],
                                medicationStatementDetailsRowAnyDisplayed,
                                navigator
                            )
                        ),
                        new When("f:medicationReference",
                            new ShowSingleReference(x =>
                                {
                                    if (x.ResourceReferencePresent)
                                    {
                                        var medicationInfrequentProperties =
                                            InfrequentProperties.Evaluate<MedicationInfrequestProperties>(x.Navigator);
                                        var medicationAnyDisplayed = medicationInfrequentProperties.ContainsAnyOf(
                                            MedicationInfrequestProperties.Manufacturer,
                                            MedicationInfrequestProperties.Form,
                                            MedicationInfrequestProperties.Ingredient,
                                            MedicationInfrequestProperties.Amount,
                                            MedicationInfrequestProperties.BatchLotNumber,
                                            MedicationInfrequestProperties.BatchExpirationDate);

                                        return
                                        [
                                            new MedicationBlock(
                                                new TextContainer(TextStyle.Bold,
                                                [
                                                    new Choose([
                                                        new When("f:code",
                                                            new ChangeContext("f:code", new CodeableConcept()))
                                                    ], new LocalizedLabel("medication-statement.medication"))
                                                ]),
                                                [
                                                    medicationNameValuePairs,
                                                    medicationStatementDetailsRow,
                                                ],
                                                medicationStatementDetailsRowAnyDisplayed || medicationAnyDisplayed,
                                                navigator,
                                                x.Navigator
                                            )
                                        ];
                                    }

                                    return
                                    [
                                        new MedicationBlock(
                                            new TextContainer(TextStyle.Bold,
                                                [new ConstantText(x.ReferenceDisplay)]),
                                            [medicationStatementDetailsRow],
                                            medicationStatementDetailsRowAnyDisplayed,
                                            navigator
                                        )
                                    ];
                                },
                                "f:medicationReference"
                            )
                        ),
                    ]),
                ], FlexDirection.Column, flexContainerClasses: "gap-1"
            )
        ];

        return output.RenderConcatenatedResult(navigator, renderer, context);
    }

    private class MedicationBlock(
        Widget heading,
        Widget[] medicationContent,
        bool anyDisplayedPropertyPresent,
        XmlDocumentNavigator item,
        XmlDocumentNavigator? referenceNavigator = null
    ) : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            return new Concat([
                new Row([
                    new Heading([
                        heading,
                    ], HeadingSize.H5, customClass: "m-0 blue-color"),
                    new ChangeContext(item,
                        new EnumIconTooltip("f:status",
                            "http://hl7.org/fhir/CodeSystem/medication-statement-status",
                            new EhdsiDisplayLabel(LabelCodes.Status)),
                        new NarrativeModal(alignRight: false)
                    ),
                ], flexContainerClasses: "gap-1 align-items-center", flexWrap: false),
                new FlexList([
                    new If(_ => anyDisplayedPropertyPresent, new FlexList(medicationContent, FlexDirection.Row,
                            flexContainerClasses: "column-gap-6 row-gap-1",
                            idSource: referenceNavigator), new ChangeContext(item, new DosageCard()))
                        .Else(
                            new NameValuePair([
                                    new LocalizedLabel("dosage.dosage-information")
                                ], [
                                    new ChangeContext(item, new DosageCard(bodyOnly: true))
                                ], direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary,
                                idSource: referenceNavigator
                            )),
                ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1")
            ]).Render(navigator, renderer, context);
        }
    }

    private enum MedicationStatementInfrequentProperties
    {
        [OpenType("medication")] Medication,
        Dosage,
        [OpenType("effective")] Effective,
        ReasonCode,
        ReasonReference,

        [EnumValueSet("http://hl7.org/fhir/CodeSystem/medication-statement-status")]
        Status,
        Text,
    }

    private enum MedicationInfrequestProperties
    {
        Manufacturer,
        Form,
        Ingredient,
        Amount,
        [PropertyPath("f:batch/f:lotNumber")] BatchLotNumber,

        [PropertyPath("f:batch/f:expirationDate")]
        BatchExpirationDate,
    }
}