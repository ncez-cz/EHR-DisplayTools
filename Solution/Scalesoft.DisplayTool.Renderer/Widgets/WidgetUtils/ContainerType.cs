namespace Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

public enum ContainerType
{
    /// <summary>
    /// Will be set to span, unless the Container's children include a div - then it will be set to div.
    /// </summary>
    Auto,
    Paragraph,
    Span,
    Div,
}