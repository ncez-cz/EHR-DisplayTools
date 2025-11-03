using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

/// <summary>
///     Selects all nodes matching itemsPath, constructs a widget for each of them using itemBuilder,
///     then concatenates the results.
/// </summary>
/// <param name="itemsPath">Xpath expression that should match multiple nodes</param>
/// <param name="itemBuilder">
///     Gets called for every matching node, with the first parameter being the current index,
///     the second being the total number of nodes
/// </param>
public class CommaSeparatedBuilder(
    string itemsPath,
    Func<int, int, XmlDocumentNavigator, Widget[]> itemBuilder,
    NodeOrderer? orderer = null
)
    : ConcatBuilder(itemsPath, itemBuilder, orderer, new ConstantText(", "))
{
    public CommaSeparatedBuilder(string itemsPath, Func<int, Widget[]> itemBuilder) : this(itemsPath,
        (i, totalCount, nav) => itemBuilder(i))
    {
    }

    public CommaSeparatedBuilder(string itemsPath, Func<int, Widget> itemBuilder) : this(itemsPath,
        (i, _, _) => [itemBuilder(i)])
    {
    }
}
