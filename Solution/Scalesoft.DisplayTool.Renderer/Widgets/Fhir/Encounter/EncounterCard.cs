using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;

public class EncounterCard(
    XmlDocumentNavigator? navigatorParam,
    bool displayedAsCollapser = true,
    bool showNarrative = true
)
    : ColumnResourceBase<EncounterCard>, IResourceWidget
{
    public bool DisplayedAsCollapser => displayedAsCollapser;

    public static string ResourceType => "Encounter";

    public static bool HasBorderedContainer(Widget resourceWidget)
    {
        if (resourceWidget is EncounterCard encounterCard)
        {
            return encounterCard.DisplayedAsCollapser;
        }

        throw new InvalidOperationException(
            $"Expected {nameof(EncounterCard)} widget,  got {resourceWidget.GetType().Name}");
    }

    public EncounterCard() : this(null)
    {
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<EncounterInfrequentProperties>(navigator);

        var headerInfo = new Container([
            new LocalizedLabel("node-names.Encounter"),
            new HideableDetails(
                new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/encounter-status",
                    new EhdsiDisplayLabel(LabelCodes.Status)
                )
            ),
        ], ContainerType.Div, "d-flex align-items-center gap-1");

        var badge = new PlainBadge(new LocalizedLabel("general.basic-information"));
        var basicInfo = new Container([
            new NameValuePair(
                new LocalizedLabel("encounter.class"),
                new ChangeContext("f:class", new Coding())
            ),
            new Condition("f:statusHistory",
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("encounter.statusHistory"),
                    new ItemListBuilder("f:statusHistory", ItemListType.Unordered, _ =>
                    [
                        new Optional("f:status",
                            new EnumLabel(".", "http://hl7.org/fhir/ValueSet/encounter-status")),
                        new ConstantText(" - "),
                        new Optional("f:period", new ShowPeriod())
                    ])
                ))
            ),
            infrequentProperties.Optional(EncounterInfrequentProperties.Subject,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("risk-assessment.subject"),
                    }
                )),
            new Condition("f:appointment",
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("encounter.appointment"),
                    new ItemListBuilder("f:appointment", ItemListType.Unordered, _ =>
                        [new AnyReferenceNamingWidget()])
                ))
            ),
            new Condition("f:reasonCode",
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("encounter.reasonCode"),
                    new CommaSeparatedBuilder("f:reasonCode", _ => [new CodeableConcept()])
                ))
            ),
            infrequentProperties.Optional(EncounterInfrequentProperties.Period,
                new NameValuePair(
                    new LocalizedLabel("encounter.period"),
                    new ShowPeriod()
                )),
            infrequentProperties.Optional(EncounterInfrequentProperties.Length,
                new HideableDetails(
                    new NameValuePair(
                        new EhdsiDisplayLabel(LabelCodes.Duration),
                        new ShowDuration()
                    )
                )
            ),
        ], optionalClass: "name-value-pair-wrapper w-fit-content");

        var actorsBadge = new PlainBadge(new LocalizedLabel("general.involvedParties"));
        var actorsInfo = new Container([
            infrequentProperties.Optional(EncounterInfrequentProperties.ServiceProvider,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("encounter.serviceProvider"),
                    }
                )
            ),
            new Condition("f:account",
                new HideableDetails(
                    new NameValuePair(
                        new LocalizedLabel("encounter.account"),
                        new ItemListBuilder("f:account", ItemListType.Unordered, _ => [new AnyReferenceNamingWidget()])
                    )
                )
            ),
            new Condition("f:participant",
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("encounter.participant-plural"),
                    new ItemListBuilder("f:participant", ItemListType.Unordered, _ =>
                    [
                        new AnyReferenceNamingWidget("f:individual"),
                        new Optional("f:type",
                            new ConstantText(" - "),
                            new TextContainer(TextStyle.Italic, [
                                new CommaSeparatedBuilder(".", _ => [new CodeableConcept()]),
                            ])
                        )
                    ])
                ))
            )
        ], optionalClass: "name-value-pair-wrapper w-fit-content");

        var additionalInfoBadge = new PlainBadge(new LocalizedLabel("general.additional-info"));
        var additionalInfo =
            new Container([
                new Condition("f:identifier",
                    new NameValuePair(
                        new LocalizedLabel("encounter.identifier"),
                        new CommaSeparatedBuilder("f:identifier", _ => [new ShowIdentifier()])
                    )
                ),
                infrequentProperties.Optional(EncounterInfrequentProperties.Priority,
                    new NameValuePair(
                        new LocalizedLabel("encounter.priority"),
                        new CodeableConcept()
                    )
                ),
                new Condition("f:type",
                    new NameValuePair(
                        new LocalizedLabel("encounter.type"),
                        new CommaSeparatedBuilder("f:type", _ => [new CodeableConcept()])
                    )
                ),
                infrequentProperties.Optional(EncounterInfrequentProperties.ServiceType,
                    new NameValuePair(
                        new LocalizedLabel("encounter.serviceType"),
                        new CodeableConcept()
                    )
                ),
                // Související zdroje
                infrequentProperties.Optional(EncounterInfrequentProperties.PartOf,
                    new AnyReferenceNamingWidget(
                        widgetModel: new ReferenceNamingWidgetModel
                        {
                            Type = ReferenceNamingWidgetType.NameValuePair,
                            LabelOverride = new LocalizedLabel("encounter.partOf"),
                        }
                    )
                ),
                new Condition("f:basedOn",
                    new NameValuePair(
                        new LocalizedLabel("encounter.basedOn"),
                        new ItemListBuilder("f:basedOn", ItemListType.Unordered, _ =>
                            [new AnyReferenceNamingWidget()])
                    )
                ),
                new Condition("f:episodeOfCare",
                    new NameValuePair(
                        new LocalizedLabel("encounter.episodeOfCare"),
                        new ItemListBuilder("f:episodeOfCare", ItemListType.Unordered, _ =>
                            [new AnyReferenceNamingWidget()])
                    )
                ),
                new Condition("f:reasonReference",
                    new HideableDetails(new NameValuePair(
                        new LocalizedLabel("encounter.ReasonReference"),
                        new ItemListBuilder("f:reasonReference", ItemListType.Unordered, _ =>
                            [new AnyReferenceNamingWidget()])
                    ))
                ),
            ], optionalClass: "name-value-pair-wrapper w-fit-content");

        var locationBadge = new PlainBadge(new LocalizedLabel("encounter.location"));
        var locationInfo = new Container(
            [
                new ItemListBuilder(
                    "f:location",
                    ItemListType.Unordered,
                    _ =>
                    [
                        new NameValuePair(
                            new LocalizedLabel("encounter.location.location"),
                            new AnyReferenceNamingWidget("f:location")
                        ),
                        infrequentProperties.Optional(EncounterInfrequentProperties.Status,
                            new NameValuePair(
                                new EhdsiDisplayLabel(LabelCodes.Status),
                                new EnumLabel(".", "http://hl7.org/fhir/ValueSet/location-status")
                            )
                        ),
                        infrequentProperties.Optional(EncounterInfrequentProperties.PhysicalType,
                            new NameValuePair(
                                new LocalizedLabel("encounter.location.physicalType"),
                                new CodeableConcept()
                            )
                        ),
                        infrequentProperties.Optional(EncounterInfrequentProperties.Period,
                            new NameValuePair(
                                new EhdsiDisplayLabel(LabelCodes.Duration),
                                new ShowPeriod()
                            )
                        )
                    ]
                )
            ]
        );

        List<Widget> complete =
        [
            badge,
            basicInfo, // assume class property is required and leave out condition for content before thematic break
            new Condition("f:serviceProvider or f:account or f:participant",
                new ThematicBreak(),
                actorsBadge,
                actorsInfo
            ),
            new Condition(
                "f:identifier or f:priority or f:type or f:serviceType or f:partOf or f:basedOn or f:episodeOfCare or f:reasonReference",
                new HideableDetails(
                    new ThematicBreak(),
                    additionalInfoBadge,
                    additionalInfo
                )
            ),
            new Condition(
                "f:location",
                new HideableDetails(
                    new ThematicBreak(),
                    locationBadge,
                    locationInfo
                )
            )
        ];

        if (!displayedAsCollapser && showNarrative)
        {
            complete.Add(
                new NarrativeCollapser()
            );
        }

        var selectedNavigator = navigatorParam ?? navigator;

        Widget result =
            displayedAsCollapser
                ? new Collapser([headerInfo], complete, isCollapsed: true,
                    footer: selectedNavigator.EvaluateCondition("f:text") && showNarrative
                        ?
                        [
                            new NarrativeCollapser()
                        ]
                        : null, iconPrefix: [new If(_ => showNarrative, new NarrativeModal())])
                : new Concat(complete);

        return await result.Render(selectedNavigator, renderer, context);
    }

    public enum EncounterInfrequentProperties
    {
        Subject,
        Period,
        Length,
        ServiceProvider,
        Priority,
        ServiceType,
        PartOf,
        Status,
        PhysicalType,
    }
}