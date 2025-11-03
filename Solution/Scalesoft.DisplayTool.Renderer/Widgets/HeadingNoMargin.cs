using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class HeadingNoMargin(
    Widget[] content,
    HeadingSize size = HeadingSize.H1,
    string customClass = "",
    IdentifierSource? idSource = null,
    IdentifierSource? visualIdSource = null
) : Heading(
    content, size, customClass + " mb-0", idSource, visualIdSource
);