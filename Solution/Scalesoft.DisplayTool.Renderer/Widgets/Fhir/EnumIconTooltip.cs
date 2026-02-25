using System.Runtime.Serialization;
using System.Xml.XPath;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class EnumIconTooltip(string nodePath, string enumDefinitionUri, Widget title, Widget? valueToDisplay = null)
    : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var node = navigator.SelectSingleNode(nodePath);
        if (node.Node == null)
        {
            return new ParseError
            {
                Kind = ErrorKind.MissingValue,
                Message = $"Could not find value for enum {enumDefinitionUri}",
                Path = node.GetFullPath(),
                Severity = ErrorSeverity.Warning,
            };
        }

        var enumValue = node.Node.NodeType == XPathNodeType.Attribute
            ? node.Node.Value
            : node.SelectSingleNode("@value").Node?.Value;

        if (enumValue == null)
        {
            return new ParseError
            {
                Kind = ErrorKind.MissingValue,
                Message = $"Could not find value for enum {enumDefinitionUri}",
                Path = node.GetFullPath(),
                Severity = ErrorSeverity.Warning,
            };
        }

        var displayWidget = new EnumLabel(nodePath, enumDefinitionUri);

        var enumValueParsed = enumValue.ToLower().ToEnum<SupportedCodes>();
        if (enumValueParsed == null || !TryGetIcon(enumValueParsed.Value, enumDefinitionUri, out var iconTuple))
        {
            return await new Tooltip([],
                [
                    new NameValuePair([title], [valueToDisplay ?? displayWidget])
                ],
                [new HideableDetails(ContainerType.Span, new ConstantText(enumDefinitionUri))],
                icon: new Icon(SupportedIcons.TooltipInfo, " enum-tooltip-icon")
            ).Render(navigator, renderer, context);
        }

        if (iconTuple == null)
        {
            return RenderResult.NullResult;
        }

        var icon = iconTuple.Value.icon;
        var iconClass = iconTuple.Value.iconClass.ToEnumString();

        var tooltip = new Tooltip(
            [],
            [
                new NameValuePair([title], [valueToDisplay ?? displayWidget])
            ],
            [new HideableDetails(ContainerType.Span, new ConstantText(enumDefinitionUri))],
            icon: new Icon(icon, iconClass + " enum-tooltip-icon")
        );


        return await tooltip.Render(navigator, renderer, context);
    }

    public static bool TryGetIcon(
        SupportedCodes code,
        string uri,
        out (SupportedIcons icon, SupportedIconClasses iconClass)? iconTuple
    )
    {
        return m_iconMap.TryGetValue((code, uri), out iconTuple)
               || m_iconMap.TryGetValue((code, ""), out iconTuple);
    }

    private static readonly Dictionary<(SupportedCodes, string), (SupportedIcons icon, SupportedIconClasses iconClass)?>
        m_iconMap =
            new()
            {
                {
                    (SupportedCodes.Active, "http://terminology.hl7.org/CodeSystem/allergyintolerance-clinical"),
                    (SupportedIcons.TriangleExclamation, SupportedIconClasses.RedDangerIcon)
                },
                {
                    (SupportedCodes.Active, ""),
                    null
                },
                {
                    (SupportedCodes.Amended, ""),
                    (SupportedIcons.Wrench, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.Arrived, ""),
                    (SupportedIcons.Truck, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.Available, ""),
                    (SupportedIcons.Star, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.Booked, ""),
                    (SupportedIcons.ClipboardList, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.Cancelled, ""),
                    (SupportedIcons.SquareXMark, SupportedIconClasses.RedDangerIcon)
                },
                {
                    (SupportedCodes.CheckedIn, ""),
                    (SupportedIcons.LocationDot, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.Completed, ""),
                    null
                },
                {
                    (SupportedCodes.Confirmed, ""),
                    (SupportedIcons.EnvelopeOpenText, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.Declined, ""),
                    (SupportedIcons.ThumbsDown, SupportedIconClasses.RedDangerIcon)
                },
                {
                    (SupportedCodes.Draft, ""),
                    (SupportedIcons.PenNib, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.EnteredInError, ""),
                    (SupportedIcons.Bug, SupportedIconClasses.RedDangerIcon)
                },
                {
                    (SupportedCodes.Final, ""),
                    null
                },
                {
                    (SupportedCodes.Fulfilled, ""),
                    (SupportedIcons.ListUl, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.High, ""),
                    (SupportedIcons.ChartLine, SupportedIconClasses.RedDangerIcon)
                },
                {
                    (SupportedCodes.Inactive, "http://hl7.org/fhir/ValueSet/allergyintolerance-clinical"),
                    (SupportedIcons.CircleMinus, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.Inactive, ""),
                    (SupportedIcons.CircleXMark, SupportedIconClasses.OrangeWarningIcon)
                },
                {
                    (SupportedCodes.InProgress, ""),
                    (SupportedIcons.Spinner, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.Low, ""),
                    (SupportedIcons.ChartGantt, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.Mild, ""),
                    (SupportedIcons.MildCircles, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.MildCondition, ""),
                    (SupportedIcons.MildCircles, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.Severe, ""),
                    (SupportedIcons.SevereCircles, SupportedIconClasses.RedDangerIcon)
                },
                {
                    (SupportedCodes.SevereCondition, ""),
                    (SupportedIcons.SevereCircles, SupportedIconClasses.RedDangerIcon)
                },
                {
                    (SupportedCodes.Moderate, ""),
                    (SupportedIcons.ModerateCircles, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.ModerateCondition, ""),
                    (SupportedIcons.ModerateCircles, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.NoShow, ""),
                    (SupportedIcons.UserXMark, SupportedIconClasses.OrangeWarningIcon)
                },
                {
                    (SupportedCodes.OnHold, ""),
                    (SupportedIcons.CirclePause, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.Pending, ""),
                    (SupportedIcons.CircleNotch, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.Preliminary, ""),
                    (SupportedIcons.PenToSquare, SupportedIconClasses.OrangeWarningIcon)
                },
                {
                    (SupportedCodes.Preparation, ""),
                    (SupportedIcons.Pencil, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.Presumed, ""),
                    (SupportedIcons.Lightbulb, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.Proposed, ""),
                    (SupportedIcons.FileHand, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.Refuted, ""),
                    (SupportedIcons.CircleMinus, SupportedIconClasses.OrangeWarningIcon)
                },
                {
                    (SupportedCodes.Resolved, ""),
                    (SupportedIcons.MagnifyingGlassCheck, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.Revoked, ""),
                    (SupportedIcons.ShieldXMark, SupportedIconClasses.OrangeWarningIcon)
                },
                {
                    (SupportedCodes.Stopped, ""),
                    (SupportedIcons.CircleStop, SupportedIconClasses.RedDangerIcon)
                },
                {
                    (SupportedCodes.UnableToAssess, ""),
                    (SupportedIcons.CircleQuestion, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.Unavailable, ""),
                    (SupportedIcons.StarSlash, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.Unconfirmed, ""),
                    (SupportedIcons.FileCircleQuestion, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.Unknown, ""),
                    (SupportedIcons.CircleQuestion, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.Unsatisfactory, ""),
                    (SupportedIcons.StarHalfStroke, SupportedIconClasses.OrangeWarningIcon)
                },
                {
                    (SupportedCodes.Waitlist, ""),
                    (SupportedIcons.HourglassHalf, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.Scheduled, ""),
                    (SupportedIcons.CalendarDays, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.Registered, ""),
                    (SupportedIcons.Registered, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.Corrected, ""),
                    (SupportedIcons.PenRuler, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.Accepted, ""),
                    (SupportedIcons.UserCheck, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.Tentative, ""),
                    (SupportedIcons.UserQuestion, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.NeedsAction, ""),
                    (SupportedIcons.UserExclamation, SupportedIconClasses.RedDangerIcon)
                },
                {
                    (SupportedCodes.HighPriority, ""),
                    (SupportedIcons.HighPyramid, SupportedIconClasses.RedDangerIcon)
                },
                {
                    (SupportedCodes.MediumPriority, ""),
                    (SupportedIcons.MediumPyramid, SupportedIconClasses.YellowWarningIcon)
                },
                {
                    (SupportedCodes.LowPriority, ""),
                    (SupportedIcons.LowPyramid, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.Rejected, ""),
                    (SupportedIcons.HandXMark, SupportedIconClasses.RedDangerIcon)
                },
                {
                    (SupportedCodes.Planned, ""),
                    (SupportedIcons.Calendar, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.LaboratoryTrendB, ""),
                    (SupportedIcons.FaceSmile, SupportedIconClasses.GreenSuccessIcon)
                },
                {
                    (SupportedCodes.LaboratoryTrendD, ""),
                    (SupportedIcons.StackedChevronDown, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.LaboratoryTrendU, ""),
                    (SupportedIcons.StackedChevronUp, SupportedIconClasses.BlueInfoIcon)
                },
                {
                    (SupportedCodes.LaboratoryTrendW, ""),
                    (SupportedIcons.FaceFrown, SupportedIconClasses.RedDangerIcon)
                },
            };
}

