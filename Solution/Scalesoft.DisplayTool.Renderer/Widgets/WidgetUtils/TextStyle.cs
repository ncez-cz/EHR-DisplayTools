namespace Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

[Flags]
public enum TextStyle
{
    None = 0,
    Regular = 1 << 0,
    Bold = 1 << 1,
    Italic = 1 << 2,
    Underlined = 1 << 3,
    Strike = 1 << 4,
    Preformatted = 1 << 5,
    Small = 1 << 6,
    Muted = 1 << 7,
    Capitalize = 1 << 8,
    Uppercase = 1 << 9,
    CapitalizeFirst = 1 << 10,
    Light = 1 << 11,
}