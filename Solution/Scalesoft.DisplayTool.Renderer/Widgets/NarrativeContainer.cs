using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class NarrativeContainer : Widget
{
    private readonly IList<Widget> m_content;
    private readonly IdentifierSource? m_idSource;

    public NarrativeContainer(IList<Widget> content, IdentifierSource? idSource = null)
    {
        m_content = content;
        m_idSource = idSource;
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var contentResult = await m_content.RenderConcatenatedResult(navigator, renderer, context);
        if (!contentResult.HasValue)
        {
            return contentResult;
        }

        var viewModel = new ViewModel
        {
            Content = contentResult.Content,
        };

        HandleIds(context, navigator, viewModel, m_idSource, null);
        var view = await renderer.RenderNarrativeContainer(viewModel);

        return new RenderResult(view, contentResult.Errors);
    }

    public class ViewModel : ViewModelBase
    {
        public required string Content { get; set; }
    }
}