using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Communication(bool skipIdPopulation = true)
    : AlternatingBackgroundColumnResourceBase<Communication>, IResourceWidget
{
    public Communication() : this(true)
    {
    }

    public static string ResourceType => "Communication";

    public static bool RequiresExternalTitle => true;

    public static bool HasBorderedContainer(Widget widget) => false;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        var summaryItems = new List<Widget>();
        if (item.EvaluateCondition("f:topic"))
        {
            summaryItems.Add(new ChangeContext(item, "f:topic", new CodeableConcept()));
        }

        if (item.EvaluateCondition("f:payload"))
        {
            foreach (var navigator in item.SelectAllNodes("f:payload"))
            {
                summaryItems.Add(new ChangeContext(navigator, new OpenTypeElement(null, "content")));
            }
        }

        if (summaryItems.Count == 0)
        {
            return null;
        }

        var result = summaryItems.Intersperse(new ConstantText(" ")).ToArray();

        return new ResourceSummaryModel
        {
            Value = new Container(result, ContainerType.Span),
        };
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<CommunicationInfrequentPropertiesPaths>(navigator);

        var nameValuePairs = new FlexList([
            infrequentProperties.Condition(CommunicationInfrequentPropertiesPaths.BasedOn,
                new NameValuePair(
                    new LocalizedLabel("communication.basedOn"),
                    new CommaSeparatedBuilder("f:basedOn", _ => [new AnyReferenceNamingWidget()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            infrequentProperties.Condition(CommunicationInfrequentPropertiesPaths.PartOf,
                new NameValuePair(
                    new LocalizedLabel("communication.partOf"),
                    new CommaSeparatedBuilder("f:partOf", _ => [new AnyReferenceNamingWidget()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            infrequentProperties.Condition(CommunicationInfrequentPropertiesPaths.InResponseTo,
                new NameValuePair(
                    new LocalizedLabel("communication.inResponseTo"),
                    new CommaSeparatedBuilder("f:inResponseTo", _ => [new AnyReferenceNamingWidget()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            infrequentProperties.Optional(CommunicationInfrequentPropertiesPaths.StatusReason,
                new NameValuePair(
                    new LocalizedLabel("communication.statusReason"),
                    new CodeableConcept(),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            infrequentProperties.Condition(CommunicationInfrequentPropertiesPaths.Category,
                new NameValuePair(
                    new LocalizedLabel("communication.category"),
                    new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            infrequentProperties.Optional(CommunicationInfrequentPropertiesPaths.Priority,
                new NameValuePair(
                    new LocalizedLabel("communication.priority"),
                    new EnumLabel("@value", "http://hl7.org/fhir/ValueSet/request-priority"),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            infrequentProperties.Condition(CommunicationInfrequentPropertiesPaths.Medium,
                new NameValuePair(
                    new LocalizedLabel("communication.medium"),
                    new CommaSeparatedBuilder("f:medium", _ => [new CodeableConcept()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            infrequentProperties.Optional(CommunicationInfrequentPropertiesPaths.Subject,
                new AnyReferenceNamingWidget(widgetModel: new ReferenceNamingWidgetModel
                {
                    Type = ReferenceNamingWidgetType.NameValuePair,
                    LabelOverride = new LocalizedLabel("communication.subject"), Direction = FlexDirection.Column,
                    Style = NameValuePair.NameValuePairStyle.Primary, Size = NameValuePair.NameValuePairSize.Regular,
                })
            ),
            infrequentProperties.Condition(CommunicationInfrequentPropertiesPaths.About,
                new NameValuePair(
                    new LocalizedLabel("communication.about"),
                    new CommaSeparatedBuilder("f:about", _ => [new AnyReferenceNamingWidget()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            infrequentProperties.Optional(CommunicationInfrequentPropertiesPaths.Encounter,
                new HideableDetails(
                    new NameValuePair(
                        [new LocalizedLabel("node-names.Encounter")],
                        [new AnyReferenceNamingWidget()],
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    )
                )),
            infrequentProperties.Optional(CommunicationInfrequentPropertiesPaths.Sent,
                new NameValuePair(
                    new LocalizedLabel("communication.sent"),
                    new ShowDateTime(),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            infrequentProperties.Optional(CommunicationInfrequentPropertiesPaths.Received,
                new NameValuePair(
                    new LocalizedLabel("communication.received"),
                    new ShowDateTime(),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            infrequentProperties.Condition(CommunicationInfrequentPropertiesPaths.Recipient,
                new NameValuePair(
                    new LocalizedLabel("communication.recipient"),
                    new CommaSeparatedBuilder("f:recipient", _ => [new AnyReferenceNamingWidget()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            infrequentProperties.Optional(CommunicationInfrequentPropertiesPaths.Sender,
                new NameValuePair(
                    new LocalizedLabel("communication.sender"),
                    new AnyReferenceNamingWidget(),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                )
            ),
            new If(_ => infrequentProperties.ContainsAnyOf(
                    CommunicationInfrequentPropertiesPaths.ReasonCode,
                    CommunicationInfrequentPropertiesPaths.ReasonReference),
                new NameValuePair(
                    [new LocalizedLabel("communication.reasonX")],
                    [
                        new CommaSeparatedBuilder("f:reasonCode",
                            _ => [new CodeableConcept()]),
                        new Condition("f:reasonCode and f:reasonReference", new ConstantText(", ")),
                        new ConcatBuilder("f:reasonReference",
                            _ => [new AnyReferenceNamingWidget()])
                    ],
                    direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary
                )),
            infrequentProperties.Condition(CommunicationInfrequentPropertiesPaths.Payload,
                new ConcatBuilder("f:payload", (_, _, nav) =>
                {
                    var infrequentPayloadProperties =
                        InfrequentProperties.Evaluate<CommunicationPayloadInfrequentPropertiesPaths>(nav);

                    return
                    [
                        infrequentPayloadProperties.Condition(CommunicationPayloadInfrequentPropertiesPaths.Content,
                            new NameValuePair(
                                new LocalizedLabel("communication.payload.content"),
                                new OpenTypeElement(null, "content"),
                                direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary
                            )),
                    ];
                })
            ),
        ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1");

        var resultWidget = new Concat([
            new Row([
                    new Container([
                        new TextContainer(TextStyle.Bold,
                        [
                            new If(_ => infrequentProperties.Contains(CommunicationInfrequentPropertiesPaths.Topic),
                                    new ChangeContext("f:topic", new CodeableConcept()))
                                .Else(new LocalizedLabel("communication.fallback"))
                        ]),
                    ], optionalClass: "h5 m-0 blue-color"),
                    new EnumIconTooltip("f:status", "http://hl7.org/fhir/event-status",
                        new EhdsiDisplayLabel(LabelCodes.Status)),
                    new NarrativeModal(alignRight: false),
                ], flexContainerClasses: "gap-1 align-items-center",
                idSource: skipIdPopulation ? null : new IdentifierSource(navigator),
                flexWrap: false),
            new FlexList([
                nameValuePairs,
                ThematicBreak.SurroundedThematicBreak(
                    infrequentProperties, [
                        CommunicationInfrequentPropertiesPaths.BasedOn,
                        CommunicationInfrequentPropertiesPaths.PartOf,
                        CommunicationInfrequentPropertiesPaths.InResponseTo,
                        CommunicationInfrequentPropertiesPaths.StatusReason,
                        CommunicationInfrequentPropertiesPaths.Category,
                        CommunicationInfrequentPropertiesPaths.Priority,
                        CommunicationInfrequentPropertiesPaths.Medium,
                        CommunicationInfrequentPropertiesPaths.Subject,
                        CommunicationInfrequentPropertiesPaths.Topic,
                        CommunicationInfrequentPropertiesPaths.About,
                        CommunicationInfrequentPropertiesPaths.Encounter,
                        CommunicationInfrequentPropertiesPaths.Sent,
                        CommunicationInfrequentPropertiesPaths.Received,
                        CommunicationInfrequentPropertiesPaths.Recipient,
                        CommunicationInfrequentPropertiesPaths.ReasonReference,
                        CommunicationInfrequentPropertiesPaths.ReasonCode,
                        CommunicationInfrequentPropertiesPaths.Payload,
                    ], [
                        CommunicationInfrequentPropertiesPaths.Note,
                        CommunicationInfrequentPropertiesPaths.Text,
                    ]
                ),
                new If(_ => infrequentProperties.Contains(CommunicationInfrequentPropertiesPaths.Note),
                    new NameValuePair(
                        [new LocalizedLabel("communication.note")],
                        [
                            new ConcatBuilder("f:note", _ => [new ShowAnnotationCompact()], new LineBreak()),
                        ],
                        style: NameValuePair.NameValuePairStyle.Secondary,
                        direction: FlexDirection.Row
                    )),
                new Condition("f:text", new NarrativeCollapser()),
            ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1")
        ]);

        return resultWidget.Render(navigator, renderer, context);
    }

    private enum CommunicationInfrequentPropertiesPaths
    {
        Language,
        [NarrativeDisplayType] Text,
        InstantiatesCanonical,
        InstantiatesUri,
        BasedOn,
        PartOf,
        InResponseTo,
        StatusReason,
        Status,
        Category,
        Priority,
        Medium,
        [HiddenRedundantSubjectDisplayType] Subject,
        Topic,
        About,
        [HiddenInSimpleMode] Encounter,
        Sent,
        Received,
        Recipient,
        Sender,
        ReasonCode,
        ReasonReference,
        Payload,
        Note,
    }

    private enum CommunicationPayloadInfrequentPropertiesPaths
    {
        [OpenType("content")] Content,
    }
}