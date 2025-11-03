using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

public abstract class AlternatingBackgroundColumnResourceBase<T> : Widget
    where T : AlternatingBackgroundColumnResourceBase<T>, IResourceWidget, new()
{
    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        var widgets = items
            .Select(x =>
                new ChangeContext(
                    x,
                    new Container(new T(), idSource: x)
                )
            )
            .ToList<Widget>();
        return [new AlternatingBackgroundColumn(widgets)];
    }
}