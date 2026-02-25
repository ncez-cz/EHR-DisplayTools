using System.Diagnostics.CodeAnalysis;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Link : Widget
{
    private readonly IdentifierSource? m_idSource;
    private readonly IdentifierSource? m_visualIdSource;
    private Widget Content { get; set; }
    private string HrefSimple { get; }

    [MemberNotNullWhen(true, nameof(HrefSimple))]
    private bool BySimpleValue { get; }

    private string? DownloadInfo { get; }

    private string? OptionalClass { get; }

    private string? ContentType { get; }

    public Link(
        Widget content,
        string hrefSimple,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        string? downloadInfo = null,
        string? optionalClass = null,
        string? contentType = null
    )
    {
        m_idSource = idSource;
        m_visualIdSource = visualIdSource ?? idSource;
        Content = content;
        HrefSimple = hrefSimple;
        BySimpleValue = true;
        DownloadInfo = downloadInfo;
        OptionalClass = optionalClass;
        ContentType = contentType;
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (context.RenderMode == RenderMode.Documentation)
        {
            Content = new ConstantText(navigator.GetFullPath());
        }

        Widget[] content = [Content];

        var renderResult = await RenderInternal(navigator, renderer, context, content);
        if (renderResult.IsFatal)
        {
            return renderResult.Errors;
        }

        var text = renderResult.GetContent(Content) ?? string.Empty;

        var viewModel = new ViewModel
        {
            Text = text,
            Href = HrefSimple,
            Download = DownloadInfo,
            CustomClass = OptionalClass,
            ContentType = ContentType,
        };

        HandleIds(context, navigator, viewModel, m_idSource, m_visualIdSource);

        return await renderer.RenderLink(viewModel);
    }

    public class ViewModel : ViewModelBase
    {
        public required string Text { get; set; }

        public required string Href { get; set; }

        public string? Download { get; set; }

        public string? ContentType { get; set; }
    }
}