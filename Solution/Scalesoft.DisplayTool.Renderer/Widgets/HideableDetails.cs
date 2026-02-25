using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class HideableDetails(
    ContainerType containerType,
    string? optionalClass,
    bool isOverridable = false,
    params Widget[] children
) : Widget
{
    public const string HideableDetailsClass = "optional-detail";
    public const string OverridableHideableDetailsClass = "optional-detail-overridable";

    public HideableDetails(ContainerType containerType, string? optionalClass, params Widget[] children) : this(
        containerType, optionalClass, false,
        children)
    {
    }

    public HideableDetails(ContainerType containerType, params Widget[] children) : this(containerType, null, false,
        children)
    {
    }

    public HideableDetails(params Widget[] children) : this(ContainerType.Auto, null, false, children)
    {
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        navigator = navigator.WithHideableContext();

        return new Container(children, containerType,
            optionalClass:
            $"{optionalClass} {(isOverridable ? OverridableHideableDetailsClass : HideableDetailsClass)}").Render(
            navigator,
            renderer,
            context
        );
    }
}