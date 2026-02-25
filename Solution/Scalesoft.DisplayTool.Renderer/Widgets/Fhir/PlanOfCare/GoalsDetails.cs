using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PlanOfCare;

public class GoalsDetails(XmlDocumentNavigator item) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator _,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var card = new Condition(
            "f:goal",
            new HideableDetails(new Section(".", null, [new LocalizedLabel("care-plan.goal")],
            [
                new ShowMultiReference((navs, _) =>
                {
                    var widgets = navs
                        .Select(x =>
                            new ChangeContext(
                                x,
                                new Container(new Goals(), idSource: x)
                            )
                        )
                        .ToList<Widget>();
                    return [new AlternatingBackgroundColumn(widgets)];
                }, "f:goal")
            ]))
        );

        return await card.Render(item, renderer, context);
    }
}