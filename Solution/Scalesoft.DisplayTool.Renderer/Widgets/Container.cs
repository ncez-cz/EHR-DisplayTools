using System.Text.RegularExpressions;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Container : Widget
{
    private readonly IList<Widget> m_content;
    private readonly ContainerType m_type;
    private readonly string? m_optionalClass;
    private readonly IdentifierSource? m_idSource;
    private readonly IdentifierSource? m_visualIdSource;

    public Container(IList<Widget> content, ContainerType type = ContainerType.Div, string? optionalClass = null, IdentifierSource? idSource = null, IdentifierSource? visualIdSource = null)
    {
        m_content = content;
        m_type = type;
        m_optionalClass = optionalClass;
        m_idSource = idSource;
        m_visualIdSource = visualIdSource ?? idSource;
    }

    public Container(
        Widget content,
        ContainerType type = ContainerType.Div,
        string? optionalClass = null,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null
    ) : this([content], type, optionalClass, idSource, visualIdSource)
    {
    }

    public override async Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
        var contentResult = await m_content.RenderConcatenatedResult(navigator, renderer, context);
        if (!contentResult.HasValue)
        {
            return contentResult;
        }

        var type = m_type;
        if (m_type == ContainerType.Auto)
        {
            var hasDiv = DivRegex.Regex().IsMatch(contentResult.Content);
            type = hasDiv ? ContainerType.Div : ContainerType.Span;
        }

        var viewModel = new ViewModel
        {
            Content = contentResult.Content,
            Type = type,
            CustomClass = m_optionalClass,
        };

        HandleIds(context, navigator, viewModel, m_idSource, m_visualIdSource);
        var view = await renderer.RenderContainer(viewModel);
        return new RenderResult(view, contentResult.Errors);
    }

    public class ViewModel : ViewModelBase
    {
        public required string Content { get; set; }

        public required ContainerType Type { get; set; }
    }
}

public static partial class DivRegex
{
    [GeneratedRegex(@"<\s*div\b[^<>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    public static partial Regex Regex();
}