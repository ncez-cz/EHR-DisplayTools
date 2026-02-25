using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;
/// <summary>
/// Wrap the content when the specified condition is met.
/// </summary>
/// <param name="condition">Specify the condition that determines whether content should be wrapped.</param>
/// <param name="wrapperBuilder">Defines the wrapper element. Use the content parameter (an array of Widget[]) to specify where the children should be rendered. (Default is HideableDetails)</param>
/// <param name="children">Content that is conditionally wrapped.</param>
public class ConditionalWrapper(
    Func<XmlDocumentNavigator, bool> condition,
    Func<Widget[], Widget> wrapperBuilder,
    params Widget[] children
    ) : Widget
{
    public ConditionalWrapper(
        Func<XmlDocumentNavigator, bool> condition,
        params Widget[] children
    ) : this(condition, x => new HideableDetails(x), children)
    {
        
    }
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context)
    {
        if (condition(navigator))
        {
            return wrapperBuilder(children).Render(navigator, renderer, context);
        }

        return new Concat(children).Render(navigator, renderer, context);
    }
}