using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Card(
    Widget? title,
    Widget body,
    Severity? severity = null,
    List<Widget>? subtitle = null,
    string? optionalClass = null,
    string? bodyOptionalClass = null,
    Widget? footer = null,
    IdentifierSource? idSource = null,
    IdentifierSource? visualIdSource = null
)
    : CardBase(title, body, severity, subtitle, optionalClass: optionalClass, bodyOptionalClass: bodyOptionalClass,
        footer: [footer ?? new NullWidget()],
        idSource: idSource,
        visualIdSource: visualIdSource);