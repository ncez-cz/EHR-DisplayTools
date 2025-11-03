using System.Diagnostics.CodeAnalysis;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class LazyWidget : Widget
{
    private readonly Func<IList<Widget>>? m_builder;
    private readonly IList<Widget>? m_value;

    [MemberNotNullWhen(true, nameof(m_value))]
    [MemberNotNullWhen(false, nameof(m_builder))]
    private bool ShouldShortCircuit { get; }

    public LazyWidget(Func<IList<Widget>> builder)
    {
        m_builder = builder;
        ShouldShortCircuit = false;
    }

    public LazyWidget(IList<Widget> list)
    {
        m_value = list;
        ShouldShortCircuit = true;
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        return (ShouldShortCircuit ? m_value : m_builder()).RenderConcatenatedResult(navigator, renderer, context);
    }
}