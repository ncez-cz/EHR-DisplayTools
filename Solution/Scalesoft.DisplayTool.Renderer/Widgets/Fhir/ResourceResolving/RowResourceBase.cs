using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

/// <summary>
/// Base for a resource that should be displayed in a basic
/// </summary>
public abstract class RowResourceBase<T> : Widget where T : RowResourceBase<T>, IResourceWidget, new()
{
    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        // more than one → wrap in a Row
        if (items.Count != 1)
        {
            return
            [
                new Row(
                    [..items.Select(x => new ChangeContext(x, new Container([new T()], idSource: x)))],
                    flexContainerClasses: "gap-0"
                ),
            ];
        }

        var item = items.FirstOrDefault();
        if (item == null)
        {
            return [new NullWidget()];
        }

        // just return the widget directly
        return
        [
            new ChangeContext(
                item,
                new Container([new T()], idSource: item)
            ),
        ];
    }
}