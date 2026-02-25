using System.Runtime.Serialization;
using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class HealthcareServiceAvailableTime : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var timeNavigators = navigator.SelectAllNodes("f:availableTime");
        SortedDictionary<DayOfWeek, List<Widget>> dayWidgets = [];
        HashSet<DayOfWeek> allDayDays = [];

        // Process each available time element
        foreach (var timeNavigator in timeNavigators)
        {
            var isAllDay = timeNavigator.EvaluateCondition("f:allDay[@value='true']");

            if (!timeNavigator.EvaluateCondition("f:availableStartTime") &&
                !timeNavigator.EvaluateCondition("f:availableEndTime") &&
                !isAllDay)
            {
                continue;
            }

            // Get all days in this time slot
            var daysOfWeekNodes = timeNavigator.SelectAllNodes("f:daysOfWeek");
            Widget timeText;

            if (isAllDay)
            {
                timeText = new TextContainer(TextStyle.Bold,
                    new LocalizedLabel("healthcare-service.availableTime.allDay"));
            }
            else
            {
                timeText =
                    new ChangeContext(timeNavigator, new If(
                        _ => timeNavigator.EvaluateCondition("f:availableStartTime"),
                        new ShowTime("f:availableStartTime")
                    ).Else(
                        new ConstantText("?")
                    ), new ConstantText(" - "), new If(_ => timeNavigator.EvaluateCondition("f:availableEndTime"),
                        new ShowTime("f:availableEndTime")
                    ).Else(
                        new ConstantText("?")
                    ));
            }

            // Associate the time with each specified day
            foreach (var dayNode in daysOfWeekNodes)
            {
                var dayCode = dayNode.SelectSingleNode("@value").Node?.Value.ToLowerInvariant();
                var dayEnum = dayCode.ToEnum<DayOfWeek>();

                // Skip invalid day codes
                if (dayEnum == null)
                {
                    continue;
                }

                var day = dayEnum.Value;


                if (isAllDay)
                {
                    // If all day, replace any existing entry and mark it in the set
                    dayWidgets[day] = [timeText];
                    allDayDays.Add(day);
                }
                else if (!allDayDays.Contains(day))
                {
                    if (!dayWidgets.TryGetValue(day, out var existingTimes))
                    {
                        existingTimes = [];
                        dayWidgets[day] = existingTimes;
                    }

                    existingTimes.Add(timeText);
                }
            }
        }

        foreach (var day in Enum.GetValues<DayOfWeek>())
        {
            if (!dayWidgets.ContainsKey(day))
            {
                dayWidgets[day] = [new TextContainer(TextStyle.Muted, new LocalizedLabel("general.unspecified"))];
            }
        }

        List<Widget> outputWidgets = [];


        foreach (var dayWidget in dayWidgets)
        {
            var dayName = dayWidget.Key.ToEnumString();

            if (dayName != null)
            {
                outputWidgets.Add(
                    new Container([
                        new PlainBadge(
                            new CodedValue(dayName, "http://hl7.org/fhir/days-of-week", fallbackValue: dayName),
                            optionalClass: "available-time-badge"),
                        new LineBreak(),
                        new Concat(dayWidget.Value, new LineBreak()),
                    ], ContainerType.Div, "text-center")
                );
            }
        }

        return outputWidgets.RenderConcatenatedResult(navigator, renderer, context);
    }

    private enum DayOfWeek
    {
        [UsedImplicitly] [EnumMember(Value = "mon")] Monday,
        [UsedImplicitly] [EnumMember(Value = "tue")] Tuesday,
        [UsedImplicitly] [EnumMember(Value = "wed")] Wednesday,
        [UsedImplicitly] [EnumMember(Value = "thu")] Thursday,
        [UsedImplicitly] [EnumMember(Value = "fri")] Friday,
        [UsedImplicitly] [EnumMember(Value = "sat")] Saturday,
        [UsedImplicitly] [EnumMember(Value = "sun")] Sunday,
    }
}