using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

/// <summary>
///     Evaluates the given test. Returns the children if it's truthy, otherwise returns an empty string.
///     Equivalent of <a href="https://developer.mozilla.org/en-US/docs/Web/XSLT/Element/if">xls:if</a>
/// </summary>
public class Condition : Widget
{
    private readonly string m_test;
    private readonly Widget[] m_children;

    /// <summary>
    ///     Evaluates the given test. Returns the children if it's truthy, otherwise returns an empty string.
    ///     Equivalent of <a href="https://developer.mozilla.org/en-US/docs/Web/XSLT/Element/if">xls:if</a>
    /// </summary>
    /// <param name="test">XPath expression</param>
    /// <param name="children">Children to render</param>
    public Condition(string test, params Widget[] children)
    {
        m_test = test;
        m_children = children;
    }

    public Condition(string test, Func<Widget[]> builder)
    {
        m_test = test;
        m_children = builder();
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator data,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var testResult = data.EvaluateCondition(m_test);

        return testResult
            ? await m_children.RenderConcatenatedResult(data, renderer, context)
            : RenderResult.NullResult;
    }
}