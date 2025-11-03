using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Column(
    IEnumerable<Widget> children,
    string? childContainerClasses = null,
    string? flexContainerClasses = null,
    bool? wrapChildren = false,
    ContainerType containerType = ContainerType.Div,
    bool flexWrap = true
) : FlexList(children, FlexDirection.Column, childContainerClasses, flexContainerClasses ?? "gap-0", wrapChildren,
    containerType: containerType, flexWrap: flexWrap);