using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Location : SequentialResourceBase<Location>, IResourceWidget
{
    public static string ResourceType => "Location";

    public static bool HasBorderedContainer(Widget widget) => true;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        var physicalTypeSystem = item.SelectSingleNode("f:physicalType/f:coding/f:system/@value").Node?.Value;
        var physicalTypeCode = item.SelectSingleNode("f:physicalType/f:coding/f:code/@value").Node?.Value;

        if (physicalTypeSystem != "http://terminology.hl7.org/CodeSystem/location-physical-type" ||
            physicalTypeCode != "ve")
        {
            var locationKeyItems = new List<Widget>();

            if (item.EvaluateCondition("f:name"))
            {
                locationKeyItems.Add(new ChangeContext(item, new Text("f:name/@value")));
            }

            if (item.EvaluateCondition("f:description"))
            {
                locationKeyItems.Add(new ChangeContext(item, new Text("f:description/@value")));
            }

            if (item.EvaluateCondition("f:address"))
            {
                locationKeyItems.Add(new ChangeContext(item, new Address("f:address")));
            }

            if (locationKeyItems.Count == 0)
            {
                return null;
            }

            var result = locationKeyItems.Intersperse(new ConstantText(", ")).ToArray();

            return new ResourceSummaryModel(null,
                new Concat(result));
        }

        var ids = new List<Widget>();

        var licensePlateId =
            item.SelectSingleNode(
                    "f:identifier[f:system/@value='https://ncez.mzcr.cz/fhir/sid/licensePlate']/f:value/@value").Node
                ?.Value;
        var vinId = item
            .SelectSingleNode("f:identifier[f:system/@value='https://ncez.mzcr.cz/fhir/sid/vin']/f:value/@value").Node
            ?.Value;
        var callSignId = item
            .SelectSingleNode("f:identifier[f:system/@value='https://ncez.mzcr.cz/fhir/sid/call-sign']/f:value/@value")
            .Node?.Value;
        var helicopterIdId =
            item.SelectSingleNode(
                    "f:identifier[f:system/@value='https://ncez.mzcr.cz/fhir/sid/helicopterId']/f:value/@value").Node
                ?.Value;

        if (!string.IsNullOrEmpty(licensePlateId))
        {
            ids.Add(new NameValuePair(new LocalizedLabel("location.vehicle-licensePlate"),
                new ConstantText(licensePlateId)));
        }

        if (!string.IsNullOrEmpty(vinId))
        {
            ids.Add(new NameValuePair(new LocalizedLabel("location.vehicle-VIN"), new ConstantText(vinId)));
        }

        if (!string.IsNullOrEmpty(callSignId))
        {
            ids.Add(new NameValuePair(new LocalizedLabel("location.vehicle-callSign"), new ConstantText(callSignId)));
        }

        if (!string.IsNullOrEmpty(helicopterIdId))
        {
            ids.Add(new NameValuePair(new LocalizedLabel("location.vehicle-helicopterId"),
                new ConstantText(helicopterIdId)));
        }

        return new ResourceSummaryModel(null, new Concat(ids));
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var properties = InfrequentProperties.Evaluate<LocationProperties>(navigator);

        var headerInfo = new Container([
            new LocalizedLabel("location"),
            properties.Optional(LocationProperties.Name,
                new ConstantText(" ("),
                new Text("@value"),
                new ConstantText(")")
            ),
        ], ContainerType.Span);

        var badge = new PlainBadge(new LocalizedLabel("general.basic-information"));

        var basicInfo = new Container([
            properties.Optional(LocationProperties.Status, new NameValuePair(
                new EhdsiDisplayLabel(LabelCodes.Status),
                new EnumLabel(".", "http://hl7.org/fhir/ValueSet/location-status")
            )),
            properties.Optional(LocationProperties.OperationalStatus, new NameValuePair(
                new LocalizedLabel("location.operationalStatus"),
                new Coding()
            )),
            properties.Optional(LocationProperties.Name, new NameValuePair(
                new EhdsiDisplayLabel(LabelCodes.Name),
                new Text("@value")
            )),
            properties.Condition(LocationProperties.Alias, new NameValuePair(
                new LocalizedLabel("location.alias"),
                new CommaSeparatedBuilder("f:alias", _ => [new Text("@value")])
            )),
            properties.Optional(LocationProperties.Description, new NameValuePair(
                new EhdsiDisplayLabel(LabelCodes.Description),
                new Text("@value")
            )),
            properties.Optional(LocationProperties.IdentifierLicensePlate,
                new NameValuePair(
                    new LocalizedLabel("location.licensePlate"),
                    new Text("f:value/@value")
                )),
            properties.Optional(LocationProperties.IdentifierVIN,
                new NameValuePair(
                    new LocalizedLabel("location.VIN"),
                    new Text("f:value/@value")
                )),
            properties.Optional(LocationProperties.IdentifierCallSign,
                new NameValuePair(
                    new LocalizedLabel("location.callSign"),
                    new Text("f:value/@value")
                )),
            properties.Optional(LocationProperties.IdentifierHelicopterId,
                new NameValuePair(
                    new LocalizedLabel("location.helicopterId"),
                    new Text("f:value/@value")
                )),
        ]);

        var locationBadge = new PlainBadge(new LocalizedLabel("location.location-information"));
        var locationInfo = new Container([
            properties.Condition(LocationProperties.Type, new NameValuePair(
                new LocalizedLabel("location.type"),
                new CommaSeparatedBuilder("f:type", _ => [new CodeableConcept()])
            )),
            properties.Condition(LocationProperties.Telecom, new NameValuePair(
                new EhdsiDisplayLabel(LabelCodes.Telecom),
                new ShowContactPoint()
            )),
            properties.Optional(LocationProperties.Address,
                new Address()
            ),
            properties.Optional(LocationProperties.PhysicalType, new NameValuePair(
                new LocalizedLabel("location.physicalType"),
                new CodeableConcept()
            )),
            properties.Optional(LocationProperties.Position,
                new NameValuePair(
                    [new LocalizedLabel("location.position")],
                    [
                        new Optional("f:longitude",
                            new NameValuePair(
                                new LocalizedLabel("location.position.longitude"),
                                new Concat([new Text("@value"), new ConstantText("°")], string.Empty)
                            )
                        ),
                        new Optional("f:latitude",
                            new NameValuePair(
                                new LocalizedLabel("location.position.latitude"),
                                new Concat([new Text("@value"), new ConstantText("°")], string.Empty)
                            )
                        ),
                        new Optional("f:altitude",
                            new NameValuePair(
                                new LocalizedLabel("location.position.altitude"),
                                new Concat([new Text("@value"), new ConstantText("m")])
                            )
                        ),
                    ]
                )
            ),
        ]);

        var operationBadge = new PlainBadge(new LocalizedLabel("location.operation-details"));
        var operationInfo = new Container([
            properties.Optional(LocationProperties.ManagingOrganization,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("location.managingOrganization"),
                    }
                )
            ),
            properties.Optional(LocationProperties.PartOf,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("location.partOf"),
                    }
                )
            ),
            properties.Condition(LocationProperties.HoursOfOperation,
                new ConcatBuilder("f:hoursOfOperation", _ =>
                    [
                        new NameValuePair(
                            new LocalizedLabel("location.hoursOfOperation.dayOfWeek"),
                            new CommaSeparatedBuilder("f:daysOfWeek",
                                _ => [new EnumLabel("@value", "http://hl7.org/fhir/ValueSet/days-of-week")])
                        ),
                        new NameValuePair(
                            [new LocalizedLabel("location.hoursOfOperation.times")],
                            [
                                new Optional("f:allDay",
                                    new ShowBoolean(new NullWidget(),
                                        new TextContainer(TextStyle.Bold,
                                            [new LocalizedLabel("location.hoursOfOperation.allDay")]))),
                                new Concat(
                                [
                                    new Optional("f:openingTime",
                                        new Text("@value")),
                                    new Condition("f:openingTime and f:closingTime",
                                        new ConstantText(" - ")
                                    ),
                                    new Optional("f:closingTime",
                                        new Text("@value")
                                    ),
                                ]),
                            ]
                        ),
                    ]
                )
            ),
            properties.Optional(LocationProperties.AvailabilityExceptions,
                new NameValuePair(
                    new LocalizedLabel("location.availabilityExceptions"),
                    new Text("@value")
                )
            ),
        ]);


        var complete =
            new Collapser([headerInfo], [
                    new If(
                        _ => properties.ContainsAnyOf(LocationProperties.Name, LocationProperties.Alias,
                            LocationProperties.Description, LocationProperties.Status,
                            LocationProperties.OperationalStatus, LocationProperties.IdentifierLicensePlate,
                            LocationProperties.IdentifierVIN, LocationProperties.IdentifierCallSign,
                            LocationProperties.IdentifierHelicopterId), badge, basicInfo),
                    ThematicBreak.SurroundedThematicBreak(properties, [
                        LocationProperties.Name, LocationProperties.Alias,
                        LocationProperties.Description, LocationProperties.Status,
                        LocationProperties.OperationalStatus, LocationProperties.IdentifierLicensePlate,
                        LocationProperties.IdentifierVIN, LocationProperties.IdentifierCallSign,
                        LocationProperties.IdentifierHelicopterId,
                    ], [
                        LocationProperties.Type, LocationProperties.Telecom,
                        LocationProperties.Address, LocationProperties.PhysicalType, LocationProperties.Position,
                    ]),
                    new If(
                        _ => properties.ContainsAnyOf(LocationProperties.Type, LocationProperties.Telecom,
                            LocationProperties.Address, LocationProperties.PhysicalType, LocationProperties.Position),
                        locationBadge, locationInfo),
                    ThematicBreak.SurroundedThematicBreak(properties, [
                        LocationProperties.Type, LocationProperties.Telecom,
                        LocationProperties.Address, LocationProperties.PhysicalType, LocationProperties.Position
                    ], [
                        LocationProperties.ManagingOrganization,
                        LocationProperties.PartOf, LocationProperties.HoursOfOperation,
                        LocationProperties.AvailabilityExceptions
                    ]),
                    new If(
                        _ => properties.ContainsAnyOf(LocationProperties.ManagingOrganization,
                            LocationProperties.PartOf, LocationProperties.HoursOfOperation,
                            LocationProperties.AvailabilityExceptions), operationBadge, operationInfo),
                ], footer: navigator.EvaluateCondition("f:text")
                    ?
                    [
                        new NarrativeCollapser()
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()]
            );


        return complete.Render(navigator, renderer, context);
    }

    private enum LocationProperties
    {
        Name,
        Status,
        OperationalStatus,
        Alias,
        Description,

        [PropertyPath("f:identifier[f:system/@value='https://ncez.mzcr.cz/fhir/sid/licensePlate']")]
        IdentifierLicensePlate,

        [PropertyPath("f:identifier[f:system/@value='https://ncez.mzcr.cz/fhir/sid/vin']")]
        IdentifierVIN,

        [PropertyPath("f:identifier[f:system/@value='https://ncez.mzcr.cz/fhir/sid/call-sign']")]
        IdentifierCallSign,

        [PropertyPath("f:identifier[f:system/@value='https://ncez.mzcr.cz/fhir/sid/helicopterId']")]
        IdentifierHelicopterId,
        Type,
        Telecom,
        Address,
        PhysicalType,
        Position,
        ManagingOrganization,
        PartOf,
        HoursOfOperation,
        AvailabilityExceptions,
    }
}