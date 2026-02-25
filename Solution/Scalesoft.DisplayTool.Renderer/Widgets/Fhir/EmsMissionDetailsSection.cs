using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class EmsMissionDetailsSection : Widget
{
    private const string MissionEncounterEntryPath =
        "f:section[f:code/f:coding/f:code/@value = '67664-3' and f:code/f:coding/f:system/@value = 'http://loinc.org']/f:entry";

    private const string TimelinePathEntryPath =
        "f:section[f:code/f:coding/f:code/@value = '67667-6' and f:code/f:coding/f:system/@value = 'http://loinc.org']/f:entry";

    private const string DispatchPathEntryPath =
        "f:section[f:code/f:coding/f:code/@value = '67660-1' and f:code/f:coding/f:system/@value = 'http://loinc.org']/f:entry";

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (context.DocumentType != DocumentType.EmsReport)
        {
            return RenderResult.NullResult;
        }

        var encounter = ReferenceHandler.GetSingleNodeNavigatorFromReference(navigator, "f:encounter", ".");
        var section = new Section(".", null, [new LocalizedLabel("ems.header-mission-details")], [
            new Column([
                new Row([
                    new ChangeContext(encounter, [
                            new Optional("f:priority",
                                new NameValuePair(new LocalizedLabel("ems.header-priority"), new CodeableConcept(),
                                    style: NameValuePair.NameValuePairStyle.Primary,
                                    direction: FlexDirection.Column)
                            ),
                            new ConcatBuilder(
                            "f:identifier", // are all identifiers dispatch identifiers? standard does not specify an identifier system
                            _ =>
                            [
                                new NameValuePair(
                                    new LocalizedLabel("ems.header-dispatch-number"), new ShowIdentifier(),
                                    style: NameValuePair.NameValuePairStyle.Primary,
                                    direction: FlexDirection.Column
                                )
                            ]), new Optional("f:period/f:start",
                            new NameValuePair(
                                new LocalizedLabel("ems.header-dispatch-date"),
                                new ShowDateTime(preferredDisplay: DateFormatDisplay.DateOnly),
                                style: NameValuePair.NameValuePairStyle.Primary,
                                direction: FlexDirection.Column
                            )
                        )]
                    ),
                    new Condition(
                        MissionEncounterEntryPath,
                        () =>
                        {
                            var result = new List<Widget>();
                            var entries = ReferenceHandler.GetNodeNavigatorsFromReferences(navigator,
                                MissionEncounterEntryPath);
                            var missionAmbulances = entries?.Where(x =>
                                    x.Node?.Name == "Location" && x.EvaluateCondition(
                                        "f:physicalType/f:coding[f:code/@value='ve' and f:system/@value='http://terminology.hl7.org/CodeSystem/location-physical-type']"))
                                .ToArray();
                            // ideally, entry discrimination should be done based on their code, but it is not defined by the current standard
                            if (missionAmbulances != null && missionAmbulances.Length != 0)
                            {
                                foreach (var missionAmbulance in missionAmbulances)
                                {
                                    var licensePlateIdentifier = missionAmbulance.SelectSingleNode(
                                        "f:identifier[f:system/@value='https://ncez.mzcr.cz/fhir/sid/licensePlate']");
                                    if (licensePlateIdentifier.Node != null)
                                    {
                                        result.Add(new NameValuePair(
                                            new LocalizedLabel("ems.header-vehicle-license-plate"),
                                            new ChangeContext(licensePlateIdentifier, new Text("f:value/@value")),
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column));
                                    }
                                }
                            }

                            return result.ToArray();
                        }),
                    new Condition(
                        "f:extension[@url = 'http://hl7.eu/fhir/StructureDefinition/information-recipient']",
                        new ConcatBuilder(
                            "f:extension[@url = 'http://hl7.eu/fhir/StructureDefinition/information-recipient']",
                            _ =>
                            [
                                new AnyReferenceNamingWidget("f:valueReference",
                                    widgetModel: new ReferenceNamingWidgetModel()
                                    {
                                        Direction = FlexDirection.Column,
                                        Style = NameValuePair.NameValuePairStyle.Primary,
                                        LabelOverride = new LocalizedLabel("ems.header-intended-recipient"),
                                        Type = ReferenceNamingWidgetType.NameValuePair
                                    })
                            ])
                    ),
                    new ChangeContext(encounter,
                        new Optional(
                            "f:location[f:physicalType/f:coding[f:code/@value='si' and f:system/@value='http://terminology.hl7.org/CodeSystem/location-physical-type']]",
                            new AnyReferenceNamingWidget("f:location", widgetModel: new ReferenceNamingWidgetModel
                            {
                                Direction = FlexDirection.Column,
                                Style = NameValuePair.NameValuePairStyle.Primary,
                                LabelOverride = new LocalizedLabel("ems.header-intended-location"),
                                Type = ReferenceNamingWidgetType.NameValuePair
                            })
                        )
                    ),
                ], flexContainerClasses: "justify-content-between column-gap-4"),
                new Row([
                    new Card(null,
                        new Row([
                            new ChangeContext(encounter, new Optional("f:period/f:start",
                                    new NameValuePair(
                                        new LocalizedLabel("ems.header-request"),
                                        new ShowDateTime(preferredDisplay: DateFormatDisplay.TimeOnly),
                                        style: NameValuePair.NameValuePairStyle.Primary,
                                        direction: FlexDirection.Column
                                    )
                                )
                            ),
                            new Condition(
                                MissionEncounterEntryPath,
                                () =>
                                {
                                    var result = new List<Widget>();
                                    var entries = ReferenceHandler.GetNodeNavigatorsFromReferences(navigator,
                                        MissionEncounterEntryPath);
                                    // ideally, entry discrimination should be done based on their code, but it is not defined by the current standard
                                    var missionEncounter = entries?.SingleOrDefault(x => x.Node?.Name == "Encounter");
                                    var missionTimeStatus = entries?.SingleOrDefault(x =>
                                        x.Node?.Name == "Observation" && x.EvaluateCondition(
                                            "f:code/f:coding/f:code/@value = '69476-0' and f:code/f:coding/f:system/@value = 'http://loinc.org'"));
                                    if (missionEncounter != null)
                                    {
                                        var periodStart = missionEncounter.SelectSingleNode("f:period/f:start");
                                        if (periodStart.Node != null)
                                        {
                                            result.Add(
                                                new NameValuePair(
                                                    new LocalizedLabel("ems.header-dispatch"),
                                                    new ChangeContext(periodStart,
                                                        new ShowDateTime(preferredDisplay: DateFormatDisplay.TimeOnly)),
                                                    style: NameValuePair.NameValuePairStyle.Primary,
                                                    direction: FlexDirection.Column
                                                )
                                            );
                                        }

                                        var locationPeriodStart =
                                            missionEncounter.SelectAllNodes("f:location/f:period/f:start");
                                        foreach (var nav in locationPeriodStart)
                                        {
                                            result.Add(
                                                new NameValuePair(
                                                    new LocalizedLabel("ems.header-incident-arrived"),
                                                    new ChangeContext(nav,
                                                        new ShowDateTime(preferredDisplay: DateFormatDisplay.TimeOnly)),
                                                    style: NameValuePair.NameValuePairStyle.Primary,
                                                    direction: FlexDirection.Column
                                                )
                                            );
                                        }

                                        var locationPeriodEnd =
                                            missionEncounter.SelectAllNodes("f:location/f:period/f:end");
                                        foreach (var nav in locationPeriodEnd)
                                        {
                                            result.Add(
                                                new NameValuePair(
                                                    new LocalizedLabel("ems.header-transport"),
                                                    new ChangeContext(nav,
                                                        new ShowDateTime(preferredDisplay: DateFormatDisplay.TimeOnly)),
                                                    style: NameValuePair.NameValuePairStyle.Primary,
                                                    direction: FlexDirection.Column
                                                ));
                                        }
                                    }

                                    if (missionTimeStatus != null)
                                    {
                                        var valueDateTime =
                                            missionTimeStatus.SelectSingleNode("f:valueDateTime");
                                        if (valueDateTime.Node != null)
                                        {
                                            result.Add(
                                                new NameValuePair(
                                                    new LocalizedLabel("ems.header-medical-facility-arrival"),
                                                    new ChangeContext(valueDateTime,
                                                        new ShowDateTime(preferredDisplay: DateFormatDisplay.TimeOnly)),
                                                    style: NameValuePair.NameValuePairStyle.Primary,
                                                    direction: FlexDirection.Column
                                                )
                                            );
                                        }
                                    }

                                    if (missionEncounter != null)
                                    {
                                        var periodEnd = missionEncounter.SelectSingleNode("f:period/f:end");
                                        if (periodEnd.Node != null)
                                        {
                                            result.Add(
                                                new NameValuePair(new LocalizedLabel("ems.header-handover"),
                                                    new ChangeContext(periodEnd,
                                                        new ShowDateTime(preferredDisplay: DateFormatDisplay.TimeOnly)),
                                                    style: NameValuePair.NameValuePairStyle.Primary,
                                                    direction: FlexDirection.Column
                                                )
                                            );
                                        }
                                    }

                                    return result.ToArray();
                                }),
                            new ChangeContext(encounter, new Optional("f:period/f:end",
                                    new NameValuePair(
                                        new LocalizedLabel("ems.header-completion"),
                                        new ShowDateTime(preferredDisplay: DateFormatDisplay.TimeOnly),
                                        style: NameValuePair.NameValuePairStyle.Primary,
                                        direction: FlexDirection.Column
                                    )
                                )
                            ),
                        ], flexContainerClasses: "column-gap-3"),
                        bodyOptionalClass: "name-value-pair-wrapper align-items-center",
                        optionalClass: "bg-transparent"),
                    new Container([
                        new Condition(DispatchPathEntryPath, () =>
                        {
                            return
                            [
                                new AnyReferenceNamingWidget(DispatchPathEntryPath,
                                    widgetModel: new ReferenceNamingWidgetModel
                                    {
                                        Type = ReferenceNamingWidgetType.NameValuePair,
                                        Direction = FlexDirection.Row,
                                        Size = NameValuePair.NameValuePairSize.Regular,
                                        Style = NameValuePair.NameValuePairStyle.Primary,
                                    }, resolvedNavFilter: nav => nav.Node?.Name is "PractitionerRole" or "Practitioner",
                                    customFallbackName: new LocalizedLabel("ems.header-crew-member")),
                            ];
                        }),
                        new Condition(
                            TimelinePathEntryPath,
                            () =>
                            {
                                var result = new List<Widget>();
                                var entries = ReferenceHandler.GetNodeNavigatorsFromReferences(navigator,
                                    TimelinePathEntryPath);
                                var communications = entries?.Where(x => x.Node?.Name == "Communication").ToArray();
                                // ideally entry discrimination should be done based on their code, but it is not defined by the current standard
                                if (communications != null)
                                {
                                    foreach (var nav in communications)
                                    {
                                        result.Add(new NameValuePair(new LocalizedLabel("ems.header-dispatcher"),
                                            new ChangeContext(nav, new AnyReferenceNamingWidget("f:recipient")),
                                            style: NameValuePair.NameValuePairStyle.Primary));
                                    }
                                }

                                return result.ToArray();
                            }),
                    ], optionalClass: "name-value-pair-wrapper h-100")
                ])
            ], flexContainerClasses: "row-gap-2")
        ]);

        return await section.Render(navigator, renderer, context);
    }
}