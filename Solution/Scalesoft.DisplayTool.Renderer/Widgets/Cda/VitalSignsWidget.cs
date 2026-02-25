using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class VitalSignsWidget : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
    List<Widget> widgetTree = [
new ChangeContext("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$vitalSignsSectionCode]]", [
new Variable("systolicObservation", "n1:entry/n1:organizer/n1:templateId[@root='1.3.6.1.4.1.19376.1.5.3.1.4.13.1']/../n1:component/n1:observation/n1:templateId[@root='1.3.6.1.4.1.19376.1.5.3.1.4.13.2']/../n1:code[@code='8480-6']/.."), 
new Variable("diastolicObservation", "n1:entry/n1:organizer/n1:templateId[@root='1.3.6.1.4.1.19376.1.5.3.1.4.13.1']/../n1:component/n1:observation/n1:templateId[@root='1.3.6.1.4.1.19376.1.5.3.1.4.13.2']/../n1:code[@code='8462-4']/.."), 
new Variable("systolicDate", "$systolicObservation/n1:effectiveTime/@value"), 
new Variable("diastolicDate", "$diastolicObservation/n1:effectiveTime/@value"), 
new Variable("physicalFindingsOrganizerDate", "n1:entry/n1:organizer/n1:templateId[@root='1.3.6.1.4.1.19376.1.5.3.1.4.13.1']/../n1:effectiveTime"), 
new Section("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$vitalSignsSectionCode]]", null, [
new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
new Variable("code", "$vitalSignsSectionCode"),
]), 
], [
new Choose([
new When("(not($physicalFindingsOrganizerDate/@value = $systolicDate) or not($physicalFindingsOrganizerDate/@value = $diastolicDate))", [
new Table([
new TableRow([
new TableCell([
new Icon(SupportedIcons.TriangleExclamation), 
new WidgetWithVariables(new ShowEHdsiDisplayLabelWidget(), [
new Variable("code", "'151'"),
]), 
new LineBreak(), 
],
TableCellType.Data), 
]), 
], true), 
]),
], [
]), 
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
                            new Variable("code", "'17'"),
                        ]), 
                    ],
                    TableCellType.Header), 
                new TableCell([
                        new WidgetWithVariables(new ShowEHdsiBloodPressureWidget(), [
                            new Variable("code", "'8480-6'"),
                        ]), 
                    ],
                    TableCellType.Header), 
                new TableCell([
                        new WidgetWithVariables(new ShowEHdsiBloodPressureWidget(), [
                            new Variable("code", "'8462-4'"),
                        ]), 
                    ],
                    TableCellType.Header), 
            ]), 
            new ChangeContext("n1:entry/n1:organizer", new Widget5()), 
        ]), 
    ], true), 
]), 
], titleAbbreviations: SectionTitleAbbreviations.VitalSigns),
]), 
];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}
