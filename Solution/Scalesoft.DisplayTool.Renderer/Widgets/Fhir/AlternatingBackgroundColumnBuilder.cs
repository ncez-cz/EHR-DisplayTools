using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

/// <summary>
///     Selects nodes matching itemsPath, builds widgets for them, and wraps them
///     in an AlternatingBackgroundColumn structure.
/// </summary>
public class AlternatingBackgroundColumnBuilder(
    string itemsPath,
    Func<int, int, XmlDocumentNavigator, XmlDocumentNavigator?, IList<Widget>> itemBuilder,
    string? orderSelector = null,
    bool orderAscending = true,
    NodeOrderer? orderer = null
) : ParsingWidget(itemsPath)
{
    private readonly List<XmlDocumentNavigator>? m_providedNodes;

    private readonly NodeOrderer m_defaultOrderer = nodes =>
    {
        if (string.IsNullOrEmpty(orderSelector))
        {
            return nodes.ToList();
        }

        var elementsToRender = orderAscending
            ? nodes.OrderBy(nav => nav.EvaluateNumber(orderSelector))
            : nodes.OrderByDescending(nav => nav.EvaluateNumber(orderSelector));
        return elementsToRender.ToList();
    };

    #region Constructors

    public AlternatingBackgroundColumnBuilder(
        string itemsPath,
        Func<int, IList<Widget>> itemBuilder,
        string? orderSelector = null,
        bool orderAscending = true
    ) : this(itemsPath,
        (i, _, _, _) => itemBuilder(i), orderSelector, orderAscending)
    {
    }

    public AlternatingBackgroundColumnBuilder(
        string itemsPath,
        Func<int, int, IList<Widget>> itemBuilder,
        string? orderSelector = null,
        bool orderAscending = true
    ) : this(itemsPath,
        (i, totalCount, _, _) => itemBuilder(i, totalCount), orderSelector, orderAscending)
    {
    }

    public AlternatingBackgroundColumnBuilder(
        List<XmlDocumentNavigator> items,
        Func<int, int, XmlDocumentNavigator, IList<Widget>> itemBuilder,
        string? orderSelector = null,
        bool orderAscending = true
    ) : this(string.Empty, (i, totalCount, nav, _) => itemBuilder(i, totalCount, nav), orderSelector,
        orderAscending)
    {
        m_providedNodes = items;
    }

    public AlternatingBackgroundColumnBuilder(
        string itemsPath,
        Func<int, int, XmlDocumentNavigator, IList<Widget>> itemBuilder,
        NodeOrderer? orderer
    ) : this(itemsPath, (i, totalCount, nav, _) => itemBuilder(i, totalCount, nav), orderer: orderer)
    {
    }

    #endregion

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator data,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var elements = m_providedNodes ?? data.SelectAllNodes(Path).ToList();
        var count = elements.Count;
        var elementsToRender = (orderer ?? m_defaultOrderer)(elements);

        List<Widget> allWidgets = [];

        for (var i = 0; i < elementsToRender.Count; i++)
        {
            var element = elementsToRender[i];
            var nextNav = elementsToRender.ElementAtOrDefault(i + 1);

            var itemWidgets = itemBuilder(i, count, element, nextNav);

            if (itemWidgets.Count == 0)
            {
                continue;
            }

            var rowContent = new Concat(itemWidgets);

            allWidgets.Add(new ChangeContext(element, rowContent));
        }

        return await new AlternatingBackgroundColumn(allWidgets).Render(data, renderer, context);
    }
}