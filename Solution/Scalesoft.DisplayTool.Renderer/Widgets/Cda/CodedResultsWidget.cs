using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class CodedResultsWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new ChangeContext(
                "/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$codedResultsSectionCode]]",
                new Section(
                    "/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$codedResultsSectionCode]]",
                    null, [
                        new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
                            new Variable("code", "$codedResultsSectionCode"),
                        ]),
                    ], [
                        new Collapser([
                            new Text("$originalNarrativeTableTitle"),
                        ], [
                            new NarrativeText("n1:text", null),
                        ]),
                        new LineBreak(),
                        new Collapser([
                            new Text("$translatedCodedTableTitle"),
                        ], [
                            new Table([
                                new TableHead([
                                    new TableRow([
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'118'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'159'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'160'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'161'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'162'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                    ]),
                                ]),
                                new TableBody([
                                    new Choose([
                                            new When("n1:entry/@nullFlavor", new TableRow([
                                                new TableCell([
                                                        new WidgetWithVariables(new ShowEHdsiNullFlavorWidget(), [
                                                            new Variable("code", "n1:entry/@nullFlavor"),
                                                        ]),
                                                    ],
                                                    TableCellType.Data, 2),
                                            ])),
                                        ], new ChangeContext("n1:entry/n1:observation", new Widget37()),
                                        new ChangeContext("n1:entry/n1:organizer/n1:component/n1:observation",
                                            new Widget38())),
                                ]),
                            ]),
                        ]),
                    ], titleAbbreviations: SectionTitleAbbreviations.RelevantDiagnosticTests)),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}