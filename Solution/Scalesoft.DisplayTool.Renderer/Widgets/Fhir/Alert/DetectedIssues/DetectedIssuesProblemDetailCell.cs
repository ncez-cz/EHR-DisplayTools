using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert.DetectedIssues;

public class DetectedIssuesProblemDetailCell(XmlDocumentNavigator item) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions = InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([item]);

        var tree = new TableCell([new Container([
                infrequentOptions.Contains(InfrequentPropertiesPaths.Code)
                ? new NameValuePair([new ConstantText("Kategorie")],
                [
                    new Optional("f:code", new CodeableConcept())
                ])
                : new NullWidget(),
            infrequentOptions.Contains(InfrequentPropertiesPaths.Detail)
                ? new NameValuePair([new ConstantText("Detail")],
                [
                    new Optional("f:detail", new Text("@value"))
                ], direction: FlexDirection.Column)
                : new NullWidget(),
            infrequentOptions.Contains(InfrequentPropertiesPaths.Implicated)
                ? new NameValuePair([new ConstantText("Související zdroje")],
                [
                    new ConcatBuilder("f:implicated", _ => [new ConstantText("- "), new AnyReferenceNamingWidget()],
                        new LineBreak())
                ], direction: FlexDirection.Column)
                : new NullWidget()
            ], optionalClass: "name-value-pair-wrapper"),
        ]);

        if (infrequentOptions.Count == 0)
        {
            tree = new TableCell([
                new TextContainer(TextStyle.Muted, [new ConstantText("Informace nejsou k dispozici")])
            ]);
        }

        return tree.Render(item, renderer, context);
    }

    private enum InfrequentPropertiesPaths
    {
        Code,
        Detail,
        Implicated
    }
}