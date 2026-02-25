using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class SurgicalProceduresWidget : Widget
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
                "/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$surgicalProceduresSectionCode]]",
                new Variable("surgicalProcedure", "n1:entry/n1:procedure"),
                new Variable("surgicalProcedureCode", "$surgicalProcedure/n1:code"), new Section(
                    "/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$surgicalProceduresSectionCode]]",
                    "The History of Procedures section is missing !", [
                        new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
                            new Variable("code", "$surgicalProceduresSectionCode"),
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
                            new Choose([
                                new When("not($surgicalProcedure/@nullFlavor)", new Table([
                                    new TableBody([
                                        new Condition(
                                            "not ($surgicalProcedureCode/@code='no-known-procedures' or $surgicalProcedureCode/@code='no-procedure-info')",
                                            new TableRow([
                                                new TableCell([
                                                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                            new Variable("code", "'62'"),
                                                        ]),
                                                    ],
                                                    TableCellType.Header),
                                                new TableCell([
                                                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                            new Variable("code", "'154'"),
                                                        ]),
                                                    ],
                                                    TableCellType.Header),
                                                new TableCell([
                                                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                                            new Variable("code", "'63'"),
                                                        ]),
                                                    ],
                                                    TableCellType.Header),
                                            ])),
                                        new ChangeContext("n1:entry/n1:procedure", new Widget26()),
                                    ]),
                                ], true)),
                            ], new LineBreak(), new WidgetWithVariables(new ShowEHdsiNullFlavorWidget(), [
                                new Variable("code", "$surgicalProcedure/@nullFlavor"),
                            ])),
                        ]),
                    ], titleAbbreviations: SectionTitleAbbreviations.HistoryOfProcedures)),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}