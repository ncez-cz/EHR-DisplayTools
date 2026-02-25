using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class PregnancyHistoryWidget : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
    List<Widget> widgetTree = [
new ChangeContext("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$pregnancyHistorySectionCode]]", [
new Section("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$pregnancyHistorySectionCode]]", null, [
new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
new Variable("code", "$pregnancyHistorySectionCode"),
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
                new When("not(n1:entry/n1:observation/n1:code[@code='82810-3']/../@nullFlavor)", [
                    new TableRow([
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'174'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'175'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                    new Variable("code", "'176'"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiPregnancyInformationWidget(), [
                                    new Variable("node", "n1:entry/n1:observation/n1:code[@code='82810-3']/../n1:entryRelationship[@typeCode='COMP']/n1:observation/n1:code"),
                                ]), 
                            ],
                            TableCellType.Header), 
                        new ChangeContext("n1:entry/n1:observation/n1:code[@code='82810-3']", new Widget7()), 
                    ]), 
                ]),
            ], [
                new TableRow([
                    new TableCell([
                            new WidgetWithVariables(new ShowEHdsiNullFlavorWidget(), [
                                new Variable("code", "n1:entry/n1:observation/n1:code[@code='82810-3']/../@nullFlavor"),
                            ]), 
                        ],
                        TableCellType.Data), 
                ]), 
            ]), 
        ]), 
    ], true), 
    new Table([
        new TableBody([
            new TableRow([
                new TableCell([
                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                            new Variable("code", "'177'"),
                        ]), 
                    ],
                    TableCellType.Header), 
                new TableCell([
                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                            new Variable("code", "'178'"),
                        ]), 
                    ],
                    TableCellType.Header), 
                new TableCell([
                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                            new Variable("code", "'179'"),
                        ]), 
                    ],
                    TableCellType.Header), 
            ]), 
            new ChangeContext("n1:entry/n1:observation/n1:code[@code!='93857-1'][@code!='82810-3']", new Widget8()),  
        ]), 
    ], true), 
    new Table([
        new TableBody([
            new Choose([
                new When("not(n1:entry/n1:observation/n1:code[@code='93857-1']/../@nullFlavor)", [
                    new TableBody([
                        new TableRow([
                            new TableCell([
                                    new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                        new Variable("code", "'180'"),
                                    ]), 
                                ],
                                TableCellType.Header), 
                        ]), 
                        new ChangeContext("n1:entry/n1:observation/n1:code[@code='93857-1']", new Widget9()), 
                    ]), 
                ]),
            ], [
                new TableRow([
                    new TableCell([
                            new WidgetWithVariables(new ShowEHdsiNullFlavorWidget(), [
                                new Variable("code", "n1:entry/n1:observation/n1:code[@code='93857-1']/../@nullFlavor"),
                            ]), 
                        ],
                        TableCellType.Data), 
                ]), 
            ]), 
        ]), 
    ], true), 
]), 
], titleAbbreviations: SectionTitleAbbreviations.HistoryOfPregnancies),
]), 
];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}
