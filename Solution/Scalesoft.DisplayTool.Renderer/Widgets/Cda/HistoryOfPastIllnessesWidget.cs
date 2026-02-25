using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class HistoryOfPastIllnessesWidget : Widget
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
                "/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$historyOfPastIllnessesSectionCode]]",
                new Section(
                    "/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$historyOfPastIllnessesSectionCode]]",
                    null, [
                        new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
                            new Variable("code", "$historyOfPastIllnessesSectionCode"),
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
                                new TableBody([
                                    new TableRow([
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'11'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'45'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'26'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'167'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'199'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'200'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'201'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                    ]),
                                    new ChangeContext(
                                        "n1:entry/n1:act/n1:entryRelationship[@typeCode='SUBJ']/n1:observation/n1:value[@codeSystem='1.3.6.1.4.1.12559.11.10.1.3.1.44.2']",
                                        new Widget22()),
                                    new TableRow([
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'198'"),
                                                ]),
                                            ],
                                            TableCellType.Header, 7),
                                    ]),
                                    new ChangeContext(
                                        "n1:entry/n1:act/n1:entryRelationship[@typeCode='SUBJ']/n1:observation/n1:value[@codeSystem='1.3.6.1.4.1.12559.11.10.1.3.1.44.5']",
                                        new Widget21()),
                                ]),
                            ], true),
                        ]),
                    ], titleAbbreviations: SectionTitleAbbreviations.HistoryOfPastIllness)),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}