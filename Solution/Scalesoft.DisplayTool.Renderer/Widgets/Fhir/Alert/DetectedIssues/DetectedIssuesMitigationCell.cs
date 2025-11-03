using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert.DetectedIssues;

public class DetectedIssuesMitigationCell(XmlDocumentNavigator item) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var mitigations = new TableCell([
            new Choose([
                new When("f:mitigation",
                    new ConcatBuilder("f:mitigation", _ => [new MitigationWidget()], new LineBreak())
                )
            ], new TextContainer(TextStyle.Muted, [new ConstantText("Informace nejsou k dispozici")]))
        ]);

        return mitigations.Render(item, renderer, context);
    }

    private class MitigationWidget : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var infrequentOptions =
                InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([navigator]);

            Widget[] mitigationWidget =
            [
                new Container([
                    infrequentOptions.Contains(InfrequentPropertiesPaths.Action)
                        ? new NameValuePair([new ConstantText("Vykonána akce")],
                        [
                            new Optional("f:action", new CodeableConcept())
                        ], direction: FlexDirection.Column)
                        : new NullWidget(),
                    infrequentOptions.Contains(InfrequentPropertiesPaths.Date)
                        ? new NameValuePair([new DisplayLabel(LabelCodes.Date)],
                        [
                            new Optional("f:date", new ShowDateTime())
                        ])
                        : new NullWidget(),
                    infrequentOptions.Contains(InfrequentPropertiesPaths.Author)
                        ? new NameValuePair([new ConstantText("Autor opatření")],
                        [
                            new AnyReferenceNamingWidget("f:author")
                        ])
                        : new NullWidget()
                ], optionalClass: "name-value-pair-wrapper"),
            ];
            return mitigationWidget.RenderConcatenatedResult(navigator, renderer, context);
        }
    }

    private enum InfrequentPropertiesPaths
    {
        Action,
        Date,
        Author,
    }
}