using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

/// <summary>
///     Base for a resource that should be displayed in a basic
/// </summary>
public abstract class ColumnResourceBase<T> : Widget where T : ColumnResourceBase<T>, IResourceWidget, new()
{
    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        // more than one → wrap in a Column
        if (items.Count != 1)
        {
            return
            [
                new Column(
                    [
                        ..items.Select(x => new ChangeContext(x,
                            new Container([new T()], idSource: x, optionalClass: "resource-container")))
                    ],
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
                new Container([new T()], idSource: item, optionalClass: "resource-container")
            ),
        ];
    }
}