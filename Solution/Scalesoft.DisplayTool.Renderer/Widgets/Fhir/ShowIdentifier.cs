using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowIdentifier(string path = ".", bool showSystem = true) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        //use, type, period, assigner are ignored if own layout want to be implemented
        var isIdentifier = navigator.EvaluateCondition(path);

        var isValue = navigator.EvaluateCondition($"{path}/f:value/@value");
        var isSystem = navigator.EvaluateCondition($"{path}/f:system/@value");

        if (!isIdentifier)
        {
            return new NullWidget().Render(navigator, renderer, context);
        }

        if (!isValue && !isSystem && isIdentifier)
        {
            if (context.RenderMode == RenderMode.Documentation)
            {
                return Task.FromResult<RenderResult>(navigator.GetFullPath());
            }

            return new TextContainer(
                TextStyle.Muted,
                [new ConstantText("Identifikátor je definován, ale nemá hodnotu ani systém")]
            ).Render(
                navigator,
                renderer,
                context
            );
        }

        var widgets = new List<Widget>();

        if (isValue)
        {
            widgets.Add(
                new TextContainer(TextStyle.Bold, [new Optional($"{path}/f:value", new Text("@value"))])
            );
            widgets.Add(new ConstantText(" "));
        }

        if (showSystem && isSystem)
        {
            widgets.Add(
                new HideableDetails(
                    new TextContainer(TextStyle.Muted, [new ConstantText("(")]),
                    new TextContainer(TextStyle.Muted, [new ConstantText("Systém: ")]),
                    new ConstantText(" "),
                    new TextContainer(TextStyle.Muted, [new Text($"{path}/f:system/@value")]),
                    new TextContainer(TextStyle.Muted, [new ConstantText(")")])
                )
            );
        }

        var output = new Container(widgets, ContainerType.Span);

        return output.Render(navigator, renderer, context);
    }
}