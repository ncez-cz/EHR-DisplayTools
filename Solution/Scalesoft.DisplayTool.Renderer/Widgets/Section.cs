using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class Section(
    string select,
    string? requiredSectionMissingTitle,
    List<Widget> title,
    IList<Widget> content,
    Func<Severity?> getSeverity,
    List<Widget>? subtitle = null,
    IdentifierSource? idSource = null,
    IdentifierSource? visualIdSource = null,
    LocalizedAbbreviations? titleAbbreviations = null,
    Widget? narrativeModal = null,
    string? customClass = null,
    bool isCollapsed = false
)
    : Widget
{
    public Section(
        string select,
        string? requiredSectionMissingTitle,
        List<Widget> title,
        IList<Widget> content,
        List<Widget>? subtitle = null,
        IdentifierSource? idSource = null,
        IdentifierSource? visualIdSource = null,
        LocalizedAbbreviations? titleAbbreviations = null,
        Severity? severity = null,
        Widget? narrativeModal = null,
        string? customClass = null,
        bool isCollapsed = false
    ) : this(select, requiredSectionMissingTitle, title, content, () => severity, subtitle, idSource, visualIdSource,
        titleAbbreviations, narrativeModal, customClass, isCollapsed)
    {
    }

    private readonly IdentifierSource? m_visualIdSource = visualIdSource ?? idSource;

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var selected = navigator.SelectAllNodes(select);
        if (!selected.Any())
        {
            if (string.IsNullOrEmpty(requiredSectionMissingTitle))
            {
                return string.Empty;
            }

            return new ParseError
            {
                Kind = ErrorKind.MissingValue,
                Message = requiredSectionMissingTitle,
                Path = navigator.GetFullPath(),
                Severity = ErrorSeverity.Fatal,
            };
        }

        var titleWithContext = new ChangeContext(select, title.ToArray());
        var contentWithContext = new ChangeContext(select, content.ToArray());
        var titleResult = await titleWithContext.Render(navigator, renderer, context);
        var subtitleResult =
            subtitle != null ? await subtitle.RenderConcatenatedResult(navigator, renderer, context) : null;

        var contentResult = await contentWithContext.Render(navigator, renderer, context);
        var dividerIcon = IconHelper.GetInstance(SupportedIcons.ChevronUp, context);

        var narrativeTextModal =
            narrativeModal != null ? await narrativeModal.Render(navigator, renderer, context) : null;

        var viewModel = new ViewModel
        {
            InputId = Id,
            Title = titleResult?.Content ?? string.Empty,
            Subtitle = subtitleResult?.Content,
            Content = contentResult.Content ?? string.Empty,
            OnClick = "updateName()",
            Severity = getSeverity(),
            DividerIcon = dividerIcon,
            NarrativeTextModal = narrativeTextModal?.Content,
            CustomClass = customClass,
            IsCollapsed = isCollapsed,
        };
        if (titleAbbreviations != null)
        {
            if (titleAbbreviations.Abbreviations.TryGetValue(context.Language.Primary, out var abbreviation))
            {
                viewModel.TitleAbbreviation = abbreviation;
            }
            else if (titleAbbreviations.Abbreviations.TryGetValue(context.Language.Fallback,
                         out var abbreviationFallback))
            {
                viewModel.TitleAbbreviation = abbreviationFallback;
            }
        }

        HandleIds(context, navigator, viewModel, idSource, m_visualIdSource);
        var view = await renderer.RenderSection(viewModel);

        return new RenderResult(view, contentResult.Errors);
    }

    public class ViewModel : ViewModelBase
    {
        public uint InputId { get; set; }

        public string? OnClick { get; set; }

        public required string Title { get; set; }

        public string? TitleAbbreviation { get; set; }

        public required string Content { get; set; }

        public string? Subtitle { get; set; }

        public Severity? Severity { get; set; }

        public required string DividerIcon { get; init; }

        public string? NarrativeTextModal { get; set; }

        public bool IsCollapsed { get; set; }
    }
}