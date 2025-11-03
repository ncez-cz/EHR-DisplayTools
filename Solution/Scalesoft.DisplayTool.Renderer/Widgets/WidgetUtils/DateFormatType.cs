namespace Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

public enum DateFormatType
{
    Year,
    MonthYear,
    DayMonthYear,
    DayMonthYearTimezone,
    MinuteHourDayMonthYear,
    MinuteHourDayMonthYearTimezone,
    SecondMinuteHourDayMonthYear,
    SecondMinuteHourDayMonthYearTimezone,
    Timezone, // special format not defined in FHIR for simplified mode
}