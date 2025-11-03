using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class FlexList : Widget
{
    private readonly IEnumerable<Widget> m_children;
    private readonly FlexDirection m_direction;
    private readonly string? m_childContainerClasses;
    private readonly string? m_flexContainerClasses;
    private readonly bool? m_wrapChildren;
    private readonly IdentifierSource? m_idSource;
    private readonly IdentifierSource? m_visualIdSource;
    private readonly ContainerType m_containerType;
    private readonly bool m_flexWrap;

    public FlexList(
        IEnumerable<Widget> children,
        FlexDirection direction,
        string? childContainerClasses = null,
        string? flexContainerClasses = null,
        bool? wrapChildren = false,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        ContainerType containerType = ContainerType.Div,
        bool flexWrap = true
    )
    {
        m_children = children;
        m_direction = direction;
        m_childContainerClasses = childContainerClasses;
        m_flexContainerClasses = flexContainerClasses;
        m_wrapChildren = wrapChildren;
        m_idSource = idSource;
        m_visualIdSource = visualIdSource ?? idSource;
        m_containerType = containerType;
        m_flexWrap = flexWrap;
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var internalResult = await RenderInternal(navigator, renderer, m_children, context);
        if (internalResult.IsFatal)
        {
            return internalResult.Errors;
        }

        var results = internalResult.GetValidContents();

        var viewModel = new ViewModel
        {
            Direction = m_direction,
            Children = results,
            CustomClass = m_flexContainerClasses,
            ChildContainerClasses = m_childContainerClasses,
            WrapChildren = m_wrapChildren,
            ContainerType = m_containerType,
            FlexWrap = m_flexWrap
        };
        HandleIds(context, navigator, viewModel, m_idSource, m_visualIdSource);
        var result = await renderer.RenderList(viewModel);
        return new RenderResult(result, internalResult.Errors);
    }

    public class ViewModel : ViewModelBase
    {
        public required FlexDirection Direction { get; init; }
        public string? ChildContainerClasses { get; set; }
        public bool? WrapChildren { get; set; }
        public required IEnumerable<string> Children { get; init; }

        public ContainerType ContainerType { get; set; } = ContainerType.Div;
        public bool FlexWrap { get; set; }
    }
}