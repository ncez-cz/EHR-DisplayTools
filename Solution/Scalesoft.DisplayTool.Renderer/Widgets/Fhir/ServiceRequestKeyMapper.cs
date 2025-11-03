using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public static class ServiceRequestKeyMapper
{
    public static ServiceRequestKeys.ServiceRequestKey MapRequestKey(XmlDocumentNavigator navigator)
    {
        return new ServiceRequestKeys.ServiceRequestKey
        {
            AuthoredOn = MapDateTimeKey(navigator.SelectSingleNode("f:authoredOn")),
            Priority = navigator.SelectSingleNode("f:priority/@value").Node?.Value,
            Insurance = navigator.SelectAllNodes("f:insurance").Select(MapReferenceKey).WhereNotNull().ToList(),
            Occurence = MapOccurenceKey(navigator),
            Performer = navigator.SelectAllNodes("f:performer").Select(MapReferenceKey).WhereNotNull().ToList(),
            ReasonReference = navigator.SelectAllNodes("f:reasonReference").Select(MapReferenceKey).WhereNotNull()
                .ToList(),
            ReasonCode = navigator.SelectAllNodes("f:reasonCode").Select(MapCodeableConceptKey).WhereNotNull().ToList(),
            BodySite = navigator.SelectAllNodes("f:bodySite").Select(MapCodeableConceptKey).WhereNotNull().ToList(),
            Note = navigator.SelectAllNodes("f:note").Select(MapAnnotationKey).WhereNotNull().ToList(),
        };
    }

    private static ServiceRequestKeys.ReferenceKey? MapReferenceKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.ReferenceKey
        {
            Display = navigator.SelectSingleNode("f:display/@value").Node?.Value,
            Reference = navigator.SelectSingleNode("f:reference/@value").Node?.Value,
            Type = navigator.SelectSingleNode("f:type/@value").Node?.Value,
            Identifier = MapIdentifierKey(navigator.SelectSingleNode("f:identifier")),
        };
    }

    private static ServiceRequestKeys.IdentifierKey? MapIdentifierKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.IdentifierKey
        {
            Use = navigator.SelectSingleNode("f:use/@value").Node?.Value,
            Type = MapCodeableConceptKey(navigator.SelectSingleNode("f:type")),
            System = navigator.SelectSingleNode("f:system/@value").Node?.Value,
            Value = navigator.SelectSingleNode("f:value/@value").Node?.Value,
            Period = MapPeriodKey(navigator.SelectSingleNode("f:period")),
            Assigner = MapReferenceKey(navigator.SelectSingleNode("f:assigner")),
        };
    }

    private static ServiceRequestKeys.CodingKey? MapCodingKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.CodingKey
        {
            Code = navigator.SelectSingleNode("f:code/@value").Node?.Value,
            System = navigator.SelectSingleNode("f:system/@value").Node?.Value,
            Display = navigator.SelectSingleNode("f:display/@value").Node?.Value,
            UserSelected = navigator.SelectSingleNode("f:userSelected/@value").Node?.Value,
            Version = navigator.SelectSingleNode("f:version/@value").Node?.Value,
        };
    }

    private static ServiceRequestKeys.CodeableConceptKey? MapCodeableConceptKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.CodeableConceptKey
        {
            Text = navigator.SelectSingleNode("f:text/@value").Node?.Value,
            Coding = navigator.SelectAllNodes("f:coding").Select(MapCodingKey).WhereNotNull().ToList(),
        };
    }

    private static ServiceRequestKeys.OccurenceKey? MapOccurenceKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        var occurrenceDateTimeNav = navigator.SelectSingleNode("f:occurrenceDateTime");
        var occurrencePeriodNav = navigator.SelectSingleNode("f:occurrencePeriod");
        var occurrenceTimingNav = navigator.SelectSingleNode("f:occurrenceTiming");
        if (occurrenceDateTimeNav.Node == null && occurrencePeriodNav.Node == null && occurrenceTimingNav.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.OccurenceKey
        {
            DateTime = MapDateTimeKey(occurrenceDateTimeNav),
            Period = MapPeriodKey(occurrencePeriodNav),
            Timing = MapTimingKey(occurrenceTimingNav),
        };
    }

    private static ServiceRequestKeys.DateTimeKey? MapDateTimeKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.DateTimeKey
        {
            Value = navigator.SelectSingleNode("f:value/@value").Node?.Value,
        };
    }

    private static ServiceRequestKeys.PeriodKey? MapPeriodKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.PeriodKey
        {
            Start = MapDateTimeKey(navigator.SelectSingleNode("f:start")),
            End = MapDateTimeKey(navigator.SelectSingleNode("f:end")),
        };
    }

    private static ServiceRequestKeys.TimingKey? MapTimingKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.TimingKey
        {
            Event = navigator.SelectAllNodes("f:event").Select(MapDateTimeKey).WhereNotNull().ToList(),
            Repeat = MapTimingRepeatKey(navigator.SelectSingleNode("f:repeat")),
            Code = MapCodeableConceptKey(navigator.SelectSingleNode("f:code")),
        };
    }

    private static ServiceRequestKeys.TimingRepeatKey? MapTimingRepeatKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.TimingRepeatKey
        {
            Bounds = MapTimingRepeatBoundsKey(navigator.SelectSingleNode("f:bounds")),
            Count = navigator.SelectSingleNode("f:count/@value").Node?.Value,
            CountMax = navigator.SelectSingleNode("f:countMax/@value").Node?.Value,
            Duration = navigator.SelectSingleNode("f:duration/@value").Node?.Value,
            DurationMax = navigator.SelectSingleNode("f:durationMax/@value").Node?.Value,
            DurationUnit = navigator.SelectSingleNode("f:durationUnit/@value").Node?.Value,
            Freqency = navigator.SelectSingleNode("f:frequency/@value").Node?.Value,
            FreqencyMax = navigator.SelectSingleNode("f:frequencyMax/@value").Node?.Value,
            Period = navigator.SelectSingleNode("f:period/@value").Node?.Value,
            PeriodMax = navigator.SelectSingleNode("f:periodMax/@value").Node?.Value,
            PeriodUnit = navigator.SelectSingleNode("f:periodUnit/@value").Node?.Value,
            DayOfWeek = navigator.SelectAllNodes("f:dayOfWeek/@value").Select(x => x.Node!.Value).ToList(),
            TimeOfDay = navigator.SelectAllNodes("f:timeOfDay/@value").Select(x => x.Node!.Value).ToList(),
            When = navigator.SelectAllNodes("f:when/@value").Select(x => x.Node!.Value).ToList(),
            Offset = navigator.SelectSingleNode("f:offset/@value").Node?.Value,
        };
    }

    private static ServiceRequestKeys.TimingRepeatBoundsKey? MapTimingRepeatBoundsKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        var boundsDurationNav = navigator.SelectSingleNode("f:boundsDuration");
        var boundsRangeNav = navigator.SelectSingleNode("f:boundsRange");
        var boundsPeriodNav = navigator.SelectSingleNode("f:boundsPeriod");

        if (boundsDurationNav.Node == null && boundsRangeNav.Node == null && boundsPeriodNav.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.TimingRepeatBoundsKey
        {
            Duration = MapDurationKey(boundsDurationNav),
            Range = MapRangeKey(boundsRangeNav),
            Period = MapPeriodKey(boundsPeriodNav),
        };
    }

    private static ServiceRequestKeys.QuantityKey? MapQuantityKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.QuantityKey
        {
            Value = navigator.SelectSingleNode("f:value/@value").Node?.Value,
            Comparator = navigator.SelectSingleNode("f:comparator/@value").Node?.Value,
            Unit = navigator.SelectSingleNode("f:unit/@value").Node?.Value,
            System = navigator.SelectSingleNode("f:system/@value").Node?.Value,
            Code = navigator.SelectSingleNode("f:code/@value").Node?.Value,
        };
    }

    private static ServiceRequestKeys.DurationKey? MapDurationKey(XmlDocumentNavigator navigator)
    {
        return (ServiceRequestKeys.DurationKey?)MapQuantityKey(navigator);
    }

    private static ServiceRequestKeys.RangeKey? MapRangeKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.RangeKey
        {
            High = MapQuantityKey(navigator.SelectSingleNode("f:high")),
            Low = MapQuantityKey(navigator.SelectSingleNode("f:low")),
        };
    }

    private static ServiceRequestKeys.AnnotationKey? MapAnnotationKey(XmlDocumentNavigator navigator)
    {
        if (navigator.Node == null)
        {
            return null;
        }

        return new ServiceRequestKeys.AnnotationKey
        {
            Author = navigator.SelectSingleNode("f:authorString").Node?.Value,
            AuthorReference = MapReferenceKey(navigator.SelectSingleNode("f:authorReference")),
            Time = MapDateTimeKey(navigator.SelectSingleNode("f:time")),
            Text = navigator.SelectSingleNode("f:text/@value").Node?.Value,
        };
    }
}