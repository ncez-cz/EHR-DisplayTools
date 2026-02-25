using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PlanOfCare;

/// <summary>
///     Renders the 'addresses' (Problems/Conditions) associated with a Care Plan as a Card.
/// </summary>
public class PlanAddresses(XmlDocumentNavigator item) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator _,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var card = new Condition("f:addresses",
            new HideableDetails(
                new Section(".", null,
                    [
                        new LocalizedLabel("care-plan.addresses")
                    ],
                    [
                        new ShowMultiReference(
                            (navs, groupName) =>
                            {
                                var widgets = navs
                                    .Select(x =>
                                        new ChangeContext(
                                            x,
                                            new Container(new ConditionResource(), idSource: x)
                                        )
                                    )
                                    .ToList<Widget>();

                                return [new AlternatingBackgroundColumn(widgets)];
                            }, "f:addresses"),
                    ]))); // Problems

        return await card.Render(item, renderer, context);
    }
}