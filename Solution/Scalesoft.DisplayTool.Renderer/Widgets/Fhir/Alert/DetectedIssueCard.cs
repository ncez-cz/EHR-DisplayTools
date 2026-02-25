using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert;

public class DetectedIssueCard : AlternatingBackgroundColumnResourceBase<DetectedIssueCard>, IResourceWidget
{
    public static string ResourceType => "DetectedIssue";
    public static bool HasBorderedContainer(Widget widget) => false;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        var summaryItems = new List<Widget>();
        if (item.EvaluateCondition("f:code"))
        {
            summaryItems.Add(new ChangeContext(item, "f:code", new CodeableConcept()));
        }

        if (summaryItems.Count == 0)
        {
            return null;
        }

        return new ResourceSummaryModel
        {
            Value = new Container(summaryItems, ContainerType.Span),
        };
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<DetectedIssueInfrequentProperties>(navigator);

        var mitigationContext = navigator.SelectSingleNode("f:mitigation");
        var mitigationInfrequentProperties =
            InfrequentProperties.Evaluate<MitigationInfrequentProperties>(mitigationContext);

        var referenceLinkValue = navigator.SelectSingleNode("f:reference/@value").Node?.Value ?? "";

        Widget[] result =
        [
            new Column([
                new Row([
                    new Heading(
                    [
                        new TextContainer(TextStyle.Bold, [
                            new If(_ => infrequentProperties.Contains(DetectedIssueInfrequentProperties.Code),
                                new ChangeContext("f:code", new CodeableConcept())
                            ).Else(new LocalNodeName(navigator.Node?.LocalName)),
                        ]),
                    ], HeadingSize.H5, customClass: "m-0 blue-color"),
                    new EnumIconTooltip("f:status",
                        "http://hl7.org/fhir/ValueSet/fm-status",
                        new EhdsiDisplayLabel(LabelCodes.Status)),
                    new EnumIconTooltip("f:severity",
                        "https://hl7.org/fhir/R4/valueset-detectedissue-severity.html",
                        new EhdsiDisplayLabel(LabelCodes.Severity)),
                    new NarrativeModal(alignRight: false),
                ], flexContainerClasses: "gap-1 align-items-center", flexWrap: false),
                new Row([
                    infrequentProperties.Optional(DetectedIssueInfrequentProperties.Detail,
                        new NameValuePair(
                            [new LocalizedLabel("general.detail")],
                            [new Text("@value")],
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Primary
                        )
                    ),
                ], flexContainerClasses: "gap-1 align-items-center"),
                infrequentProperties.Optional(DetectedIssueInfrequentProperties.Mitigation,
                    new NameValuePair(
                        [new LocalizedLabel("detected-issue.mitigation")],
                        [
                            new Container([
                                mitigationInfrequentProperties.Optional(MitigationInfrequentProperties.Action,
                                    new NameValuePair(
                                        new LocalizedLabel("detected-issue.mitigation.action"),
                                        new CodeableConcept(),
                                        direction: FlexDirection.Row,
                                        style: NameValuePair.NameValuePairStyle.Secondary
                                    )
                                ),
                                mitigationInfrequentProperties.Optional(MitigationInfrequentProperties.Date,
                                    new NameValuePair(
                                        new LocalizedLabel("detected-issue.mitigation.date"),
                                        new ShowDateTime(),
                                        direction: FlexDirection.Row,
                                        style: NameValuePair.NameValuePairStyle.Secondary
                                    )
                                ),
                                mitigationInfrequentProperties.Optional(MitigationInfrequentProperties.Author,
                                    new AnyReferenceNamingWidget(
                                        widgetModel: new ReferenceNamingWidgetModel
                                        {
                                            Type = ReferenceNamingWidgetType.NameValuePair,
                                            Direction = FlexDirection.Row,
                                            Style = NameValuePair.NameValuePairStyle.Secondary,
                                            LabelOverride = new LocalizedLabel("detected-issue.mitigation.author"),
                                        }
                                    )
                                ),
                            ], optionalClass: "name-value-pair-wrapper w-fit-content"),
                        ],
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    )
                ),
                new Row([
                    new InfrequentProperties.Builder<DetectedIssueInfrequentProperties>(
                        infrequentProperties,
                        DetectedIssueInfrequentProperties.Implicated,
                        items =>
                        [
                            new Column([
                                new NameValuePair(
                                    [new LocalizedLabel("detected-issue.implicated")],
                                    [
                                        new ListBuilder(items, FlexDirection.Column, (_, _) =>
                                        [
                                            new AnyReferenceNamingWidget(),
                                        ], flexContainerClasses: "row-gap-0"),
                                    ],
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary
                                ),
                            ]),
                        ]),
                    new If(_ => infrequentProperties.ContainsAnyOf(
                            DetectedIssueInfrequentProperties.Author,
                            DetectedIssueInfrequentProperties.Patient),
                        children:
                        [
                            new NameValuePair(
                                [new LocalizedLabel("general.involvedParties")],
                                [
                                    new Container([
                                        infrequentProperties.Optional(DetectedIssueInfrequentProperties.Author,
                                            new AnyReferenceNamingWidget(
                                                widgetModel: new ReferenceNamingWidgetModel
                                                {
                                                    Type = ReferenceNamingWidgetType.NameValuePair,
                                                    Direction = FlexDirection.Row,
                                                    Style = NameValuePair.NameValuePairStyle.Secondary,
                                                    LabelOverride = new LocalizedLabel("detected-issue.author"),
                                                }
                                            )
                                        ),
                                        infrequentProperties.Optional(DetectedIssueInfrequentProperties.Patient,
                                            new AnyReferenceNamingWidget(
                                                widgetModel: new ReferenceNamingWidgetModel
                                                {
                                                    Type = ReferenceNamingWidgetType.NameValuePair,
                                                    Direction = FlexDirection.Row,
                                                    Style = NameValuePair.NameValuePairStyle.Secondary,
                                                    LabelOverride = new LocalizedLabel("detected-issue.patient"),
                                                }
                                            )
                                        ),
                                    ], optionalClass: "name-value-pair-wrapper w-fit-content"),
                                ],
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            ),
                        ]
                    ),
                    new If(_ => infrequentProperties.ContainsAnyOf(
                            DetectedIssueInfrequentProperties.Identifier,
                            DetectedIssueInfrequentProperties.IdentifiedDateTime,
                            DetectedIssueInfrequentProperties.IdentifiedPeriod,
                            DetectedIssueInfrequentProperties.Evidence,
                            DetectedIssueInfrequentProperties.Reference),
                        children:
                        [
                            new NameValuePair(
                                [new LocalizedLabel("general.additional-info")],
                                [
                                    new Container([
                                        infrequentProperties.Optional(DetectedIssueInfrequentProperties.Identifier,
                                            new HideableDetails(
                                                new NameValuePair(
                                                    [new LocalizedLabel("general.identifier")],
                                                    [new ShowIdentifier()],
                                                    direction: FlexDirection.Row,
                                                    style: NameValuePair.NameValuePairStyle.Secondary
                                                )
                                            )
                                        ),
                                        infrequentProperties.Optional(
                                            DetectedIssueInfrequentProperties.IdentifiedDateTime,
                                            new NameValuePair(
                                                new EhdsiDisplayLabel(LabelCodes.Date),
                                                new ShowDateTime(),
                                                direction: FlexDirection.Row,
                                                style: NameValuePair.NameValuePairStyle.Secondary
                                            )
                                        ),
                                        infrequentProperties.Optional(DetectedIssueInfrequentProperties.Evidence,
                                            new NameValuePair(
                                                new LocalizedLabel("detected-issue.evidence"),
                                                new CodeableConcept(),
                                                direction: FlexDirection.Row,
                                                style: NameValuePair.NameValuePairStyle.Secondary
                                            )
                                        ),
                                        infrequentProperties.Optional(DetectedIssueInfrequentProperties.Reference,
                                            new NameValuePair(
                                                new LocalizedLabel("detected-issue.reference"),
                                                new Link(
                                                    new LocalizedLabel("general.link"),
                                                    referenceLinkValue
                                                ),
                                                direction: FlexDirection.Row,
                                                style: NameValuePair.NameValuePairStyle.Secondary
                                            )
                                        ),
                                    ], optionalClass: "name-value-pair-wrapper w-fit-content"),
                                ],
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            ),
                        ]
                    ),
                ]),
            ], flexContainerClasses: "row-gap-2"),
        ];

        return result.RenderConcatenatedResult(navigator, renderer, context);
    }

    public enum DetectedIssueInfrequentProperties
    {
        Code,
        Detail,
        Implicated,
        Patient,
        Author,
        Reference,
        Identifier,
        IdentifiedDateTime,
        IdentifiedPeriod,
        Evidence,
        Mitigation,
    }

    public enum MitigationInfrequentProperties
    {
        Author,
        Action,
        Date,
    }
}