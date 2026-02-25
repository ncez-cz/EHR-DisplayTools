using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class DisplayRepresentedOrganizationWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Container([
                new PlainBadge(
                    new EhdsiDisplayLabel(LabelCodes.RepresentedOrganization)
                    , Severity.Primary),
                new LineBreak(),
                new Choose([
                    new When("$representedOrganization/n1:name/@nullFlavor", new WidgetWithVariables(
                        new ShowEHdsiNullFlavorWidget(), [
                            new Variable("code", "$representedOrganization/n1:name/@nullFlavor"),
                        ])),
                ], new Choose([
                    new When("$representedOrganization/n1:name", new Text("$representedOrganization/n1:name")),
                ], new LocalizedLabel("general.unspecified"))),
            ]),

            new WidgetWithVariables(new ShowContactInfoWidget(), [
                new Variable("contact", "$representedOrganization"),
            ]),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}