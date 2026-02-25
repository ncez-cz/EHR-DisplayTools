using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Person;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class MessageHeader : Widget, IResourceWidget
{
    public static string ResourceType => "MessageHeader";
    
    public static bool RequiresExternalTitle => true;
    
    public static bool HasBorderedContainer(Widget widget) => false;
    
    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return items.Select(x => new ChangeContext(x, new MessageHeader())).ToList<Widget>();
    }
    
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer, RenderContext context)
    {
        var infrequentProperties = InfrequentProperties.Evaluate<MessageHeaderInfrequentProperties>(navigator);
        
        var destinationNav = navigator.SelectSingleNode("f:destination");
        var receiverInfrequentProperties = InfrequentProperties.Evaluate<DestinationInfrequentProperties>(destinationNav);
        
        var sourceNav = navigator.SelectSingleNode("f:source");
        var sourceInfrequentProperties = InfrequentProperties.Evaluate<SourceInfrequentProperties>(sourceNav);
        
        var widget =
            new Column(
            [
                new Row([
                    new Heading(
                    [
                        new TextContainer(TextStyle.Bold,
                            [new OpenTypeElement(null, "event")]),
                    ], HeadingSize.H4, customClass: "m-0 blue-color"),
                    new EnumIconTooltip("f:status",
                        "http://hl7.org/fhir/ValueSet/fm-status",
                        new EhdsiDisplayLabel(LabelCodes.Status)),
                    new NarrativeModal(alignRight: false),
                ]),
                new Row([
                    infrequentProperties.Optional(MessageHeaderInfrequentProperties.Destination,
                        new NameValuePair(
                            new LocalizedLabel("message-header.destination"),
                            new Concat([
                                receiverInfrequentProperties.Optional(DestinationInfrequentProperties.Name,
                                    new NameValuePair(
                                        new LocalizedLabel("message-header.destination.name"),
                                        new Text("@value"),
                                        direction: FlexDirection.Row,
                                        style: NameValuePair.NameValuePairStyle.Secondary
                                    )
                                ),
                                receiverInfrequentProperties.Optional(DestinationInfrequentProperties.Endpoint,
                                    new NameValuePair(
                                        new LocalizedLabel("message-header.destination.endpoint"),
                                        new Text("@value"),
                                        direction: FlexDirection.Row,
                                        style: NameValuePair.NameValuePairStyle.Secondary
                                    )
                                ),
                                receiverInfrequentProperties.Optional(DestinationInfrequentProperties.Receiver,
                                    new AnyReferenceNamingWidget(widgetModel: new ReferenceNamingWidgetModel
                                    {
                                        Type = ReferenceNamingWidgetType.NameValuePair,
                                        LabelOverride = new LocalizedLabel("message-header.destination.receiver"),
                                        Style = NameValuePair.NameValuePairStyle.Secondary,
                                    })
                                ),
                                receiverInfrequentProperties.Optional(DestinationInfrequentProperties.Target,
                                    new AnyReferenceNamingWidget(widgetModel: new ReferenceNamingWidgetModel
                                    {
                                        Type = ReferenceNamingWidgetType.NameValuePair,
                                        LabelOverride = new LocalizedLabel("message-header.destination.target"),
                                        Style = NameValuePair.NameValuePairStyle.Secondary,
                                    })
                                ),
                            ]),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Primary
                        )
                    ),
                    infrequentProperties.Optional(MessageHeaderInfrequentProperties.Source,
                        new NameValuePair(
                            new LocalizedLabel("message-header.source"),
                            new Concat([
                                sourceInfrequentProperties.Optional(SourceInfrequentProperties.Name,
                                    new NameValuePair(
                                        new LocalizedLabel("message-header.source.name"),
                                        new Text("@value"),
                                        direction: FlexDirection.Row,
                                        style: NameValuePair.NameValuePairStyle.Secondary
                                    )
                                ),
                                new HideableDetails(
                                    sourceInfrequentProperties.Optional(SourceInfrequentProperties.Software,
                                        new NameValuePair(
                                            new LocalizedLabel("message-header.source.software"),
                                            new Text("@value"),
                                            direction: FlexDirection.Row,
                                            style: NameValuePair.NameValuePairStyle.Secondary
                                        )
                                    )
                                ),
                                new HideableDetails(
                                    sourceInfrequentProperties.Optional(SourceInfrequentProperties.Version,
                                        new NameValuePair(
                                            new LocalizedLabel("message-header.source.version"),
                                            new Text("@value"),
                                            direction: FlexDirection.Row,
                                            style: NameValuePair.NameValuePairStyle.Secondary
                                        )
                                    )
                                ),
                                sourceInfrequentProperties.Optional(SourceInfrequentProperties.Contact,
                                    new NameValuePair(
                                        new LocalizedLabel("message-header.source.contact"),
                                        new ContactInformation(),
                                        direction: FlexDirection.Row,
                                        style: NameValuePair.NameValuePairStyle.Secondary
                                    )
                                ),
                                sourceInfrequentProperties.Optional(SourceInfrequentProperties.Endpoint,
                                    new NameValuePair(
                                        new LocalizedLabel("message-header.source.endpoint"),
                                        new Text("@value"),
                                        direction: FlexDirection.Row,
                                        style: NameValuePair.NameValuePairStyle.Secondary
                                    )
                                ),
                            ]),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Primary
                        )
                    ),
                    infrequentProperties.Optional(MessageHeaderInfrequentProperties.Reason,
                        new NameValuePair(
                            new LocalizedLabel("message-header.reason"),
                            new CodeableConcept(),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Primary
                        )
                    ),
                    infrequentProperties.Optional(MessageHeaderInfrequentProperties.Response,
                        new HideableDetails(
                            new NameValuePair(
                                new LocalizedLabel("message-header.response"),
                                new Concat([
                                    new NameValuePair(
                                        new LocalizedLabel("message-header.response.identifier"),
                                        new Text("f:identifier/@value"),
                                        direction: FlexDirection.Row,
                                        style: NameValuePair.NameValuePairStyle.Secondary
                                    ),
                                    new NameValuePair(
                                        new LocalizedLabel("message-header.response.code"),
                                        new Coding(),
                                        direction: FlexDirection.Row,
                                        style: NameValuePair.NameValuePairStyle.Secondary
                                    ),
                                ]),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                ], flexContainerClasses: "column-gap-6 row-gap-1"),
                new Column([
                    receiverInfrequentProperties.Condition(DestinationInfrequentProperties.Receiver,
                        new ConcatBuilder("f:destination/f:receiver", _ =>
                        [
                            ShowSingleReference.WithDefaultDisplayHandler(nav =>
                            [
                                new Container([
                                    new PersonOrOrganization(
                                        nav,
                                        skipWhenInactive: true,
                                        collapserTitle: new LocalizedLabel("message-header.destination.receiver")
                                    ),
                                ], idSource: nav),
                            ]),
                        ])
                    ),
                    receiverInfrequentProperties.Condition(DestinationInfrequentProperties.Target,
                        new ConcatBuilder("f:destination/f:target", _ =>
                        [
                            ShowSingleReference.WithDefaultDisplayHandler(nav =>
                            [
                                new Container([
                                    new PersonOrOrganization(
                                        nav,
                                        skipWhenInactive: true,
                                        collapserTitle: new LocalizedLabel("message-header.destination.target")
                                    ),
                                ], idSource: nav),
                            ]),
                        ])
                    ),
                    infrequentProperties.Condition(MessageHeaderInfrequentProperties.Sender,
                        new ConcatBuilder("f:sender", _ =>
                        [
                            ShowSingleReference.WithDefaultDisplayHandler(nav =>
                            [
                                new Container([
                                    new PersonOrOrganization(
                                        nav,
                                        skipWhenInactive: true,
                                        collapserTitle: new LocalizedLabel("message-header.sender")
                                    ),
                                ], idSource: nav),
                            ]),
                        ])
                    ),
                    infrequentProperties.Condition(MessageHeaderInfrequentProperties.Enterer,
                        new HideableDetails(
                            new ConcatBuilder("f:enterer", _ =>
                            [
                                ShowSingleReference.WithDefaultDisplayHandler(nav =>
                                [
                                    new Container([
                                        new PersonOrOrganization(
                                            nav,
                                            skipWhenInactive: true,
                                            collapserTitle: new LocalizedLabel("message-header.enterer")
                                        ),
                                    ], idSource: nav),
                                ]),
                            ])
                        )
                    ),
                    infrequentProperties.Condition(MessageHeaderInfrequentProperties.Author,
                        new HideableDetails(
                            new ConcatBuilder("f:author", _ =>
                            [
                                ShowSingleReference.WithDefaultDisplayHandler(nav =>
                                [
                                    new Container([
                                        new PersonOrOrganization(
                                            nav,
                                            skipWhenInactive: true,
                                            collapserTitle: new LocalizedLabel("message-header.author")
                                        ),
                                    ], idSource: nav),
                                ]),
                            ])
                        )
                    ),
                    infrequentProperties.Condition(MessageHeaderInfrequentProperties.Responsible,
                        new HideableDetails(
                            new ConcatBuilder("f:responsible", _ =>
                            [
                                ShowSingleReference.WithDefaultDisplayHandler(nav =>
                                [
                                    new Container([
                                        new PersonOrOrganization(
                                            nav,
                                            skipWhenInactive: true,
                                            collapserTitle: new LocalizedLabel("message-header.responsible")
                                        ),
                                    ], idSource: nav),
                                ]),
                            ])
                        )
                    ),
                ]),
                new Concat( [
                    new Heading(
                    [
                        new TextContainer(TextStyle.Bold,
                            new LocalizedLabel("message-header.focus")
                            ),
                    ], HeadingSize.H5, customClass: "m-0 blue-color mt-3"),
                    new ShowMultiReference("f:focus", displayResourceType: true),
                ]),
            ], flexContainerClasses: "gap-1");
         
        return widget.Render(navigator, renderer, context);
    }
    
    private enum MessageHeaderInfrequentProperties
    {
        Destination,
        Source,
        Responsible,
        Author,
        Sender,
        Enterer,
        Reason,
        Response,
    }

    private enum DestinationInfrequentProperties
    {
        Receiver,
        Name,
        Target,
        Endpoint,
    }
    
    private enum SourceInfrequentProperties
    {
        Name,
        Software,
        Version,
        Contact,
        Endpoint,
    }
}