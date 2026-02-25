using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class DisplayPreferredHcpAndLegalOrganizationWidget : Widget
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
                new EhdsiDisplayLabel(LabelCodes.PreferredProvider)
            ], [
                new ConcatBuilder("$participantPRS", i =>
                [
                    new WidgetWithVariables(new DisplayAssignedPersonWidget(), [
                        new Variable("assignedPerson", "n1:associatedPerson"),
                        new Variable("contactInfoRoot", "."),
                    ]),
                    new LineBreak(),
                    new WidgetWithVariables(new DisplayRepresentedOrganizationWidget(), [
                        new Variable("representedOrganization", "../n1:scopingOrganization"),
                    ]),
                ]),
            ]),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}