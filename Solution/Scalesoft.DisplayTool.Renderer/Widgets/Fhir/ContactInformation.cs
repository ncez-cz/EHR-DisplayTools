using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ContactInformation(
    string addressPath = "f:address",
    string telecomPath = "f:telecom"
) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> contact =
        [
            new If(_ => navigator.EvaluateCondition(addressPath) ||
                        navigator.EvaluateCondition(telecomPath),
                new PlainBadge(new DisplayLabel(LabelCodes.ContactInformation)),
                new Container([
                    new Address(addressPath),
                    new ShowContactPoint(telecomPath),
                ], optionalClass: "name-value-pair-wrapper")
            ),
        ];

        return await contact.RenderConcatenatedResult(navigator, renderer, context);
    }
}