using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Optional : Widget
{
    private readonly string m_path;
    private readonly Func<XmlDocumentNavigator, IList<Widget>> m_builder;

    private Widget[] m_elseChildren = [];

    public Optional(string path, params Widget[] children)
    {
        m_path = path;
        m_builder = _ => children;
    }

    public Optional(string path, Func<XmlDocumentNavigator, IList<Widget>> builder)
    {
        m_path = path;
        m_builder = builder;
    }

    public Optional(string path, Func<XmlDocumentNavigator, Widget> builder)
    {
        m_path = path;
        m_builder = nav => [builder(nav)];
    }

    public Optional Else(params Widget[] children)
    {
        m_elseChildren = children;
        return this;
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (navigator.Node == null)
        {
            return RenderResult.NullResult;
        }
        
        var element = navigator.SelectSingleNode(m_path);

        if (element.Node != null)
        {
            return await m_builder(element).RenderConcatenatedResult(element, renderer, context);
        }

        if (m_elseChildren.Length == 0)
        {
            return RenderResult.NullResult;
        }

        return await m_elseChildren.RenderConcatenatedResult(navigator, renderer, context);
    }
}