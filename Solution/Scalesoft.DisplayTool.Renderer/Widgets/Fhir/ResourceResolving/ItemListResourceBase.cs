using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

/// <summary>
/// Base for a resource that should be displayed in an item list (ul)
/// </summary>
public abstract class ItemListResourceBase<T> : Widget where T : ItemListResourceBase<T>, IResourceWidget, new()
{
    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new ItemListBuilder(items, ItemListType.Unordered, (_, x) => [new Container(new T(), idSource: x)])];
    }
}