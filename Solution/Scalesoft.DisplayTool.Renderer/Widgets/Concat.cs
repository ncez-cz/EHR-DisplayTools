using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Concat(
    IList<Widget> children,
    Widget? separator
) : Widget
{
    public Concat(IList<Widget> children, string separator = " ") : this(children, new ConstantText(separator))
    {
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator data,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var separatorWidget = separator ?? new ConstantText(string.Empty);

        var results = await RenderInternal(data, renderer, [..children, separatorWidget], context);
        if (results.IsFatal)
        {
            return results.Errors;
        }

        var content = results.GetValidContents(children);

        if (content.Count == 0)
        {
            return RenderResult.NullResult;
        }

        var renderedSeparator = results.GetContent(separatorWidget);

        var concatenated = string.Join(renderedSeparator, content);
        return new RenderResult(concatenated, results.Errors);
    }
}