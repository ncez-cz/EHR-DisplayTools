using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowBoolean : Widget
{
    private readonly Widget m_onFalse;
    private readonly Widget m_onTrue;
    private readonly string m_path;

    public ShowBoolean(Widget onFalse, Widget onTrue, string path = ".")
    {
        m_onFalse = onFalse;
        m_onTrue = onTrue;
        m_path = path;
    }

    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer, RenderContext context)
    {
        if (context.RenderMode == RenderMode.Documentation)
        {
            return Task.FromResult<RenderResult>(navigator.SelectSingleNode(m_path).GetFullPath());
        }

        if (IsDataAbsent(navigator, m_path))
        {
            return new AbsentData(m_path).Render(navigator, renderer, context);
        }

        return new ChangeContext(m_path,
            new Choose([
                new When("@value = 'false'", m_onFalse),
                new When("@value = 'true'", m_onTrue),
            ], new Text("@value"))
        ).Render(navigator, renderer, context);
    }
}