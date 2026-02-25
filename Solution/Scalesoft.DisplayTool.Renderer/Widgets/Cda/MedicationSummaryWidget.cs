using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class MedicationSummaryWidget : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
    List<Widget> widgetTree = [
new ChangeContext("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$medicationSummarySectionCode]]", [
new Variable("medAct", "n1:entry/n1:act"), 
new Variable("medCode", "n1:entry/n1:substanceAdministration/n1:templateId[@root= '1.3.6.1.4.1.12559.11.10.1.3.1.3.4']/../n1:code"), 
new Section("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$medicationSummarySectionCode]]", "The Medication Summary section is missing !", [
new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
new Variable("code", "$medicationSummarySectionCode"),
]), 
], [
new Collapser([
    new Text("$originalNarrativeTableTitle")
    , 
], [
    new NarrativeText("n1:text", null), 
]), 
new LineBreak(), 
new Collapser([
    new Text("$translatedCodedTableTitle")
    , 
], [
    new Choose([
        new When("not($medAct/@nullFlavor)", [
            new Table([
                new Choose([
                    new When("$medCode/@code='no-known-medications' or $medCode/@code='no-medication-info'", [
                        new TableRow([
                            new TableCell([
                                    new Container([
                                        new WidgetWithVariables(new ShowEHdsiAbsentOrUnknownMedicationWidget(), [
                                            new Variable("node", "$medCode"),
                                        ]), 
                                    ], ContainerType.Span), 
                                    new LineBreak(), 
                                ],
                                TableCellType.Data, 8), 
                        ]), 
                    ]),
                ], [
                    new TableRow([
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'128'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'1'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'70'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'25'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'78'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'32'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'67'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'150'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'173'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                    ]), 
                    new ChangeContext("n1:entry/n1:substanceAdministration", new Widget32()),  
                ]), 
            ], true), 
        ]),
    ], [
        new WidgetWithVariables(new ShowEHdsiNullFlavorWidget(), [
            new Variable("code", "$medAct/@nullFlavor"),
        ]), 
    ]), 
]), 
], titleAbbreviations: SectionTitleAbbreviations.HistoryOfMedicationUse),
]), 
];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}
