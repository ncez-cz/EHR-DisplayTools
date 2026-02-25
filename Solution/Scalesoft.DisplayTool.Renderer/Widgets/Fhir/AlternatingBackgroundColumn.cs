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

        for (var i = 0; i < widgets.Count; i++)
        {
            var widget = widgets[i];
            if (widget.IsNullWidget)
            {
                continue;
            }

            var nextWidget = widgets.ElementAtOrDefault(i + 1);
            Widget widgetToAdd = new Container(widget, optionalClass: "p-1");

            if (nextWidget != null)
            {
                Widget thBreak;
                if (widget is HideableDetails || nextWidget is HideableDetails)
                {
                    thBreak = new HideableDetails(new ThematicBreak("m-0"));
                }
                else
                {
                    thBreak = new ThematicBreak("m-0");
                }

                widgetToAdd = new Column([widgetToAdd, thBreak], flexContainerClasses: "gap-1");
            }

            finalWidgets.Add(widgetToAdd);
        }

        return await new Container(finalWidgets, optionalClass: "alternating-bg-items-container").Render(navigator,
            renderer, context);
    }
}