using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public abstract class CardBase(
    Widget? title,
    Widget body,
    Severity? severity,
    List<Widget>? subtitle = null,
    bool isCollapsible = false,
    bool isCollapsed = false,
    string? optionalClass = null,
    string? bodyOptionalClass = null,
    IList<Widget>? iconPrefix = null,
    IList<Widget>? footer = null,
    IdentifierSource? idSource = null,
    IdentifierSource? visualIdSource = null
) : Widget
{
    private readonly IdentifierSource? m_visualIdSource = visualIdSource ?? idSource;

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        // Basic child widgets
        List<Widget> childWidgets = [body];
        if (title != null)
        {
            childWidgets.Add(title);
        }

        var childWidgetsArray = childWidgets.ToArray();
        var childrenResult = await RenderInternal(navigator, renderer, context, childWidgetsArray);

        if (childrenResult.IsFatal)
        {
            return childrenResult.Errors;
        }

        var titleContent = title == null ? null : childrenResult.GetContent(title);
        var bodyContent = childrenResult.GetContent(body);

        var footerResult = footer != null
            ? await footer.RenderConcatenatedResult(navigator, renderer, context)
            : null;

        // Collapser-specific rendering
        string? iconPrefixRendered = null;
        string? subtitleContent = null;
        if (isCollapsible)
        {
            var iconPrefixResult = iconPrefix != null
                ? await iconPrefix.RenderConcatenatedResult(navigator, renderer, context)
                : null;
            var subtitleResult = subtitle != null
                ? await subtitle.RenderConcatenatedResult(navigator, renderer, context)
                : null;

            iconPrefixRendered = iconPrefixResult?.Content;
            subtitleContent = subtitleResult?.Content;
        }

        var viewModel = new ViewModel
        {
            Title = titleContent,
            Body = bodyContent,
            BodyOptionalClass = bodyOptionalClass,
            Footer = footerResult?.Content,
            CustomClass = optionalClass,
            Severity = severity,
            IsCollapsible = isCollapsible,
            IsCollapsed = isCollapsed,
            IconPrefix = iconPrefixRendered,
            Subtitle = subtitleContent,
            Icon = IconHelper.GetInstance(SupportedIcons.ChevronUp, context),
            InputId = Id
        };

        HandleIds(context, navigator, viewModel, idSource, m_visualIdSource);

        var view = await renderer.RenderCard(viewModel);
        return new RenderResult(view, childrenResult.Errors);
    }

    public class ViewModel : ViewModelBase
    {
        public uint InputId { get; set; }
        public string? Title { get; set; }

        public string? Body { get; set; }

        public string? BodyOptionalClass { get; set; }

        public string? Footer { get; set; }

        public Severity? Severity { get; set; }

        public bool IsCollapsible { get; set; }

        public bool IsCollapsed { get; set; }

        public string? IconPrefix { get; set; }

        public string? Subtitle { get; set; }

        public required string Icon { get; set; }
    }
}