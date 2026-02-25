using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class HealthcareService : ColumnResourceBase<HealthcareService>, IResourceWidget
{
    public static string ResourceType => "HealthcareService";
    public static bool HasBorderedContainer(Widget widget) => false;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<HealthcareServiceInfrequentProperties>(navigator);

        var headerInfo = new Container([
            new LocalizedLabel("healthcare-service"),
            infrequentProperties.Optional(HealthcareServiceInfrequentProperties.Name,
                new ConstantText(" ("),
                new Text("@value"),
                new ConstantText(")")
            ),
        ], ContainerType.Span);

        var badge = new PlainBadge(new LocalizedLabel("general.basic-information"));

        var basicInfo = new Container(
        [
            infrequentProperties.Optional(HealthcareServiceInfrequentProperties.Active,
                new TextContainer(
                    TextStyle.Bold,
                    new ShowBoolean(
                        new LocalizedLabel("healthcare-service.active.false"),
                        new LocalizedLabel("healthcare-service.active.true")
                    ),
                    optionalClass: "span-over-full-name-value-pair-cell"
                )
            ),
            infrequentProperties.Optional(HealthcareServiceInfrequentProperties.Name,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.Name),
                    new Text("@value")
                )),
            infrequentProperties.Optional(HealthcareServiceInfrequentProperties.Comment,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.Description),
                    new Text("@value")
                )
            ),
            infrequentProperties.Optional(HealthcareServiceInfrequentProperties.ProvidedBy,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("healthcare-service.providedBy"),
                    }
                )
            ),
            new Condition("f:category",
                new NameValuePair(
                    new LocalizedLabel("healthcare-service.category"),
                    new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()])
                )
            ),
            new Condition("f:type",
                new NameValuePair(
                    new LocalizedLabel("healthcare-service.type"),
                    new CommaSeparatedBuilder("f:type", _ => [new CodeableConcept()])
                )
            ),
            new Condition("f:specialty",
                new NameValuePair(
                    new LocalizedLabel("healthcare-service.specialty"),
                    new CommaSeparatedBuilder("f:specialty", _ => [new CodeableConcept()])
                )
            ),
            new Condition("f:location",
                new NameValuePair(
                    new LocalizedLabel("healthcare-service.location"),
                    new CommaSeparatedBuilder("f:location", _ => [new AnyReferenceNamingWidget()])
                )
            ),
        ], optionalClass: "name-value-pair-wrapper w-fit-content");

        var detailBadge = new PlainBadge(new LocalizedLabel("general.detailed-information"));
        var detailInfo = new Container([
            infrequentProperties.Optional(HealthcareServiceInfrequentProperties.ExtraDetails,
                new NameValuePair(
                    new LocalizedLabel("healthcare-service.extraDetails"),
                    new Markdown("@value")
                )
            ),
            infrequentProperties.Optional(HealthcareServiceInfrequentProperties.Photo,
                new NameValuePair(
                    new LocalizedLabel("healthcare-service.photo"),
                    new Attachment()
                )
            ),
            new Container([
                new Condition("f:telecom",
                    new TextContainer(TextStyle.Bold, new EhdsiDisplayLabel(LabelCodes.Telecom)),
                    new Row([new ShowContactPoint()])
                ),
            ], ContainerType.Div, "mt-2 mb-2"),
            new Container([
                new Condition("f:coverageArea", new NameValuePair(
                    new LocalizedLabel("healthcare-service.coverageArea"),
                    new CommaSeparatedBuilder("f:coverageArea", _ => [new AnyReferenceNamingWidget()])
                )),
                new Condition("f:serviceProvisionCode", new NameValuePair(
                    new LocalizedLabel("healthcare-service.serviceProvisionCode"),
                    new CommaSeparatedBuilder("f:serviceProvisionCode", _ => [new CodeableConcept()])
                )),
            ], optionalClass: "name-value-pair-wrapper w-fit-content"),
            new Condition("f:eligibility",
                new TextContainer(
                    TextStyle.Bold,
                    [new LocalizedLabel("healthcare-service.eligibility"), new ConstantText(":")]
                ),
                new ItemListBuilder("f:eligibility", ItemListType.Unordered, _ =>
                [
                    new Concat([
                        new Optional("f:code",
                            new NameValuePair(
                                new LocalizedLabel("healthcare-service.eligibility.code"),
                                new CodeableConcept()
                            )
                        ),
                        new Optional("f:comment",
                            new NameValuePair(
                                new LocalizedLabel("healthcare-service.eligibility.comment"),
                                new Markdown("@value")
                            )
                        ),
                    ]),
                ])
            ),
            new Container([
                new Condition("f:program", new NameValuePair(
                    new LocalizedLabel("healthcare-service.program"),
                    new CommaSeparatedBuilder("f:program", _ => [new CodeableConcept()])
                )),
                new Condition("f:characteristic", new NameValuePair(
                    new LocalizedLabel("healthcare-service.characteristic"),
                    new CommaSeparatedBuilder("f:characteristic", _ => [new CodeableConcept()])
                )),
                new Condition("f:referralMethod", new NameValuePair(
                    new LocalizedLabel("healthcare-service.referralMethod"),
                    new CommaSeparatedBuilder("f:referralMethod", _ => [new CodeableConcept()])
                )),
            ], optionalClass: "name-value-pair-wrapper w-fit-content"),
            //ignore endpoint
        ]);

        var operationBadge = new PlainBadge(new LocalizedLabel("healthcare-service.availability-info"));
        var operationInfo = new Container([
            infrequentProperties.Optional(HealthcareServiceInfrequentProperties.AppointmentRequired,
                new TextContainer(
                    TextStyle.Bold,
                    [
                        new ShowBoolean(
                            new LocalizedLabel("healthcare-service.appointmentRequired.false"),
                            new LocalizedLabel("healthcare-service.appointmentRequired.true")
                        ),
                    ]
                ),
                new LineBreak()
            ),
            new Condition("f:availableTime",
                new TextContainer(
                    TextStyle.Bold,
                    [new LocalizedLabel("healthcare-service.availableTime"), new ConstantText(":")]
                ),
                new Row([
                    new HealthcareServiceAvailableTime(),
                ])
            ),
            new Condition("f:notAvailable",
                new TextContainer(
                    TextStyle.Bold,
                    [new LocalizedLabel("healthcare-service.availableTime.notAvailable"), new ConstantText(":")]
                ),
                new Row([
                    new ListBuilder("f:notAvailable[f:during]", FlexDirection.Row, _ =>
                        [
                            new Condition("f:during",
                                new Card(new ShowPeriod("f:during"),
                                    new Markdown("f:description/@value"), optionalClass: "time-card")
                            ),
                        ], flexContainerClasses: "column-gap-2 flex-wrap"
                    ),
                    new Condition("not(f:during)",
                        new Card(new LocalizedLabel("healthcare-service.notAvailable.during.absent"),
                            new ItemListBuilder("f:notAvailable[not(f:during)]",
                                ItemListType.Unordered, _ => [new Markdown("f:description/@value")]
                            ), optionalClass: "time-card")
                    )
                ])
            ),
            infrequentProperties.Optional(HealthcareServiceInfrequentProperties.AvailabilityExceptions,
                new NameValuePair(
                    new LocalizedLabel("healthcare-service.availabilityExceptions"),
                    new Text("@value")
                )
            ),
        ]);


        var complete =
            new Container([
                new Collapser([headerInfo], [
                        new Condition(
                            "f:active or f:name or f:comment or f:providedBy or f:category or f:type or f:specialty or f:location",
                            badge,
                            basicInfo,
                            new Condition(
                                "f:extraDetails or f:photo or f:telecom or f:coverageArea or f:serviceProvisionCode or f:eligibility or " +
                                "f:program or f:characteristic or f:referralMethod or f:appointmentRequired or f:availableTime or f:notAvailable or f:availabilityExceptions",
                                new ThematicBreak()
                            )
                        ),
                        new Condition(
                            "f:extraDetails or f:photo or f:telecom or f:coverageArea or f:serviceProvisionCode or f:eligibility or f:program or " +
                            "f:characteristic or f:referralMethod",
                            detailBadge,
                            detailInfo,
                            new Condition(
                                "f:appointmentRequired or f:availableTime or f:notAvailable or f:availabilityExceptions",
                                new ThematicBreak()
                            )
                        ),
                        new Condition(
                            "f:appointmentRequired or f:availableTime or f:notAvailable or f:availabilityExceptions",
                            operationBadge,
                            operationInfo
                        )
                    ], footer: navigator.EvaluateCondition("f:text")
                        ?
                        [
                            new NarrativeCollapser()
                        ]
                        : null,
                    iconPrefix: [new NarrativeModal()]
                ),
            ]);


        return complete.Render(navigator, renderer, context);
    }
}

public enum HealthcareServiceInfrequentProperties
{
    Name,
    Active,
    Comment,
    ProvidedBy,
    ExtraDetails,
    Photo,
    AppointmentRequired,
    AvailabilityExceptions,
}