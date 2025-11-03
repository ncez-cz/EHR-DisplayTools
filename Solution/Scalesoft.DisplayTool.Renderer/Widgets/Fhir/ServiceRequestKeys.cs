namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ServiceRequestKeys
{
    public class ServiceRequestKey
    {
        public required OccurenceKey? Occurence { get; init; }

        public required DateTimeKey? AuthoredOn { get; init; }

        public required string? Priority { get; init; }

        // Do not deduplicate by code, as it is used for titles and narrative button could be potentially left without text
        //public required CodeableConceptKey? Code { get; init; }

        public required List<ReferenceKey> Performer { get; init; } = [];

        public required List<ReferenceKey> ReasonReference { get; init; } = [];

        public required List<CodeableConceptKey> ReasonCode { get; init; } = [];

        public required List<ReferenceKey> Insurance { get; init; } = [];

        public required List<CodeableConceptKey> BodySite { get; init; } = [];

        public required List<AnnotationKey> Note { get; init; } = [];

        public HashSet<ServiceRequestProperties> Compare(ServiceRequestKey? other)
        {
            if (other is null)
            {
                return [];
            }

            var result = new HashSet<ServiceRequestProperties>();
            if (Equals(Occurence, other.Occurence))
            {
                result.Add(ServiceRequestProperties.Occurrence);
            }

            if (Equals(AuthoredOn, other.AuthoredOn))
            {
                result.Add(ServiceRequestProperties.AuthoredOn);
            }

            if (Equals(Priority, other.Priority))
            {
                result.Add(ServiceRequestProperties.Priority);
            }

            if (Performer.SequenceEqual(other.Performer))
            {
                result.Add(ServiceRequestProperties.Performer);
            }

            if (ReasonReference.SequenceEqual(other.ReasonReference))
            {
                result.Add(ServiceRequestProperties.ReasonReference);
            }

            if (ReasonCode.SequenceEqual(other.ReasonCode))
            {
                result.Add(ServiceRequestProperties.ReasonCode);
            }

            if (Insurance.SequenceEqual(other.Insurance))
            {
                result.Add(ServiceRequestProperties.Insurance);
            }

            if (BodySite.SequenceEqual(other.BodySite))
            {
                result.Add(ServiceRequestProperties.BodySite);
            }

            if (Note.SequenceEqual(other.Note))
            {
                result.Add(ServiceRequestProperties.Note);
            }

            return result;
        }
    }

    public record ReferenceKey
    {
        public required string? Reference { get; init; }

        public required string? Type { get; init; }

        public required IdentifierKey? Identifier { get; init; }

        public required string? Display { get; init; }
    }

    public record IdentifierKey
    {
        public required string? Use { get; init; }

        public required CodeableConceptKey? Type { get; init; }

        public required string? System { get; init; }

        public required string? Value { get; init; }

        public required PeriodKey? Period { get; init; }

        public required ReferenceKey? Assigner { get; init; }
    }

    public record CodingKey
    {
        public required string? System { get; init; }

        public required string? Version { get; init; }

        public required string? Code { get; init; }

        public required string? Display { get; init; }

        public required string? UserSelected { get; init; }
    }

    public class CodeableConceptKey : IEquatable<CodeableConceptKey>
    {
        public required List<CodingKey> Coding { get; init; } = [];

        public required string? Text { get; init; }

        public bool Equals(CodeableConceptKey? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Coding.SequenceEqual(other.Coding) && Text == other.Text;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CodeableConceptKey)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Coding, Text);
        }
    }

    public record OccurenceKey
    {
        public required DateTimeKey? DateTime { get; init; }
        public required PeriodKey? Period { get; init; }
        public required TimingKey? Timing { get; init; }
    }

    public record DateTimeKey
    {
        public required string? Value { get; init; }
    }

    public record PeriodKey
    {
        public required DateTimeKey? Start { get; init; }
        public required DateTimeKey? End { get; init; }
    }

    public record TimingKey
    {
        public required List<DateTimeKey> Event { get; init; }

        public required TimingRepeatKey? Repeat { get; init; }

        public required CodeableConceptKey? Code { get; init; }
    }

    public class TimingRepeatKey : IEquatable<TimingRepeatKey>
    {
        public required TimingRepeatBoundsKey? Bounds { get; init; }
        public required string? Count { get; init; }
        public required string? CountMax { get; init; }
        public required string? Duration { get; init; }
        public required string? DurationMax { get; init; }
        public required string? DurationUnit { get; init; }
        public required string? Freqency { get; init; }
        public required string? FreqencyMax { get; init; }
        public required string? Period { get; init; }
        public required string? PeriodMax { get; init; }
        public required string? PeriodUnit { get; init; }
        public required List<string> DayOfWeek { get; init; }
        public required List<string> TimeOfDay { get; init; }
        public required List<string> When { get; init; }
        public required string? Offset { get; init; }

        public bool Equals(TimingRepeatKey? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Bounds, other.Bounds) && Count == other.Count && CountMax == other.CountMax &&
                   Duration == other.Duration && DurationMax == other.DurationMax &&
                   DurationUnit == other.DurationUnit && Freqency == other.Freqency &&
                   FreqencyMax == other.FreqencyMax && Period == other.Period && PeriodMax == other.PeriodMax &&
                   PeriodUnit == other.PeriodUnit && DayOfWeek.SequenceEqual(other.DayOfWeek) &&
                   TimeOfDay.SequenceEqual(other.TimeOfDay) && When.SequenceEqual(other.When) && Offset == other.Offset;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TimingRepeatKey)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Bounds);
            hashCode.Add(Count);
            hashCode.Add(CountMax);
            hashCode.Add(Duration);
            hashCode.Add(DurationMax);
            hashCode.Add(DurationUnit);
            hashCode.Add(Freqency);
            hashCode.Add(FreqencyMax);
            hashCode.Add(Period);
            hashCode.Add(PeriodMax);
            hashCode.Add(PeriodUnit);
            hashCode.Add(DayOfWeek);
            hashCode.Add(TimeOfDay);
            hashCode.Add(When);
            hashCode.Add(Offset);
            return hashCode.ToHashCode();
        }
    }

    public record TimingRepeatBoundsKey
    {
        public required DurationKey? Duration { get; init; }
        public required RangeKey? Range { get; init; }
        public required PeriodKey? Period { get; init; }
    }

    public record QuantityKey
    {
        public required string? Value { get; init; }
        public required string? Comparator { get; init; }
        public required string? Unit { get; init; }
        public required string? System { get; init; }
        public required string? Code { get; init; }
    }

    public record DurationKey : QuantityKey
    {
    }

    public record RangeKey
    {
        public required QuantityKey? Low { get; init; }
        public required QuantityKey? High { get; init; }
    }

    public record AnnotationKey
    {
        public required string? Text { get; init; }
        public required ReferenceKey? AuthorReference { get; init; }
        public required string? Author { get; init; }
        public required DateTimeKey? Time { get; init; }
    }
}