using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class HideableDetails(ContainerType containerType, string? optionalClass, params Widget[] children) : Widget
{

    public HideableDetails(ContainerType containerType, params Widget[] children) : this(containerType, null, children)
    {
    }
    
    public HideableDetails(params Widget[] children) : this(ContainerType.Auto, null, children)
    {
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        navigator = navigator.WithHideableContext();

        return new Container(children, containerType, optionalClass: $"{optionalClass} optional-detail").Render(
            navigator,
            renderer,
            context
        );
    }
}