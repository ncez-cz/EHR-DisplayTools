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
    MinuteHour, // special format not defined in FHIR for EMS display
    MinuteHourTimezone, // special format not defined in FHIR for EMS display
}