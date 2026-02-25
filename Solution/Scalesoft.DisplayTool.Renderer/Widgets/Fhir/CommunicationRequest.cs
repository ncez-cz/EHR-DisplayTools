using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class CommunicationRequest : ColumnResourceBase<CommunicationRequest>, IResourceWidget
{
    public static string ResourceType => "CommunicationRequest";
    public static bool HasBorderedContainer(Widget widget) => true;

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<CommunicationRequestInfrequentProperties>(navigator);

        var headerInfo = new Container([
            new Container([
                new LocalizedLabel("communication-request"),
                new If(_ => infrequentProperties.Contains(CommunicationRequestInfrequentProperties.Category),
                    new ConstantText(" ("),
                    new ChangeContext("f:category", new CodeableConcept()),
                    new ConstantText(")")
                ),
            ], ContainerType.Span),
            new EnumIconTooltip("f:status", "http://hl7.org/fhir/request-status",
                new EhdsiDisplayLabel(LabelCodes.Status))
        ], ContainerType.Div, "d-flex align-items-center gap-1");

        var badge = new PlainBadge(new LocalizedLabel("general.basic-information"));
        var basicInfo = new Container([
            new If(_ => infrequentProperties.Contains(CommunicationRequestInfrequentProperties.Priority),
                new NameValuePair(
                    new LocalizedLabel("communication-request.priority"),
                    new EnumLabel("f:priority", "http://hl7.org/fhir/ValueSet/request-priority")
                )
            ),
            new If(_ => infrequentProperties.Contains(CommunicationRequestInfrequentProperties.Category),
                new NameValuePair(
                    new LocalizedLabel("communication-request.category"),
                    new ChangeContext("f:category", new CodeableConcept())
                )
            ),
            new If(_ => infrequentProperties.Contains(CommunicationRequestInfrequentProperties.Occurrence),
                new NameValuePair(
                    new LocalizedLabel("communication-request.occurrence"),
                    new Chronometry("occurrence")
                )
            ),
            new If(_ => infrequentProperties.Contains(CommunicationRequestInfrequentProperties.Medium),
                new NameValuePair(
                    new LocalizedLabel("communication-request.medium"),
                    new ChangeContext("f:medium", new CodeableConcept())
                )
            ),
            new If(_ => infrequentProperties.Contains(CommunicationRequestInfrequentProperties.AuthoredOn),
                new NameValuePair(
                    new LocalizedLabel("communication-request.authoredOn"),
                    new ShowDateTime("f:authoredOn")
                )
            ),
        ], optionalClass: "name-value-pair-wrapper w-fit-content");

        var messageBadge = new PlainBadge(new LocalizedLabel("communication-request.payload"));
        var messageInfo = new Concat([
            new If(_ => infrequentProperties.Contains(CommunicationRequestInfrequentProperties.Payload),
                new Container([
                    new ConcatBuilder("f:payload", _ =>
                    [
                        new OpenTypeElement(null, "content") // string | Attachment | Reference(Any)
                    ])
                ])
            ),
            new Container([
                infrequentProperties.Optional(CommunicationRequestInfrequentProperties.Requester,
                    new AnyReferenceNamingWidget(
                        widgetModel: new ReferenceNamingWidgetModel
                        {
                            Type = ReferenceNamingWidgetType.NameValuePair,
                            LabelOverride = new LocalizedLabel("communication-request.requester"),
                        }
                    )
                ),
                infrequentProperties.Optional(CommunicationRequestInfrequentProperties.Sender,
                    new AnyReferenceNamingWidget(
                        widgetModel: new ReferenceNamingWidgetModel
                        {
                            Type = ReferenceNamingWidgetType.NameValuePair,
                            LabelOverride = new LocalizedLabel("communication-request.sender"),
                        }
                    )
                ),
                infrequentProperties.Optional(CommunicationRequestInfrequentProperties.DoNotPerform,
                    new ShowBoolean(
                        new NullWidget(),
                        new NameValuePair(
                            new LocalizedLabel("communication-request.doNotPerform"),
                            new ShowDoNotPerform()
                        )
                    )
                ),
            ], optionalClass: "name-value-pair-wrapper w-fit-content"),
        ]);

        var complete =
            new Collapser([headerInfo], [
                    new If(_ => infrequentProperties.ContainsAnyOf(CommunicationRequestInfrequentProperties.Priority,
                            CommunicationRequestInfrequentProperties.Category,
                            CommunicationRequestInfrequentProperties.Occurrence,
                            CommunicationRequestInfrequentProperties.Medium,
                            CommunicationRequestInfrequentProperties.AuthoredOn),
                        badge,
                        basicInfo,
                        new If(_ =>
                                infrequentProperties.ContainsAnyOf(CommunicationRequestInfrequentProperties.Payload,
                                    CommunicationRequestInfrequentProperties.Requester,
                                    CommunicationRequestInfrequentProperties.Sender),
                            new ThematicBreak()
                        )
                    ),
                    new If(_ =>
                            infrequentProperties.ContainsAnyOf(CommunicationRequestInfrequentProperties.Payload,
                                CommunicationRequestInfrequentProperties.Requester,
                                CommunicationRequestInfrequentProperties.Sender),
                        messageBadge,
                        messageInfo
                    ),
                    new If(
                        _ => infrequentProperties.ContainsOnly(CommunicationRequestInfrequentProperties.Encounter,
                                 CommunicationRequestInfrequentProperties.Text) ||
                             infrequentProperties.ContainsOnly(CommunicationRequestInfrequentProperties.Text),
                        new Card(null, new Narrative("f:text"))
                    ),
                ], footer: infrequentProperties.ContainsAnyOf(CommunicationRequestInfrequentProperties.Encounter,
                    CommunicationRequestInfrequentProperties.Text)
                    ?
                    [
                        new If(_ => infrequentProperties.Contains(CommunicationRequestInfrequentProperties.Encounter),
                            new ShowMultiReference("f:encounter",
                                (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                                x =>
                                [
                                    new Collapser([new LocalizedLabel("node-names.Encounter")], x.ToList(),
                                        isCollapsed: true)
                                ])
                        ),
                        new If(_ => infrequentProperties.Contains(CommunicationRequestInfrequentProperties.Text) &&
                                    !(infrequentProperties.ContainsOnly(
                                          CommunicationRequestInfrequentProperties.Encounter,
                                          CommunicationRequestInfrequentProperties.Text) ||
                                      infrequentProperties.ContainsOnly(CommunicationRequestInfrequentProperties
                                          .Text)),
                            new NarrativeCollapser()
                        ),
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()]
            );


        return await complete.Render(navigator, renderer, context);
    }
}

public enum CommunicationRequestInfrequentProperties
{
    Payload,
    [OpenType("occurrence")] Occurrence,
    Requester,
    Sender,
    Priority,
    AuthoredOn,
    Medium,
    Category,
    DoNotPerform,
    Text,
    Encounter,
}