using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class AllergiesAndIntolerancesWidget : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
    List<Widget> widgetTree = [
new ChangeContext("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$allergiesAndIntolerancesSectionCode]]", [
new Variable("act", "n1:entry/n1:act"), 
new Variable("observation", "$act/n1:templateId[@root='1.3.6.1.4.1.12559.11.10.1.3.1.3.16']/../n1:entryRelationship[@typeCode='SUBJ']/n1:observation/n1:templateId[@root='1.3.6.1.4.1.12559.11.10.1.3.1.3.17']/.."), 
new Variable("observationValue", "$observation/n1:value"), 
new Section("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$allergiesAndIntolerancesSectionCode]]", "The Allergies, Adverse Reactions, Alerts section is missing !", [
new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
new Variable("code", "$allergiesAndIntolerancesSectionCode"),
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
        new Choose([
            new When("not($act/@nullFlavor)", [
                new Table([
                    new TableBody([
                        new Choose([
                            new When("$observationValue/@code='no-known-allergies' or $observationValue/@code='no-allergy-info'", [
                                new TableRow([
                                    new TableCell([
                                            new WidgetWithVariables(new ShowEHdsiAbsentOrUnknownAllergyWidget(), [
                                                new Variable("node", "$observationValue"),
                                            ]), 
                                        ],
                                        TableCellType.Data, 5), 
                                ]), 
                            ]),
                        ], [
                            new TableRow([
                                new TableCell([
                                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                            new Variable("code", "'65'"),
                                        ]), 
                                    ],
                                    TableCellType.Header), 
                                new TableCell([
                                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                            new Variable("code", "'10'"),
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
                                            new Variable("code", "'155'"),
                                        ]), 
                                    ],
                                    TableCellType.Header), 
                                new TableCell([
                                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                            new Variable("code", "'123'"),
                                        ]), 
                                    ],
                                    TableCellType.Header), 
                                new TableCell([
                                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                            new Variable("code", "'156'"),
                                        ]), 
                                    ],
                                    TableCellType.Header), 
                                new TableCell([
                                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                            new Variable("code", "'157'"),
                                        ]), 
                                    ],
                                    TableCellType.Header), 
                                new TableCell([
                                        new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                            new Variable("code", "'158'"),
                                        ]), 
                                    ],
                                    TableCellType.Header), 
                                new ChangeContext("n1:entry/n1:act", new Widget40()),
                            ]), 
                        ]), 
                    ]), 
                ]), 
            ]),
        ], [
            new WidgetWithVariables(new ShowEHdsiNullFlavorWidget(), [
                new Variable("code", "$act/@nullFlavor"),
            ]), 
        ]), 
    ], true), 
]), 
], titleAbbreviations: SectionTitleAbbreviations.Allergies),
]),
];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}
