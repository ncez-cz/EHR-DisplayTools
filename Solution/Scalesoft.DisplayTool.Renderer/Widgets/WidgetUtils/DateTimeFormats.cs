using System.Globalization;
using Scalesoft.DisplayTool.Renderer.Utils.Language;

namespace Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;

public static class DateTimeFormats
{
    private static readonly Dictionary<LanguageOptions, Dictionary<DateFormatType, string>> m_formats =
        new()
        {
            {
                LanguageOptions.Czech, new Dictionary<DateFormatType, string>
                {
                    { DateFormatType.Year, "yyyy" },
                    { DateFormatType.MonthYear, "MMM yyyy" },
                    { DateFormatType.DayMonthYear, "d.M.yyyy" },
                    { DateFormatType.DayMonthYearTimezone, "d.M.yyyy UTCzzz" },
                    { DateFormatType.MinuteHourDayMonthYear, "d.M.yyyy HH:mm" },
                    { DateFormatType.MinuteHourDayMonthYearTimezone, "d.M.yyyy HH:mm UTCzzz" },
                    { DateFormatType.SecondMinuteHourDayMonthYear, "d.M.yyyy HH:mm:ss" },
                    { DateFormatType.SecondMinuteHourDayMonthYearTimezone, "d.M.yyyy HH:mm:ss UTCzzz" },
                    { DateFormatType.Timezone, "UTCzzz" },
                    { DateFormatType.MinuteHour, "HH:mm" },
                    { DateFormatType.MinuteHourTimezone, "HH:mm UTCzzz" },
                }
            },
            {
                LanguageOptions.EnglishGreatBritain, new Dictionary<DateFormatType, string>
                {
                    { DateFormatType.Year, "yyyy" },
                    { DateFormatType.MonthYear, "MMM yyyy" },
                    { DateFormatType.DayMonthYear, "dd.MM.yyyy" },
                    { DateFormatType.DayMonthYearTimezone, "dd.MM.yyyy UTCzzz" },
                    { DateFormatType.MinuteHourDayMonthYear, "dd.MM.yyyy HH:mm" },
                    { DateFormatType.MinuteHourDayMonthYearTimezone, "dd.MM.yyyy HH:mm UTCzzz" },
                    { DateFormatType.SecondMinuteHourDayMonthYear, "dd.MM.yyyy HH:mm:ss" },
                    { DateFormatType.SecondMinuteHourDayMonthYearTimezone, "dd.MM.yyyy HH:mm:ss UTCzzz" },
                    { DateFormatType.Timezone, "UTCzzz" },
                    { DateFormatType.MinuteHour, "HH:mm" },
                    { DateFormatType.MinuteHourTimezone, "HH:mm UTCzzz" },
                }
            },
        };

    private static readonly Dictionary<LanguageOptions, string> m_timezoneSeparators =
        new()
        {
            {
                LanguageOptions.Czech, " "
            },
            {
                LanguageOptions.EnglishGreatBritain, " "
            },
        };

    private static readonly Dictionary<DateFormatType, DateFormatTypeTimeZoneSplit> m_timezoneSplitInfo =
        new()
        {
            {
                DateFormatType.DayMonthYearTimezone,
                new DateFormatTypeTimeZoneSplit(DateFormatType.DayMonthYear, DateFormatType.Timezone)
            },
            {
                DateFormatType.MinuteHourTimezone,
                new DateFormatTypeTimeZoneSplit(DateFormatType.MinuteHour, DateFormatType.Timezone)
            },
            {
                DateFormatType.MinuteHourDayMonthYearTimezone,
                new DateFormatTypeTimeZoneSplit(DateFormatType.MinuteHourDayMonthYear, DateFormatType.Timezone)
            },
            {
                DateFormatType.SecondMinuteHourDayMonthYearTimezone,
                new DateFormatTypeTimeZoneSplit(DateFormatType.SecondMinuteHourDayMonthYear, DateFormatType.Timezone)
            },
        };

    private static string? GetFormat(Language language, DateFormatType type)
    {
        var langFormats = m_formats.GetValueOrDefault(language.Primary) ??
                          m_formats.GetValueOrDefault(language.Fallback);
        return langFormats?.GetValueOrDefault(type);
    }

    private static string? GetTimezoneSeparator(Language language)
    {
        var separator = m_timezoneSeparators.GetValueOrDefault(language.Primary) ??
                        m_timezoneSeparators.GetValueOrDefault(language.Fallback);
        return separator;
    }

    public static List<Widget> GetTimeWidget(
        DateTimeOffset? date,
        Language language,
        DateFormatType type,
        CultureInfo? culture = null
    )
    {
        if (date == null)
        {
            return [];
        }

        culture ??= CultureInfo.InvariantCulture;
        var result = new List<Widget>();
        if (m_timezoneSplitInfo.TryGetValue(type, out var types))
        {
            var dateWithoutTimezoneFormat = GetFormat(language, types.WithoutTimeZoneFormat);
            if (dateWithoutTimezoneFormat != null)
            {
                result.Add(new ConstantText(date.Value.ToString(dateWithoutTimezoneFormat, culture)));
            }

            var dateTimezoneSeparator = GetTimezoneSeparator(language);
            if (dateTimezoneSeparator != null)
            {
                result.Add(new HideableDetails(new ConstantText(dateTimezoneSeparator)));
            }

            var timezoneFormat = GetFormat(language, types.TimezoneFormat);
            if (timezoneFormat != null)
            {
                result.Add(new HideableDetails(new ConstantText(date.Value.ToString(timezoneFormat, culture))));
            }
        }
        else
        {
            var format = GetFormat(language, type);
            if (format != null)
            {
                result.Add(new ConstantText(date.Value.ToString(format, culture)));
            }
        }

        return result;
    }
}

public class DateFormatTypeTimeZoneSplit(
    DateFormatType withoutTimeZoneFormat,
    DateFormatType timezoneFormat
)
{
    public readonly DateFormatType WithoutTimeZoneFormat = withoutTimeZoneFormat;
    public readonly DateFormatType TimezoneFormat = timezoneFormat;
}