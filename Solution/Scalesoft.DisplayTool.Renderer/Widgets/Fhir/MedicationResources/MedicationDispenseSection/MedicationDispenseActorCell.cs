using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicationResources.MedicationDispenseSection;

public class MedicationDispenseActorCell(XmlDocumentNavigator item) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions = InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([item]);

        var actorsTableCell = new TableCell(
        [
            infrequentOptions.Contains(InfrequentPropertiesPaths.Performer)
                ? new NameValuePair([new ConstantText("Žadatel")],
                [
                    new CommaSeparatedBuilder("f:performer",
                        (_, _, nav) =>
                        [
                            new Container([new AnyReferenceNamingWidget("f:actor")], ContainerType.Span, idSource: nav)
                        ])
                ], direction: FlexDirection.Column)
                : new NullWidget(),
            infrequentOptions.Contains(InfrequentPropertiesPaths.AuthorizingPrescription)
                ? new NameValuePair([new ConstantText("Identifikátor žádosti")],
                [
                    new CommaSeparatedBuilder("f:authorizingPrescription", _ => [new AnyReferenceNamingWidget()])
                ], direction: FlexDirection.Column)
                : new NullWidget()
        ]);

        if (infrequentOptions.Count == 0)
        {
            actorsTableCell = new TableCell([
                new TextContainer(TextStyle.Muted, [new ConstantText("Informace nejsou k dispozici")])
            ]);
        }

        return actorsTableCell.Render(item, renderer, context);
    }

    private enum InfrequentPropertiesPaths
    {
        Performer,
        AuthorizingPrescription
    }
}