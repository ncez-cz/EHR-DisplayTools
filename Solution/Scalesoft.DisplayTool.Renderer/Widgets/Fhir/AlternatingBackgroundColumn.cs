using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class AlternatingBackgroundColumn(IList<Widget> widgets) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (widgets.Count == 0)
        {
            return RenderResult.NullResult;
        }

        List<Widget> finalWidgets = [];

        foreach (var widget in widgets)
        {
            if (widget.IsNullWidget)
            {
                continue;
            }

            var lastWidget = widgets.Last();

            Widget widgetToAdd = new Container(widget, optionalClass: "p-1");

            if (!widget.Equals(lastWidget))
            {
                widgetToAdd = new Column([widgetToAdd, new ThematicBreak("m-0")], flexContainerClasses: "gap-1");
            }

            finalWidgets.Add(widgetToAdd);
        }

        return await new Container(finalWidgets, optionalClass: "alternating-bg-items-container").Render(navigator,
            renderer, context);
    }
}