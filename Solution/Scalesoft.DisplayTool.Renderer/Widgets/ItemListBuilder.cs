using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

/// <summary>
///     Represents an `ul` or `ol` html list.
/// </summary>
public class ItemListBuilder : Widget
{
    private readonly List<XmlDocumentNavigator>? m_items;
    private readonly string? m_itemsPath;
    private readonly ItemListType m_type;
    private readonly Func<int, XmlDocumentNavigator, Widget[]> m_itemBuilder;
    private readonly IdentifierSource? m_idSource;
    private readonly IdentifierSource? m_visualIdSource;
    private readonly NodeOrderer? m_orderer;

    /// <summary>
    ///     Represents an `ul` or `ol` html list.
    /// </summary>
    /// <param name="itemsPath"></param>
    /// <param name="type"></param>
    /// <param name="itemBuilder"></param>
    /// <param name="idSource"></param>
    /// <param name="visualIdSource"></param>
    /// <param name="orderer"></param>
    public ItemListBuilder(
        string itemsPath,
        ItemListType type,
        Func<int, Widget[]> itemBuilder,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        NodeOrderer? orderer = null
    )
    {
        m_itemsPath = itemsPath;
        m_type = type;
        m_itemBuilder = (i, _) => itemBuilder(i);
        m_idSource = idSource;
        m_visualIdSource = visualIdSource ?? idSource;
        m_orderer = orderer;
    }

    /// <summary>
    ///     Represents an `ul` or `ol` html list.
    /// </summary>
    /// <param name="itemsPath"></param>
    /// <param name="type"></param>
    /// <param name="itemBuilder"></param>
    /// <param name="idSource"></param>
    /// <param name="visualIdSource"></param>
    /// <param name="orderer"></param>
    public ItemListBuilder(
        string itemsPath,
        ItemListType type,
        Func<int, XmlDocumentNavigator, Widget[]> itemBuilder,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        NodeOrderer? orderer = null
    )
    {
        m_itemsPath = itemsPath;
        m_type = type;
        m_itemBuilder = itemBuilder;
        m_idSource = idSource;
        m_visualIdSource = visualIdSource ?? idSource;
        m_orderer = orderer;
    }
    
    public ItemListBuilder(
        List<XmlDocumentNavigator> items,
        ItemListType type,
        Func<int, XmlDocumentNavigator, Widget[]> itemBuilder,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        NodeOrderer? orderer = null
    )
    {
        m_items = items;
        m_type = type;
        m_itemBuilder = itemBuilder;
        m_idSource = idSource;
        m_visualIdSource = visualIdSource ?? idSource;
        m_orderer = orderer;
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (m_items == null && m_itemsPath == null)
        {
            throw new InvalidOperationException("Either items or itemsPath must be provided");
        }

        var elements = m_items ?? navigator.SelectAllNodes(m_itemsPath!);

        if (m_orderer != null)
        {
            elements = m_orderer(elements);
        }

        var childrenTasks = elements.Select(async (element, i) =>
            await m_itemBuilder(i, element).RenderConcatenatedResult(element, renderer, context));
        var children = await Task.WhenAll(childrenTasks);

        var errors = children.SelectMany(x => x.Errors).ToList();
        if (errors.MaxSeverity() >= ErrorSeverity.Fatal)
        {
            return errors;
        }

        var viewModel = new ItemList.ViewModel
        {
            Type = m_type, Rows = children
                .Where(x => !x.IsNullResult)
                .Select(x => x.Content)
                .OfType<string>()
                .ToList(),
        };
        HandleIds(context, navigator, viewModel, m_idSource, m_visualIdSource);
        var result = await renderer.RenderItemList(viewModel);
        return new RenderResult(result, errors);
    }
}