using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class FunctionalStatusWidget : Widget
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
                "/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$functionalStatusSectionCode]]",
                new Section(
                    "/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$functionalStatusSectionCode]]",
                    null, [
                        new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
                            new Variable("code", "$functionalStatusSectionCode"),
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
                                                    new Variable("code", "'163'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                        new TableCell([
                                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                    new Variable("code", "'164'"),
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
                                                    new Variable("code", "'166'"),
                                                ]),
                                            ],
                                            TableCellType.Header),
                                    ]),
                                    new ChangeContext("n1:entry/n1:organizer/n1:component/n1:observation",
                                        new Widget16()),
                                ]),
                            ], true),
                        ]),
                    ], titleAbbreviations: SectionTitleAbbreviations.FunctionalStatus)),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}