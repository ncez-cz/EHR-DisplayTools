using System.Globalization;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaDate(string select) : Widget
{
    private static readonly KeyValuePair<string, DateFormatType>[] m_dateFormats =
    [
        new("yyyy", DateFormatType.Year),
        new("yyyy-MM", DateFormatType.MonthYear),
        new("yyyy-MM-dd", DateFormatType.DayMonthYear),
        new("yyyy-MM-ddK", DateFormatType.DayMonthYearTimezone),
        new("yyyy-MM-ddTHH:mm", DateFormatType.MinuteHourDayMonthYear),
        new("yyyy-MM-ddTHH:mmK", DateFormatType.MinuteHourDayMonthYearTimezone),
        new("yyyy-MM-ddTHH:mm:ss", DateFormatType.SecondMinuteHourDayMonthYear),
        new("yyyy-MM-ddTHH:mm:ssK", DateFormatType.SecondMinuteHourDayMonthYearTimezone),
    ];

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var value = navigator.SelectSingleNode(select).Node?.Value;
        if (value == null)
        {
            return Task.FromResult<RenderResult>(new ParseError
            {
                Kind = ErrorKind.MissingValue,
                Message = "Missing date value",
                Path = navigator.GetFullPath(),
                Severity = ErrorSeverity.Warning,
            });
        }

        foreach (var dateFormat in m_dateFormats)
        {
            if (!DateTimeOffset.TryParseExact(value, dateFormat.Key, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var date))
            {
                continue;
            }

            if (context.RenderMode == RenderMode.Documentation)
            {
                return Task.FromResult<RenderResult>(navigator.GetFullPath());
            }

            var timeWidgets = DateTimeFormats.GetTimeWidget(date, context.Language, dateFormat.Value);

            return timeWidgets.RenderConcatenatedResult(navigator, renderer, context);
        }

        return Task.FromResult(new RenderResult(
            value,
            [
                new ParseError
                {
                    Kind = ErrorKind.InvalidValue, Message = "Invalid date format", Path = navigator.GetFullPath(),
                    Severity = ErrorSeverity.Warning,
                },
            ]));
    }
}