using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class HealthMaintenanceCarePlanWidget : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
    List<Widget> widgetTree = [
new ChangeContext("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$healthMaintenanceCarePlanSectionCode]]", [
new Section("/n1:ClinicalDocument/n1:component/n1:structuredBody/n1:component/n1:section[n1:code[@code=$healthMaintenanceCarePlanSectionCode]]", null, [
new WidgetWithVariables(new ShowEHdsiSectionWidget(), [
new Variable("code", "$healthMaintenanceCarePlanSectionCode"),
]), 
], [
new Collapser([
    new Text("$originalNarrativeTableTitle")
    , 
], [
    new NarrativeText("n1:text", null), 
]), 
], titleAbbreviations: SectionTitleAbbreviations.HealthMaintenanceCarePlan),
]), 
];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}
