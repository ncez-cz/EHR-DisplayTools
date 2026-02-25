using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class SocialHistoryWidget : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
    List<Widget> widgetTree = [
new ChangeContext("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$socialHistorySectionCode]]", [
new Section("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$socialHistorySectionCode]]", null, [
new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
new Variable("code", "$socialHistorySectionCode"),
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
            new TableRow([
                new TableCell([
                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                            new Variable("code", "'44'"),
                        ]), 
                    ],
                    TableCellType.Header), 
                new TableCell([
                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                            new Variable("code", "'155'"),
                        ]), 
                    ],
                    TableCellType.Header), 
                new TableCell([
                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                            new Variable("code", "'84'"),
                        ]), 
                    ],
                    TableCellType.Header), 
            ]), 
            new ChangeContext("n1:entry/n1:observation", new Widget11()),  
        ]), 
    ], true), 
]), 
], titleAbbreviations: SectionTitleAbbreviations.SocialHistory),
]), 
];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}
