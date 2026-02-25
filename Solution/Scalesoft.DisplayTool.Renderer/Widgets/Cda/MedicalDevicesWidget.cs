using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class MedicalDevicesWidget : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
    List<Widget> widgetTree = [
new ChangeContext("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$medicalDevicesSectionCode]]", [
new Variable("medDevAct", "n1:entry/n1:supply"), 
new Variable("playingDeviceCode", "$medDevAct/n1:participant/n1:participantRole/n1:playingDevice/n1:code"), 
new Section("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$medicalDevicesSectionCode]]", "The Medical Devices and Implants section is missing !", [
new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
new Variable("code", "$medicalDevicesSectionCode"),
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
                new When("($playingDeviceCode/@code='no-known-devices' or $playingDeviceCode/@code='no-device-info')", [
                    new TableRow([
                        new TableCell([
                                new WidgetWithVariables(new ShowEHdsiAbsentOrUnknownDeviceWidget(), [
                                    new Variable("node", "$playingDeviceCode"),
                                ]), 
                            ],
                            TableCellType.Data, 2), 
                    ]), 
                ]),
            ], [
                new TableRow([
                    new TableCell([
                            new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                new Variable("code", "'21'"),
                            ]), 
                        ],
                        TableCellType.Header), 
                    new TableCell([
                            new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                new Variable("code", "'203'"),
                            ]), 
                        ],
                        TableCellType.Header), 
                    new TableCell([
                            new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
                                new Variable("code", "'36'"),
                            ]), 
                        ],
                        TableCellType.Header), 
                ]), 
                new ChangeContext("n1:entry/n1:supply", new Widget29()), 
            ]), 
        ]), 
    ], true), 
]), 
], titleAbbreviations: SectionTitleAbbreviations.HistoryOfMedicalDeviceUse),
]),
];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}
