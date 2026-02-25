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
        if (m_content.All(x => x.IsNullWidget))
        {
            return RenderResult.NullResult;
        }

        var timeWidgets = DateTimeFormats.GetTimeWidget(SortDate, context.Language, DateFormatType.DayMonthYear);

        var viewModel = new ViewModel
        {
            Content = m_content,
            Time = timeWidgets,
            Title = m_title,
            GroupItems = m_groupItems.ToList<Widget>(),
            CssClass = CssClass,
            IsGroupContainer = m_isGroupContainer,
            IsNested = m_isNested,
        };

        var widget = new CareplanTimelineItemWidget(viewModel);

        return await widget.Render(navigator, renderer, context);
    }

    private class ViewModel
    {
        public required IList<Widget> Content { get; init; }
        public required IList<Widget> Time { get; init; }
        public Widget? Title { get; init; }
        public string? CssClass { get; init; }
        public required IList<Widget> GroupItems { get; init; }
        public bool IsGroupContainer { get; init; }
        public bool IsNested { get; init; }
    }

    private class CareplanTimelineItemWidget(ViewModel model) : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var resultWidget = new Container([
                    new Container([
                            new Container([
                                new If(_ => model.Title != null,
                                    new Container([model.Title!], optionalClass: "timeline-title")),
                                new If(_ => model.Time.Any() && !model.IsNested,
                                    new Container(model.Time, optionalClass: "timeline-time")),
                                new Container([
                                    ..model.Content,
                                    new If(
                                        _ => model.IsGroupContainer && model.GroupItems.Any(), new Container([
                                            new Container([
                                                new Container([
                                                    new Container(model.GroupItems,
                                                        optionalClass: "timeline-body")
                                                ], optionalClass: "timeline-group-content")
                                            ], optionalClass: "timeline-group-item")
                                        ], optionalClass: "timeline-group-container")),
                                ], optionalClass: "timeline-body"),
                            ], optionalClass: ""),
                        ],
                        optionalClass: model.CssClass + " " + (model.IsNested ? "nested" : "timeline-item")),
                ],
                optionalClass: model.IsNested && model.Time.Any()
                    ? "nested-timeline-card-container"
                    : "timeline-card-container");

            return resultWidget.Render(navigator, renderer, context);
        }
    }
}