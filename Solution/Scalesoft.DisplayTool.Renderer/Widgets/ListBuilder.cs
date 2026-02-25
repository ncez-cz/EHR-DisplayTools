using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class ListBuilder : Widget
{
    private readonly FlexDirection m_direction;
    private readonly Func<int, int, XmlDocumentNavigator, XmlDocumentNavigator?, IList<Widget>> m_itemBuilder;
    private readonly string? m_childContainerClasses;
    private readonly string? m_flexContainerClasses;
    private readonly bool? m_wrapChildren;
    private readonly IdentifierSource? m_idSource;
    private readonly IdentifierSource? m_visualIdSource;
    private readonly Widget? m_separator;
    private readonly List<XmlDocumentNavigator>? m_items;
    private readonly string? m_itemsPath;

    public ListBuilder(
        List<XmlDocumentNavigator> items,
        FlexDirection direction,
        Func<int, int, XmlDocumentNavigator, XmlDocumentNavigator?, IList<Widget>> itemBuilder,
        string? childContainerClasses = null,
        string? flexContainerClasses = null,
        bool? wrapChildren = false,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        Widget? separator = null
    )
    {
        m_direction = direction;
        m_itemBuilder = itemBuilder;
        m_childContainerClasses = childContainerClasses;
        m_flexContainerClasses = flexContainerClasses;
        m_wrapChildren = wrapChildren;
        m_idSource = idSource;
        m_visualIdSource = visualIdSource ?? idSource;
        m_separator = separator;
        m_items = items;
    }

    public ListBuilder(
        List<XmlDocumentNavigator> items,
        FlexDirection direction,
        Func<int, XmlDocumentNavigator, IList<Widget>> itemBuilder,
        string? childContainerClasses = null,
        string? flexContainerClasses = null,
        bool? wrapChildren = false,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        Widget? separator = null
    )
    {
        m_direction = direction;
        m_itemBuilder = (x, _, y, _) => itemBuilder(x, y);
        m_childContainerClasses = childContainerClasses;
        m_flexContainerClasses = flexContainerClasses;
        m_wrapChildren = wrapChildren;
        m_idSource = idSource;
        m_visualIdSource = visualIdSource ?? idSource;
        m_separator = separator;
        m_items = items;
    }

    public ListBuilder(
        string itemsPath,
        FlexDirection direction,
        Func<int, IList<Widget>> itemBuilder,
        string? childContainerClasses = null,
        string? flexContainerClasses = null,
        bool? wrapChildren = false,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        Widget? separator = null
    )
    {
        m_direction = direction;
        m_itemBuilder = (x, _, _, _) => itemBuilder(x);
        m_childContainerClasses = childContainerClasses;
        m_flexContainerClasses = flexContainerClasses;
        m_wrapChildren = wrapChildren;
        m_idSource = idSource;
        m_visualIdSource = visualIdSource ?? idSource;
        m_separator = separator;
        m_itemsPath = itemsPath;
    }

    public ListBuilder(
        string itemsPath,
        FlexDirection direction,
        Func<int, XmlDocumentNavigator, IList<Widget>> itemBuilder,
        string? childContainerClasses = null,
        string? flexContainerClasses = null,
        bool? wrapChildren = false,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        Widget? separator = null
    )
    {
        m_direction = direction;
        m_itemBuilder = (x, _, y, _) => itemBuilder(x, y);
        m_childContainerClasses = childContainerClasses;
        m_flexContainerClasses = flexContainerClasses;
        m_wrapChildren = wrapChildren;
        m_idSource = idSource;
        m_visualIdSource = visualIdSource ?? idSource;
        m_separator = separator;
        m_itemsPath = itemsPath;
    }

    public ListBuilder(
        string itemsPath,
        FlexDirection direction,
        Func<int, int, XmlDocumentNavigator, XmlDocumentNavigator?, IList<Widget>> itemBuilder,
        string? childContainerClasses = null,
        string? flexContainerClasses = null,
        bool? wrapChildren = false,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        Widget? separator = null
    )
    {
        m_direction = direction;
        m_itemBuilder = itemBuilder;
        m_childContainerClasses = childContainerClasses;
        m_flexContainerClasses = flexContainerClasses;
        m_wrapChildren = wrapChildren;
        m_idSource = idSource;
        m_visualIdSource = visualIdSource ?? idSource;
        m_separator = separator;
        m_itemsPath = itemsPath;
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (m_items == null && m_itemsPath == null)
        {
            return new List<ParseError>
            {
                new() { Kind = ErrorKind.Unknown, Severity = ErrorSeverity.Warning, Path = navigator.GetFullPath() }
            };
        }

        var elements = m_items ?? navigator.SelectAllNodes(m_itemsPath!).ToList();
        var elementsCount = elements.Count;

        List<RenderResult> children = [];
        for (var i = 0; i < elementsCount; i++)
        {
            var element = elements[i];
            var nextNav = elements.ElementAtOrDefault(i + 1);
            children.Add(await m_itemBuilder(i, elementsCount, element, nextNav)
                .RenderConcatenatedResult(element, renderer, context));
            if (m_separator != null && i < elementsCount - 1)
            {
                children.Add(await m_separator.Render(navigator, renderer, context));
            }
        }

        var errors = children.SelectMany(x => x.Errors).ToList();
        if (errors.MaxSeverity() >= ErrorSeverity.Fatal)
        {
            return errors;
        }

        if (children.Count == 0)
        {
            return new RenderResult(string.Empty, errors);
        }

        var viewModel = new FlexList.ViewModel
        {
            Direction = m_direction,
            Children = children.Select(x => x.Content).OfType<string>(),
            ChildContainerClasses = m_childContainerClasses,
            CustomClass = m_flexContainerClasses,
            WrapChildren = m_wrapChildren,
        };
        HandleIds(context, navigator, viewModel, m_idSource, m_visualIdSource);
        var result = await renderer.RenderList(viewModel);

        return new RenderResult(result, errors);
    }
}