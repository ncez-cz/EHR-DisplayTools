using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class CodeableConcept(string? preferredCodeSystemOverride = null) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (context.RenderMode == RenderMode.Documentation)
        {
            return Task.FromResult<RenderResult>(navigator.GetFullPath());
        }

        var text = navigator.SelectSingleNode("./f:text/@value").Node?.Value;

        var widgetTree = new Choose([
                new When("./f:text and not(./f:coding)",
                    new Text("./f:text/@value")
                ),
            ],
            new CommaSeparatedBuilder("./f:coding[*]",
                _ => [new Coding(text, preferredCodeSystemOverride: preferredCodeSystemOverride),])
        );

        return widgetTree.Render(navigator, renderer, context);
    }
}