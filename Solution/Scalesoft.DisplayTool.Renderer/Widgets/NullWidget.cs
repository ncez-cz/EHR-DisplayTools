using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

/// <summary>
///     Special widget that is used for explicitly telling parent widgets that it should not be rendered
///     <example>
///         Widget is rendered in a list, and widget consists of one optional element.
///         Use null widget to prevent bullet point with empty space from being rendered.
///     </example>
/// </summary>
public class NullWidget : Widget
{
    public NullWidget()
    {
        IsNullWidget = true;
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        return Task.FromResult(RenderResult.NullResult);
    }
}