public enum SupportedCodes
{
    [EnumMember(Value = "active")] Active,
    [EnumMember(Value = "amended")] Amended,
    [EnumMember(Value = "arrived")] Arrived,
    [EnumMember(Value = "available")] Available,
    [EnumMember(Value = "booked")] Booked,
    [EnumMember(Value = "cancelled")] Cancelled,
    [EnumMember(Value = "checked-in")] CheckedIn,
    [EnumMember(Value = "completed")] Completed,
    [EnumMember(Value = "confirmed")] Confirmed,
    [EnumMember(Value = "declined")] Declined,
    [EnumMember(Value = "draft")] Draft,

    [EnumMember(Value = "entered-in-error")]
    EnteredInError,
    [EnumMember(Value = "final")] Final,
    [EnumMember(Value = "fulfilled")] Fulfilled,
    [EnumMember(Value = "high")] High,
    [EnumMember(Value = "inactive")] Inactive,
    [EnumMember(Value = "in-progress")] InProgress,
    [EnumMember(Value = "low")] Low,
    [EnumMember(Value = "mild")] Mild,
    [EnumMember(Value = "255604002")] MildCondition,
    [EnumMember(Value = "moderate")] Moderate,
    [EnumMember(Value = "6736007")] ModerateCondition,
    [EnumMember(Value = "severe")] Severe,
    [EnumMember(Value = "24484000")] SevereCondition,
    [EnumMember(Value = "noshow")] NoShow,
    [EnumMember(Value = "on-hold")] OnHold,
    [EnumMember(Value = "pending")] Pending,
    [EnumMember(Value = "preliminary")] Preliminary,
    [EnumMember(Value = "preparation")] Preparation,
    [EnumMember(Value = "presumed")] Presumed,
    [EnumMember(Value = "proposed")] Proposed,
    [EnumMember(Value = "refuted")] Refuted,
    [EnumMember(Value = "resolved")] Resolved,
    [EnumMember(Value = "revoked")] Revoked,
    [EnumMember(Value = "stopped")] Stopped,

