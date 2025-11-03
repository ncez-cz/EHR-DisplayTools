using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.DocumentReference;

public class DocumentReferenceContext(List<XmlDocumentNavigator> items) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions = InfrequentProperties.Evaluate<InfrequentPropertiesPaths>(items);

        var table = new Table(
            [
                new TableHead([
                    new TableRow([
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Event),
                            new HideableDetails(new TableCell([new ConstantText("Událost")], TableCellType.Header))),
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Period),
                            new TableCell([new ConstantText("Období")], TableCellType.Header)),
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.PracticeSetting),
                            new HideableDetails(new TableCell([new ConstantText("Pracoviště")], TableCellType.Header))),
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.FacilityType),
                            new HideableDetails(new TableCell([new ConstantText("Typ zařízení")],
                                TableCellType.Header))),
                    ])
                ]),
                ..items.Select(x => new TableBody([new DocumentReferenceContextRowBuilder(x, infrequentOptions)])),
            ],
            true
        );

        return table.Render(navigator, renderer, context);
    }

    private class DocumentReferenceContextRowBuilder(
        XmlDocumentNavigator item,
        InfrequentPropertiesData<InfrequentPropertiesPaths> infrequentOptionsColumn
    ) : Widget
    {
        public override async Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var rowDetails = new StructuredDetails();
            if (item.EvaluateCondition("f:encounter"))
            {
                var encounterNarrative = ReferenceHandler.GetSingleNodeNavigatorFromReference(item,
                    "f:encounter", "f:text");

                rowDetails.AddCollapser(new ConstantText(Labels.Encounter),
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

            var tableRowContent = new List<Widget>
            {
                new If(_ => infrequentOptionsColumn.Contains(InfrequentPropertiesPaths.Event), new HideableDetails(
                    new TableCell([
                        new Optional("f:event", new ItemListBuilder(".", ItemListType.Unordered, _ =>
                        [
                            new CodeableConcept()
                        ]))
                    ]))),
                new If(_ => infrequentOptionsColumn.Contains(InfrequentPropertiesPaths.Period), new TableCell([
                    new Optional("f:period", new ShowPeriod())
                ])),
                new If(_ => infrequentOptionsColumn.Contains(InfrequentPropertiesPaths.FacilityType),
                    new HideableDetails(new TableCell([
                        new Optional("f:facilityType", new CodeableConcept())
                    ]))),
                new If(_ => infrequentOptionsColumn.Contains(InfrequentPropertiesPaths.PracticeSetting),
                    new HideableDetails(new TableCell([
                        new Optional("f:practiceSetting", new CodeableConcept())
                    ]))),
            };
            var result =
                await new TableRow(tableRowContent, rowDetails, idSource: item).Render(item, renderer, context);

            var isCode = item.EvaluateCondition("f:code");
            if (!isCode)
            {
                result.Errors.Add(ParseError.MissingValue(item.SelectSingleNode("f:code").GetFullPath()));
            }

            return result;
        }
    }

    private enum InfrequentPropertiesPaths
    {
        Encounter, //	0..*	Reference(Encounter | EpisodeOfCare)	Context of the document content
        Event, //	0..*	CodeableConcept	Main clinical acts documented
        Period, //0..1	Period	Time of service that is being documented
        FacilityType, //0..1	CodeableConcept	Kind of facility where patient was seen
        PracticeSetting, //0..1	CodeableConcept	Additional details about where the content was created (e.g. clinical specialty)
    }
}