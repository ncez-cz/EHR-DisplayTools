using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

/// <summary>
///     Represents a card that can be used individually or as part of a Timeline component.
/// </summary>
public class TimelineCard : DateSortableWidget
{
    private readonly IList<Widget> m_content;
    public sealed override DateTimeOffset? SortDate { get; set; }
    private readonly Widget? m_title;
    public string? CssClass { get; set; }
    private readonly List<TimelineCard> m_groupItems;
    private readonly bool m_isGroupContainer;
    private readonly bool m_isNested;

    public TimelineCard(
        IList<Widget> content,
        Widget? title = null,
        DateTimeOffset? sortDate = null,
        string? cssClass = null,
        List<TimelineCard>? groupItems = null,
        bool isNested = false
    )
    {
        m_content = content;
        SortDate = sortDate;
        m_title = title;
        CssClass = cssClass;
        m_groupItems = groupItems ?? [];
        m_isGroupContainer = m_groupItems is { Count: > 0 };
        m_isNested = isNested;
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        // Pre-render content if provided
        string? content = null;
        var contentResult = await m_content.RenderConcatenatedResult(navigator, renderer, context);
        if (contentResult.HasValue)
        {
            content = contentResult.Content;
        }
        else if (contentResult.IsNullResult)
        {
            return RenderResult.NullResult;
        }

        // Pre-render title if provided
        string? title = null;
        if (m_title != null)
        {
            var titleResult = await m_title.Render(navigator, renderer, context);
            if (titleResult.HasValue)
            {
                title = titleResult.Content;
            }
        }

        // Pre-render group items if provided
        var groupItems = await m_groupItems.Cast<Widget>().ToList()
            .RenderConcatenatedResult(navigator, renderer, context);

        var timeWidgets = DateTimeFormats.GetTimeWidget(SortDate, context.Language, DateFormatType.DayMonthYear);

        var time = await timeWidgets.RenderConcatenatedResult(navigator, renderer, context);
        var viewModel = new ViewModel
        {
            Content = content,
            Time = time.Content,
            Title = title,
            GroupItems = groupItems.Content,
            CssClass = CssClass,
            IsGroupContainer = m_isGroupContainer,
            IsNested = m_isNested
        };

        return await renderer.RenderTimelineCard(viewModel);
    }

    public class ViewModel
    {
        public string Content { get; init; }
        public string? Time { get; init; }
        public string? Title { get; init; }
        public string? CssClass { get; init; }
        public string? GroupItems { get; init; }
        public bool IsGroupContainer { get; init; }
        public bool IsNested { get; init; }
    }
}