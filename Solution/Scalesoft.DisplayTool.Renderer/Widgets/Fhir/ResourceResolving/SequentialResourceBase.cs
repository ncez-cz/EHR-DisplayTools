using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

/// <summary>
/// Base for a resource that should be simply placed in an array
/// </summary>
public abstract class SequentialResourceBase<T> : Widget where T : SequentialResourceBase<T>, IResourceWidget, new()
{
    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return items.Select(x => new ChangeContext(x, new Container(new T(), idSource: x))).ToList<Widget>();
    }
}