    [EnumMember(Value = "unable-to-asses")]
    UnableToAssess,
    [EnumMember(Value = "unavailable")] Unavailable,
    [EnumMember(Value = "unconfirmed")] Unconfirmed,
    [EnumMember(Value = "unknown")] Unknown,
    [EnumMember(Value = "unsatisfactory")] Unsatisfactory,
    [EnumMember(Value = "waitlist")] Waitlist,
    [EnumMember(Value = "scheduled")] Scheduled,
    [EnumMember(Value = "registered")] Registered,
    [EnumMember(Value = "corrected")] Corrected,
    [EnumMember(Value = "accepted")] Accepted,
    [EnumMember(Value = "tentative")] Tentative,
    [EnumMember(Value = "needs-action")] NeedsAction,
    [EnumMember(Value = "high-priority")] HighPriority,

    [EnumMember(Value = "medium-priority")]
    MediumPriority,
    [EnumMember(Value = "low-priority")] LowPriority,
    [EnumMember(Value = "rejected")] Rejected,
    [EnumMember(Value = "planned")] Planned,
    [EnumMember(Value = "b")] LaboratoryTrendB,
    [EnumMember(Value = "d")] LaboratoryTrendD,
    [EnumMember(Value = "u")] LaboratoryTrendU,
    [EnumMember(Value = "w")] LaboratoryTrendW,
}

public enum SupportedIconClasses
{
    [EnumMember(Value = "red-danger-icon")]
    RedDangerIcon,

    [EnumMember(Value = "green-success-icon")]
    GreenSuccessIcon,

    [EnumMember(Value = "orange-warning-icon")]
    OrangeWarningIcon,

    [EnumMember(Value = "yellow-warning-icon")]
    YellowWarningIcon,
    [EnumMember(Value = "blue-info-icon")] BlueInfoIcon,
}