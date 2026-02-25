using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class DisplayLegalAuthenticatorWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Collapser([
                new EhdsiDisplayLabel(LabelCodes.LegalAuthenticator)
            ], [
                new WidgetWithVariables(new DisplayAssignedPersonWidget(), [
                    new Variable("assignedPerson", "$legalAuthenticatorAssignedPerson"),
                    new Variable("contactInfoRoot", "n1:legalAuthenticator/n1:assignedEntity"),
                ]),
                new LineBreak(),
                new WidgetWithVariables(new DisplayRepresentedOrganizationWidget(), [
                    new Variable("representedOrganization", "$legalAuthenticatorRepresentedOrganization"),
                ]),
            ]),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}