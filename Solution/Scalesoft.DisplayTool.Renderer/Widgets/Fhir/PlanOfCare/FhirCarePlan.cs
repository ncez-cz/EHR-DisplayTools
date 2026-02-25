using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PlanOfCare;

public class FhirCarePlan(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "CarePlan";

    public static bool HasBorderedContainer(Widget widget) => false;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new FhirCarePlan(items)];
    }

    public static ResourceSummaryModel RenderSummary(XmlDocumentNavigator navigator)
    {
        Widget label = new ChangeContext(navigator,
            new EnumLabel("f:intent/@value", "http://hl7.org/fhir/request-intent"));

        if (navigator.EvaluateCondition("f:title"))
        {
            return new ResourceSummaryModel
            {
                Label = label,
                Value = new ChangeContext(navigator, new Text("f:title/@value")),
            };
        }

        if (navigator.EvaluateCondition("f:category"))
        {
            return new ResourceSummaryModel
            {
                Label = label,
                Value = new ChangeContext(navigator, "f:category", new CodeableConcept()),
            };
        }

        return new ResourceSummaryModel
        {
            Value = label, // Intent is required hence it's fallback value.
        };
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        // Process each top-level care plan item provided in the constructor
        var widgetsToRender = items.Select(carePlanNavigator =>
            new Container(
            [
                new CarePlanDetails(carePlanNavigator),
                new GoalsDetails(carePlanNavigator),
                new PlanAddresses(carePlanNavigator),
                new PlanActivities(carePlanNavigator),
            ], idSource: carePlanNavigator)).Cast<Widget>().ToList();

        // Render all collected widgets sequentially
        var finalRenderResult = await widgetsToRender.RenderConcatenatedResult(navigator, renderer, context);

        return finalRenderResult;
    }

    // Keep these utility methods as they're used by other widgets in the namespace
    public static Choose NarrativeAndOrChildren(IList<Widget> widgets)
    {
        return
            new Choose(
                [new When("f:text", new Tooltip(widgets, [new Narrative("f:text")]))],
                widgets.ToArray());
    }
}