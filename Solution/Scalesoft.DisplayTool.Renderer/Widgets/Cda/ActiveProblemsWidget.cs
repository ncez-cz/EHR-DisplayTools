using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class ActiveProblemsWidget : Widget
{
  public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
      RenderContext context)
    {
    List<Widget> widgetTree = [
new ChangeContext("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$currentProblemsSectionCode]]", [
new Variable("problemCondition", "n1:entry/n1:act/n1:entryRelationship[@typeCode='SUBJ']/n1:observation/n1:templateId[@root='1.3.6.1.4.1.12559.11.10.1.3.1.3.7']/../n1:value"), 
new Section("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$currentProblemsSectionCode]]", "The Active Problem section is missing !", [
new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
new Variable("code", "$currentProblemsSectionCode"),
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
    new Table([
        new TableBody([
            new Choose([
                new When("($problemCondition/@code='no-known-problems' or $problemCondition/@code='no-problem-info')", [
                    new TableRow([
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiAbsentOrUnknownProblemWidget(), [
                                    new Variable("node", "$problemCondition"),
                                ]), 
                            ],
                            TableCellType.Data, 5), 
                    ]), 
                ]),
            ], [
                new TableRow([
                    new TableCell([
                            new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                new Variable("code", "'2'"),
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
                new ChangeContext("n1:entry/n1:act/n1:entryRelationship[@typeCode='SUBJ']/n1:observation/n1:value[@codeSystem='1.3.6.1.4.1.12559.11.10.1.3.1.44.2']", new Widget35()),
                new TableRow([
                    new TableCell([
                            new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                new Variable("code", "'198'"),
                            ]), 
                        ],
                        TableCellType.Header, 5), 
                ]), 
                new ChangeContext("n1:entry/n1:act/n1:entryRelationship[@typeCode='SUBJ']/n1:observation/n1:value[@codeSystem='1.3.6.1.4.1.12559.11.10.1.3.1.44.5']", new Widget34()),
            ]), 
        ]), 
    ], true), 
]), 
], titleAbbreviations: SectionTitleAbbreviations.ActiveProblems),
]), 
];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}
