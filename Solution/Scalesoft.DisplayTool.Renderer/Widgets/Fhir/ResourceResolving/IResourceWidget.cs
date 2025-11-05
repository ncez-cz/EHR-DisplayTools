using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public interface IResourceWidget
{
    public static abstract string ResourceType { get; }

    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public static bool RequiresExternalTitle => false;

    /// <summary>
    /// Takes a List of nodes and returns instantiated widgets arranged in a renderable way
    /// (for example, in a table or striped column).
    /// </summary>
    /// <param name="items">Navigators, each in the context of a supported resource</param>
    /// <returns></returns>
    public static abstract List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items);

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        return null;
    }
}