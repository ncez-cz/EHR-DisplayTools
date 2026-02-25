using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Collapser(
    IList<Widget> title,
    IList<Widget> content,
    bool isCollapsed = false,
    IdentifierSource? idSource = null,
    IdentifierSource? visualIdSource = null,
    IList<Widget>? footer = null,
    string? customClass = null,
    IList<Widget>? iconPrefix = null,
    Severity? severity = null,
    List<Widget>? subtitle = null
)
    : CardBase(new Concat(title), new Concat(content), severity, subtitle, true, isCollapsed,
        customClass, null, iconPrefix, footer, idSource, visualIdSource);