using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Cda;

public class ShowContactInfoWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new PlainBadge(new EhdsiDisplayLabel(LabelCodes.ContactInformation), Severity.Primary),
            new LineBreak(),
            new TextContainer(TextStyle.Bold | TextStyle.Small, [new EhdsiDisplayLabel(LabelCodes.Address)]),
            new LineBreak(),
            new Choose([
                new When("$contact/n1:addr", new ConcatBuilder("$contact/n1:addr", i =>
                [
                    new WidgetWithVariables(new ShowAddressWidget(), [
                        new Variable("address", "."),
                    ]),
                ])),
            ], new LocalizedLabel("general.unspecified")),
            new LineBreak(),
            new TextContainer(TextStyle.Bold | TextStyle.Small, [new EhdsiDisplayLabel(LabelCodes.Telecom)]),
            new LineBreak(),
            new Choose([
                new When("$contact/n1:telecom", new ConcatBuilder("$contact/n1:telecom", i =>
                [
                    new WidgetWithVariables(new ShowTelecomWidget(), [
                        new Variable("telecom", "."),
                    ]),
                ])),
            ], new LocalizedLabel("general.unspecified")),
        ];
        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}