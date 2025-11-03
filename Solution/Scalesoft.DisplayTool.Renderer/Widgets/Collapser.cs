using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Collapser(
    IList<Widget> toggleLabelTitle,
    IList<Widget> title,
    IList<Widget> content,
    Func<Severity?> getSeverity,
    bool isCollapsed = false,
    IdentifierSource? idSource = null,
    IdentifierSource? visualIdSource = null,
    IList<Widget>? footer = null,
    string? customClass = null,
    IList<Widget>? iconPrefix = null,
    List<Widget>? subtitle = null
)
    : Widget
{
    private readonly IdentifierSource? m_visualIdSource = visualIdSource ?? idSource;

    public Collapser(
        IList<Widget> toggleLabelTitle,
        IList<Widget> title,
        IList<Widget> content,
        bool isCollapsed = false,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        IList<Widget>? footer = null,
        string? customClass = null,
        IList<Widget>? iconPrefix = null,
        Severity? severity = null,
        List<Widget>? subtitle = null
    ) : this(toggleLabelTitle, title, content, () => severity, isCollapsed, idSource, visualIdSource, footer,
        customClass, iconPrefix, subtitle)
    {
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var labelResult = await toggleLabelTitle.RenderConcatenatedResult(navigator, renderer, context);

        var titleResult = await title.RenderConcatenatedResult(navigator, renderer, context);

        var subtitleResult =
            subtitle != null ? await subtitle.RenderConcatenatedResult(navigator, renderer, context) : null;

        var contentResult = await content.RenderConcatenatedResult(navigator, renderer, context);

        var footerResult = footer != null
            ? await footer.RenderConcatenatedResult(navigator, renderer, context)
            : null;

        var iconPrefixRendered = iconPrefix != null
            ? await iconPrefix.RenderConcatenatedResult(navigator, renderer, context)
            : null;

        if (labelResult.MaxSeverity >= ErrorSeverity.Fatal || titleResult.MaxSeverity >= ErrorSeverity.Fatal ||
            contentResult.MaxSeverity >= ErrorSeverity.Fatal ||
            iconPrefixRendered?.MaxSeverity >= ErrorSeverity.Fatal)
        {
            List<ParseError> errors = [..labelResult.Errors, ..titleResult.Errors, ..contentResult.Errors];
            return errors;
        }

        if (labelResult.IsNullResult && subtitleResult != null && !subtitleResult.IsNullResult)
        {
            labelResult = subtitleResult;
            subtitleResult = null;
        }

        var viewModel = new ViewModel
        {
            InputId = Id,
            ToggleLabel = labelResult.Content ?? string.Empty,
            IsCollapsed = isCollapsed,
            Title = titleResult.Content ?? string.Empty,
            Content = contentResult.Content ?? string.Empty,
            Footer = footerResult?.Content,
            HideFooter = footerResult?.IsNullResult ?? true,
            Icon = IconHelper.GetInstance(SupportedIcons.ChevronUp, context),
            CustomClass = customClass,
            IconPrefix = iconPrefixRendered?.Content,
            Severity = getSeverity(),
            Subtitle = subtitleResult?.Content,
        };

        HandleIds(context, navigator, viewModel, idSource, m_visualIdSource);
        var view = await renderer.RenderCollapser(viewModel);
        return new RenderResult(view, contentResult.Errors);
    }

    public class ViewModel : ViewModelBase
    {
        public uint InputId { get; set; }
        public required string ToggleLabel { get; set; }

        public string? IconPrefix { get; set; }

        public bool IsCollapsed { get; set; }

        public required string Title { get; set; }

        public required string Content { get; set; }

        public string? Footer { get; set; }

        public bool HideFooter { get; set; }

        public required string Icon { get; set; }

        public Severity? Severity { get; set; }

        public string? Subtitle { get; set; }
    }
}