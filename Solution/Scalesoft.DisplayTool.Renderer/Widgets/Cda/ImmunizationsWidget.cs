using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class ImmunizationsWidget : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
    List<Widget> widgetTree = [
new ChangeContext("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$immunizationsSectionCode]]", [
new Section("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$immunizationsSectionCode]]", null, [
new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
new Variable("code", "$immunizationsSectionCode"),
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
        new When("not (n1:entry/n1:substanceAdministration/@nullFlavor)", [
            new Table([
                new TableBody([
                    new TableRow([
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'79'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'9'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'80'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'5'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'124'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'152'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'169'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'170'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'171'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'172'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'122'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                    ]), 
                    new ChangeContext("n1:entry/n1:substanceAdministration", new Widget19()),
                ]), 
            ], true), 
        ]),
    ], [
        new WidgetWithVariables(new ShowEHdsiNullFlavorWidget(), [
            new Variable("code", "n1:entry/n1:substanceAdministration/@nullFlavor"),
        ]), 
    ]), 
]), 
], titleAbbreviations: SectionTitleAbbreviations.Immunizations),
]), 
];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}
