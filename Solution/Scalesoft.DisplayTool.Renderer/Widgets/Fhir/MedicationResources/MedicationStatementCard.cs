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


    [UsedImplicitly]
    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator navigator)
    {
        var medicationCodeableConcept = navigator.SelectSingleNode("f:medicationCodeableConcept");
        if (medicationCodeableConcept.Node != null)
        {
            return new ResourceSummaryModel
            {
                Value = new ChangeContext(medicationCodeableConcept, new CodeableConcept()),
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
                    var widget = new ChangeContext(medicationResource.SelectSingleNode("f:code"),
                        new CodeableConcept());

                    return new ResourceSummaryModel
                    {
                        Value = widget,
                    };
                }
            }

            var medicationDisplays =
                ReferenceHandler.GetReferencesWithDisplayValue(navigator, "f:medicationReference");
            if (medicationDisplays.Count == 1)
            {
                var medicationDisplay = medicationDisplays.First();
                var display = medicationDisplay.SelectSingleNode("f:display/@value").Node?.Value;
                if (!string.IsNullOrEmpty(display))
                {
                    return new ResourceSummaryModel
                    {
                        Value = new ConstantText(display),
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
        var infrequentProperties = InfrequentProperties.Evaluate<MedicationStatementInfrequentProperties>([navigator]);

        var medicationStatementDetailsRow =
            new ChangeContext(navigator,
                new Optional("f:reasonCode",
                    new NameValuePair(
                        new DisplayLabel(LabelCodes.MedicationReason),
                        new CodeableConcept(),
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    )
                ),
                new If(
                    _ => infrequentProperties.Contains(
                        MedicationStatementInfrequentProperties.Effective),
                    new NameValuePair(
                        new DisplayLabel(LabelCodes.TreatmentDuration),
                        new Chronometry("effective"),
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    )
                )
            );

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
                                navigator
                            )
                        ),
                        new When("f:medicationReference",
                            new ShowSingleReference(x =>
                                {
                                    if (x.ResourceReferencePresent)
                                    {
                                        return
                                        [
                                            new MedicationBlock(
                                                new TextContainer(TextStyle.Bold,
                                                [
                                                    new Choose([
                                                        new When("f:code",
                                                            new ChangeContext("f:code", new CodeableConcept()))
                                                    ], new ConstantText("Lék"))
                                                ]),
                                                [
                                                    new MedicationStatementNameValuePairs(),
                                                    medicationStatementDetailsRow,
                                                ],
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
                            new DisplayLabel(LabelCodes.Status)),
                        new NarrativeModal(alignRight: false)
                    ),
                ], flexContainerClasses: "gap-1 align-items-center"),
                new FlexList([
                    new FlexList(medicationContent, FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1",
                        idSource: referenceNavigator),
                    new ChangeContext(item, new DosageCard(removeCardMargin: true))
                ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1")
            ]).Render(navigator, renderer, context);
        }
    }
}

public enum MedicationStatementInfrequentProperties
{
    [OpenType("medication")] Medication,
    Dosage,
    [OpenType("effective")] Effective,
    ReasonCode,

    [EnumValueSet("http://hl7.org/fhir/CodeSystem/medication-statement-status")]
    Status,
    Text
}