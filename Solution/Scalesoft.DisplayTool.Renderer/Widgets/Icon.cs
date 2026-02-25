using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Icon : Widget
{
    private readonly SupportedIcons m_icon;
    private readonly string? m_optionalClass;

    public Icon(
        SupportedIcons icon,
        string? optionalClass = null
    )
    {
        m_icon = icon;
        m_optionalClass = optionalClass;
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        return Task.FromResult<RenderResult>(IconHelper.GetInstance(m_icon, context, m_optionalClass));
    }
}