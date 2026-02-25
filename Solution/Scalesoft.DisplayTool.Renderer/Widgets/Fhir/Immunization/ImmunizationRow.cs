using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Person;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Immunization;

public class ImmunizationRow(
    InfrequentPropertiesData<Immunizations.InfrequentPropertiesPaths> infrequentOptions,
    InfrequentPropertiesData<Immunizations.InfrequentProtocolPropertiesPaths> infrequentProtocolOptions,
    int reactionIndex = 1,
    int totalCount = 1
) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        navigator.SetVariable("protocolApplied", $"f:protocolApplied[{reactionIndex}]");

        var collapsibleRow = new StructuredDetails();

        if (navigator.EvaluateCondition(
                "f:extension[@url='http://hl7.org/fhir/5.0/StructureDefinition/extension-Immunization.basedOn']/f:valueReference"))
        {
            collapsibleRow.Add(
                new CollapsibleDetail(
                    new LocalizedLabel("immunization.basedOn"),
                    new Container([
                            new ChangeContext(
                                "f:extension[@url='http://hl7.org/fhir/5.0/StructureDefinition/extension-Immunization.basedOn']/f:valueReference",
                                ShowSingleReference.WithDefaultDisplayHandler(nav =>
                                    [new Container([new ImmunizationRecommendation()], idSource: nav)]))
                        ],
                        idSource: navigator.SelectSingleNode(
                            "f:extension[@url='http://hl7.org/fhir/5.0/StructureDefinition/extension-Immunization.basedOn']"))
                )
            );
        }

        if (navigator.EvaluateCondition(
                "f:extension[@url='http://hl7.eu/fhir/hdr/StructureDefinition/immunization-administeredProduct']"))
        {
            collapsibleRow.Add(
                new CollapsibleDetail(
                    new LocalizedLabel("immunization.administeredProduct"),
                    new Container([
                            new ChangeContext(
                                "f:extension[@url='http://hl7.eu/fhir/hdr/StructureDefinition/immunization-administeredProduct']",
                                new Optional("f:extension[@url='concept']/f:valueCodeableConcept",
                                    new Container([new CodeableConcept()],
                                        idSource: navigator.SelectSingleNode("f:extension[@url='concept']"))),
                                new Optional("f:extension[@url='reference']/f:valueReference", new Container([
                                    ShowSingleReference.WithDefaultDisplayHandler(nav =>
                                    [
                                        new Container([
                                            new Medication(),
                                        ], idSource: nav)
                                    ])
                                ], idSource: navigator.SelectSingleNode("f:extension[@url='reference']"))))
                        ],
                        idSource: navigator.SelectSingleNode(
                            "f:extension[@url='http://hl7.eu/fhir/hdr/StructureDefinition/immunization-administeredProduct']"))
                )
            );
        }

        if (navigator.EvaluateCondition("f:manufacturer"))
        {
            collapsibleRow.Add(
                new CollapsibleDetail(
                    new LocalizedLabel("immunization.manufacturer"),
                    new Container([
                        new ChangeContext("f:manufacturer", new ShowSingleReference(x =>
                        {
                            if (x.ResourceReferencePresent)
                            {
                                return
                                [
                                    new Container([new PersonOrOrganization(x.Navigator)], idSource: x.Navigator)
                                ];
                            }

                            return [new ConstantText(x.ReferenceDisplay)];
                        }))
                    ])
                )
            );
        }

        if (navigator.EvaluateCondition("f:encounter"))
        {
            var encounterNarrative = ReferenceHandler.GetSingleNodeNavigatorFromReference(navigator,
                "f:encounter", "f:text");

            collapsibleRow.Add(
                new CollapsibleDetail(
                    new LocalizedLabel("node-names.Encounter"),
                    ShowSingleReference.WithDefaultDisplayHandler(nav => [new EncounterCard(nav, false, false)],
                        "f:encounter"),
                    encounterNarrative != null
                        ?
                        [
                            new NarrativeCollapser(encounterNarrative.GetFullPath())
                        ]
                        : null,
                    encounterNarrative != null
                        ? new NarrativeModal(encounterNarrative.GetFullPath())
                        : null
                )
            );
        }

        if (navigator.EvaluateCondition("f:text"))
        {
            collapsibleRow.Add(
                new CollapsibleDetail(
                    new EhdsiDisplayLabel(LabelCodes.OriginalNarrative),
                    new Narrative("f:text")
                )
            );
        }

        var protocolNav = navigator.SelectSingleNode("$protocolApplied");
        List<Widget> rowCells;

        if (reactionIndex == 1)
        {
            rowCells =
            [
                new TableCell([
                    new ChangeContext("f:vaccineCode", new CodeableConcept()),
                ]),
                new TableCell([
                    new Chronometry("occurrence"),
                ]),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.ExpirationDate),
                    new TableCell([new Optional("f:expirationDate", new ShowDateTime())])
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Route),
                    new TableCell([new Optional("f:route", new CodeableConcept())])
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Site),
                    new TableCell([new Optional("f:site", new CodeableConcept())])
                ),
                new If(
                    _ => infrequentProtocolOptions.Contains(Immunizations.InfrequentProtocolPropertiesPaths
                        .TargetDisease),
                    new TableCell(
                        [new CommaSeparatedBuilder("$protocolApplied/f:targetDisease", _ => [new CodeableConcept()])],
                        idSource: protocolNav.Node == null ? (IdentifierSource?)null : protocolNav)
                ),
                new If(
                    _ => infrequentProtocolOptions.Contains(Immunizations.InfrequentProtocolPropertiesPaths.DoseNumber),
                    new TableCell(protocolNav.Node == null
                        ? []
                        :
                        [
                            new ChangeContext(protocolNav, new IntOrString("doseNumber"),
                                new Optional("f:seriesDosesPositiveInt|f:seriesDosesString",
                                    new ConstantText(" / "),
                                    new Text("@value")
                                )
                            ),
                        ], visualIdSource: protocolNav.Node == null ? (IdentifierSource?)null : protocolNav)
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.LotNumber),
                    new TableCell([new Optional("f:lotNumber", new Text("@value"))])
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Location),
                    new TableCell([
                        new Optional("f:location", new AnyReferenceNamingWidget()),
                    ])
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Performer),
                    new TableCell([
                        // Exclude ordering practitioner from performer column
                        new CommaSeparatedBuilder("f:performer[not(f:function/f:coding/f:code/@value='OP')]/f:actor",
                            _ =>
                            [
                                new AnyReferenceNamingWidget()
                            ])
                    ])
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.IsSubpotent),
                    new TableCell([
                        new ShowBoolean(new EhdsiDisplayLabel(LabelCodes.No), new EhdsiDisplayLabel(LabelCodes.Yes),
                            "f:isSubpotent"),
                    ])
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Status),
                    new TableCell([
                        new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/immunization-status",
                            new EhdsiDisplayLabel(LabelCodes.Administered))
                    ])
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Text),
                    new NarrativeCell()
                ),
            ];

            // implicitRules just contains a URL to a set of rules, and has little value to the end user
        }
        else
        {
            rowCells =
            [
                new TableCell([]),
                new TableCell([]),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.ExpirationDate),
                    new TableCell([]) // f:expirationDate
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Route),
                    new TableCell([]) // f:route
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Site),
                    new TableCell([]) // f:site
                ),
                new If(
                    _ => infrequentProtocolOptions.Contains(Immunizations.InfrequentProtocolPropertiesPaths
                        .TargetDisease),
                    new TableCell(
                        [new CommaSeparatedBuilder("$protocolApplied/f:targetDisease", _ => [new CodeableConcept()])],
                        visualIdSource: protocolNav)
                ),
                new If(
                    _ => infrequentProtocolOptions.Contains(Immunizations.InfrequentProtocolPropertiesPaths.DoseNumber),
                    new TableCell([
                        new ChangeContext(protocolNav, new IntOrString("doseNumber"),
                            new Optional("f:seriesDosesPositiveInt|f:seriesDosesString",
                                new ConstantText(" / "),
                                new Text("@value")
                            )
                        ),
                    ], visualIdSource: protocolNav)
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.LotNumber),
                    new TableCell([]) // f:lotNumber
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Location),
                    new TableCell([]) // f:location
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Performer),
                    new TableCell([]) // f:performer
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.IsSubpotent),
                    new TableCell([])
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Status),
                    new TableCell([])
                ),
                new If(_ => infrequentOptions.Contains(Immunizations.InfrequentPropertiesPaths.Text),
                    new NarrativeCell(false)
                ),
            ];

            // implicitRules just contains a URL to a set of rules, and has little value to the end user
        }

        var tree = new TableRow(rowCells, reactionIndex == totalCount ? collapsibleRow : null);

        var result = await tree.Render(navigator, renderer, context);
        return result;
    }
}