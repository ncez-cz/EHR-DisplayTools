using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PlanOfCare;

/// <summary>
///     Renders the 'addresses' (Problems/Conditions) associated with a Care Plan as a Card.
/// </summary>
public class AddressesCard(XmlDocumentNavigator item) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator _,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var card = new Condition("f:addresses",
            new HideableDetails(new Card(new ConstantText("Problémy"),
                new ShowMultiReference("f:addresses", displayResourceType: false)))); // Problems

        return await card.Render(item, renderer, context);
    }
}