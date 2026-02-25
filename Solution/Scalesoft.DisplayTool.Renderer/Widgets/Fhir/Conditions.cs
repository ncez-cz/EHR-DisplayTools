using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Conditions(
    List<XmlDocumentNavigator> items,
    Widget problemColumnLabel,
    bool skipIdPopulation = false
) : Widget
{
    public Conditions(List<XmlDocumentNavigator> items, bool skipIdPopulation = false) : this(items,
        new LocalizedLabel("condition"), skipIdPopulation)
    {
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var widget =
            new AlternatingBackgroundColumn(
                items.Select(x => new ChangeContext(
                        x,
                        new ConditionResource(problemColumnLabel, skipIdPopulation)
                    )
                ).ToList<Widget>()
            );

        return widget.Render(navigator, renderer, context);
    }

    public static string ResourceType => "Condition";

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new Conditions(items)];
    }
}