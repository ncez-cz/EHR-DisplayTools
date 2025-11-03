using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Procedures(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "Procedure";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new Procedures(items)];
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions =
            InfrequentProperties.Evaluate<InfrequentPropertiesPaths>(items);

        List<Widget> headerRow =
        [
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Code),
                new TableCell([new DisplayLabel(LabelCodes.Procedure)], TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Performed),
                new TableCell([new DisplayLabel(LabelCodes.ProcedureDate)], TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.BodySite),
                new TableCell([new DisplayLabel(LabelCodes.BodySite)], TableCellType.Header)
            ),
            new If(
                _ => infrequentOptions.ContainsAnyOf(InfrequentPropertiesPaths.ReasonCode,
                    InfrequentPropertiesPaths.ReasonReference),
                new TableCell([new ConstantText("Důvod")], TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Status),
                new HideableDetails(new TableCell([new DisplayLabel(LabelCodes.Status)], TableCellType.Header))
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Text),
                new NarrativeCell(false, TableCellType.Header)
            )
        ];

        var table = new Table(
            [
                new TableHead([
                    new TableRow(headerRow)
                ]),
                ..items.Select(x => new TableBody([new ProcedureRow(x, infrequentOptions)])),
            ],
            true
        );

        return table.Render(navigator, renderer, context);
    }

    private class ProcedureRow(
        XmlDocumentNavigator item,
        InfrequentPropertiesData<InfrequentPropertiesPaths> parentInfrequentOptions
    )
        : Widget
    {
        public override async Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var infrequentOptions =
                InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([item]);

            var collapsibleRow = new StructuredDetails();

            var devices = ReferenceHandler.GetContentFromReferences(item, "f:focalDevice/f:manipulated");
            if (devices.Any())
            {
                collapsibleRow.AddCollapser(new ConstantText("Zařízení"), new ShowDevice(devices));
            }

            if (item.EvaluateCondition("f:encounter"))
            {
                var encounterNarrative = ReferenceHandler.GetSingleNodeNavigatorFromReference(item,
                    "f:encounter", "f:text");

                collapsibleRow.AddCollapser(
                    new ConstantText(Labels.Encounter),
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
                );
            }

            if (item.EvaluateCondition("f:text"))
            {
                collapsibleRow.AddCollapser(new DisplayLabel(LabelCodes.OriginalNarrative), new Narrative("f:text"));
            }

            var tableCells = new List<Widget>
            {
                new If(_ => parentInfrequentOptions.Contains(InfrequentPropertiesPaths.Code),
                    new TableCell([
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Code),
                            new CommaSeparatedBuilder("f:code", _ => [new CodeableConcept()])
                        ),
                    ])
                ),
                new If(_ => parentInfrequentOptions.Contains(InfrequentPropertiesPaths.Performed),
                    new TableCell([
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Performed),
                            new OpenTypeElement(null, "performed") // dateTime | Period | string | Age | Range
                        ),
                    ])
                ),
                new If(_ => parentInfrequentOptions.Contains(InfrequentPropertiesPaths.BodySite),
                    new TableCell([
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.BodySite),
                            new Optional("f:bodySite", new CodeableConcept()), new LineBreak())
                    ])),
                new If(_ => parentInfrequentOptions.Contains(InfrequentPropertiesPaths.ReasonCode) ||
                            parentInfrequentOptions.Contains(InfrequentPropertiesPaths.ReasonReference),
                    new TableCell([
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.ReasonCode),
                            new ItemListBuilder("f:reasonCode", ItemListType.Unordered,
                                _ => [new CodeableConcept()]), new LineBreak()),
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.ReasonReference),
                            new NameValuePair([new ConstantText("Odkaz na důvod")],
                            [
                                new ItemListBuilder("f:reasonReference", ItemListType.Unordered,
                                    _ => [new AnyReferenceNamingWidget()])
                            ]))
                    ])
                ),
                new If(_ => parentInfrequentOptions.Contains(InfrequentPropertiesPaths.Status),
                    new HideableDetails(new TableCell([
                        new Optional("f:status",
                            new EnumIconTooltip("@value", "http://hl7.org/fhir/ValueSet/event-status",
                                new DisplayLabel(LabelCodes.Status))),
                    ]))
                ),
                new If(_ => parentInfrequentOptions.Contains(InfrequentPropertiesPaths.Text),
                    new NarrativeCell()
                )
            };

            var result = await new TableRow(tableCells, collapsibleRow, idSource: item).Render(item, renderer, context);

            return result;
        }
    }

    private enum InfrequentPropertiesPaths
    {
        Code, // 0..*	CodeableConcept	Procedure code

        [OpenType("performed")]
        Performed, // 0..1 DateTime | Period | String | Age | Range When the procedure was performed
        ReasonCode, //0..*	CodeableConcept	Why the procedure was performed (code)
        ReasonReference, //	0..*	Reference(Condition (HDR) | Observation | Procedure (HDR) | DiagnosticReport | DocumentReference)	Why the procedure was performed (details)
        BodySite,

        [EnumValueSet("http://hl7.org/fhir/ValueSet/event-status")]
        Status,

        Text
    }
}