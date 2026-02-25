using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ImagingStudy : SequentialResourceBase<ImagingStudy>, IResourceWidget
{
    public static string ResourceType => "ImagingStudy";
    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<ImagingStudyInfrequentProperties>(navigator);

        Widget[] widgetTree =
        [
            new Card(
                new Concat([
                    new Row([
                        new LocalizedLabel("imaging-study"),
                        new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/imagingstudy-status",
                            new EhdsiDisplayLabel(LabelCodes.Status))
                    ], flexContainerClasses: "gap-1 align-items-center"),
                    new NarrativeModal(),
                ]),
                new Container([
                    new Container([
                        new Condition("f:modality", new NameValuePair([new LocalizedLabel("imaging-study.modality")], [
                            new ItemListBuilder("f:modality", ItemListType.Unordered, _ => [new Coding()])
                        ])),
                        infrequentProperties.Optional(ImagingStudyInfrequentProperties.Started,
                            new NameValuePair(
                                new LocalizedLabel("imaging-study.started"),
                                new ShowDateTime()
                            )
                        ),
                        // ignore subject
                        // ignore identifier
                        infrequentProperties.Optional(ImagingStudyInfrequentProperties.Referrer,
                            new HideableDetails(
                                new AnyReferenceNamingWidget(
                                    widgetModel: new ReferenceNamingWidgetModel
                                    {
                                        Type = ReferenceNamingWidgetType.NameValuePair,
                                        LabelOverride = new LocalizedLabel("imaging-study.referrer"),
                                    }
                                )
                            )
                        ),
                        new Condition("f:interpreter", new HideableDetails(
                            new NameValuePair(
                                new LocalizedLabel("imaging-study.interpreter"),
                                new ItemListBuilder("f:interpreter",
                                    ItemListType.Unordered, _ => [new AnyReferenceNamingWidget()])
                            )
                        )),
                        // ignore endpoint - little value to the end user - reader
                        infrequentProperties.Optional(ImagingStudyInfrequentProperties.NumberOfSeries,
                            new NameValuePair(
                                new LocalizedLabel("imaging-study.numberOfSeries"),
                                new Text("@value")
                            )
                        ),
                        infrequentProperties.Optional(ImagingStudyInfrequentProperties.NumberOfInstances,
                            new NameValuePair(
                                new LocalizedLabel("imaging-study.numberOfInstances"),
                                new Text("@value")
                            )
                        ),
                        new Condition("f:location", new HideableDetails(new Collapser(
                            [new LocalizedLabel("imaging-study.location"),],
                            [
                                ShowSingleReference.WithDefaultDisplayHandler(
                                    nav => [new AnyResource(nav, displayResourceType: false)],
                                    "f:location"),
                            ], true))),
                        new Condition("f:note",
                            new HideableDetails(new NameValuePair([new LocalizedLabel("imaging-study.note")],
                                [
                                    new ConcatBuilder("f:note", _ => [new ShowAnnotationCompact()],
                                        new LineBreak()),
                                ],
                                style: NameValuePair.NameValuePairStyle.Secondary))
                        ),
                        new Condition("f:reasonCode",
                            new NameValuePair([new LocalizedLabel("imaging-study.reason")],
                                [new CommaSeparatedBuilder("f:reasonCode", _ => new CodeableConcept())])),
                        new Condition("f:reasonReference", new HideableDetails(new Collapser(
                            [new LocalizedLabel("imaging-study.reason"),],
                            [
                                new ListBuilder("f:reasonReference", FlexDirection.Column,
                                    _ =>
                                    [
                                        ShowSingleReference.WithDefaultDisplayHandler(nav =>
                                            [new AnyResource(nav, displayResourceType: false)])
                                    ],
                                    separator: new LineBreak()),
                            ], true))),
                        new Condition("f:procedureCode",
                            new NameValuePair([new LocalizedLabel("imaging-study.procedure")],
                                [new CommaSeparatedBuilder("f:procedureCode", _ => new CodeableConcept())])),
                    ], optionalClass: "name-value-pair-wrapper w-fit-content"),
                    new Condition("f:procedureReference", new HideableDetails(new Collapser(
                        [new LocalizedLabel("imaging-study.procedure"),],
                        [
                            ShowSingleReference.WithDefaultDisplayHandler(
                                nav => [new AnyResource(nav, displayResourceType: false)],
                                "f:procedureReference")
                        ], true))),
                    new Condition("f:basedOn", new Collapser([new LocalizedLabel("imaging-study.basedOn"),],
                        [new ShowMultiReference("f:basedOn", displayResourceType: false)], true)),
                    new Condition("f:series",
                        new ThematicBreak(),
                        new TextContainer(TextStyle.Bold, new LocalizedLabel("imaging-study.series")),
                        new ListBuilder(
                            "f:series", FlexDirection.Column, (_, nav) =>
                            {
                                var seriesInfrequentProperties =
                                    InfrequentProperties.Evaluate<SeriesInfrequentProperties>(nav);
                                return
                                [
                                    new Collapser(
                                    [
                                        new NameValuePair(new LocalizedLabel("imaging-study.series.uid"),
                                            new Text("f:uid/@value"))
                                    ], [
                                        new Container([
                                            seriesInfrequentProperties.Optional(SeriesInfrequentProperties.Number,
                                                new NameValuePair(
                                                    new LocalizedLabel("imaging-study.series.number"),
                                                    new Text("@value")
                                                )
                                            ),
                                            new HideableDetails(
                                                new NameValuePair(
                                                    new LocalizedLabel("imaging-study.series.modality"),
                                                    new ChangeContext("f:modality", new Coding())
                                                )
                                            ),
                                            seriesInfrequentProperties.Optional(SeriesInfrequentProperties.Description,
                                                new NameValuePair(
                                                    new LocalizedLabel("imaging-study.series.description"),
                                                    new Text("@value")
                                                )
                                            ),
                                            seriesInfrequentProperties.Optional(
                                                SeriesInfrequentProperties.NumberOfInstances,
                                                new HideableDetails(
                                                    new NameValuePair(
                                                        new LocalizedLabel("imaging-study.series.numberOfInstances"),
                                                        new Text("@value")
                                                    )
                                                )
                                            ),
                                            // ignore endpoint
                                            seriesInfrequentProperties.Optional(SeriesInfrequentProperties.BodySite,
                                                new NameValuePair(
                                                    new EhdsiDisplayLabel(LabelCodes.BodySite),
                                                    new Coding()
                                                )
                                            ),
                                            seriesInfrequentProperties.Optional(SeriesInfrequentProperties.Laterality,
                                                new HideableDetails(
                                                    new NameValuePair(
                                                        new LocalizedLabel("imaging-study.series.laterality"),
                                                        new Coding()
                                                    )
                                                )
                                            ),
                                            seriesInfrequentProperties.Condition(SeriesInfrequentProperties.Specimen,
                                                new HideableDetails(
                                                    new ListBuilder("f:specimen", FlexDirection.Column, _ =>
                                                    [
                                                        new Card(new LocalizedLabel("imaging-study.series.speciment"),
                                                            new Container([
                                                                ShowSingleReference
                                                                    .WithDefaultDisplayHandler(referenceNav =>
                                                                    [
                                                                        new AnyResource(referenceNav,
                                                                            displayResourceType: false)
                                                                    ]),
                                                            ])
                                                        ),
                                                    ])
                                                )
                                            ),
                                            seriesInfrequentProperties.Optional(SeriesInfrequentProperties.Started,
                                                new HideableDetails(
                                                    new NameValuePair(
                                                        new LocalizedLabel("imaging-study.series.started"),
                                                        new ShowDateTime()
                                                    )
                                                )
                                            ),
                                        ], optionalClass: "name-value-pair-wrapper w-fit-content"),
                                        new Condition("f:performer", new Collapser(
                                            [new LocalizedLabel("imaging-study.series.performer")],
                                            [
                                                new ListBuilder("f:performer", FlexDirection.Column,
                                                    (_, _, itemNav, next) =>
                                                    [
                                                        new Optional("f:function",
                                                            new NameValuePair(
                                                                [
                                                                    new LocalizedLabel(
                                                                        "imaging-study.series.performer.function")
                                                                ],
                                                                [new CodeableConcept()])),
                                                        new AnyReferenceNamingWidget("f:actor"),
                                                        new If(
                                                            _ =>
                                                            {
                                                                const string conditionXpath = "f:function or f:actor";
                                                                return next?.EvaluateCondition(conditionXpath) ==
                                                                    true && itemNav.EvaluateCondition(conditionXpath);
                                                            },
                                                            new ThematicBreak())
                                                    ], flexContainerClasses: "gap-0"),
                                            ])),
                                        new Condition("f:instance", new HideableDetails(new Collapser(
                                            [new LocalizedLabel("imaging-study.series.instance")], [
                                                new ListBuilder("f:instance", FlexDirection.Column,
                                                    (_, _, itemNav, next) =>
                                                    [
                                                        new Container([
                                                            new ChangeContext("f:uid",
                                                                new NameValuePair(
                                                                    [
                                                                        new LocalizedLabel(
                                                                            "imaging-study.series.instance.uid")
                                                                    ],
                                                                    [new Text("@value")])),
                                                            new ChangeContext("f:sopClass",
                                                                new NameValuePair(
                                                                    [
                                                                        new LocalizedLabel(
                                                                            "imaging-study.series.instance.sopClass")
                                                                    ],
                                                                    [new Coding()])),
                                                            new Optional("f:number",
                                                                new NameValuePair(
                                                                    [
                                                                        new LocalizedLabel(
                                                                            "imaging-study.series.instance.number")
                                                                    ],
                                                                    [new Text("@value")])),
                                                            new Optional("f:title",
                                                                new NameValuePair(
                                                                    [
                                                                        new LocalizedLabel(
                                                                            "imaging-study.series.instance.title")
                                                                    ],
                                                                    [new Text("@value")])),
                                                        ], optionalClass: "name-value-pair-wrapper w-fit-content"),
                                                        new If(
                                                            _ =>
                                                            {
                                                                const string conditionXpath =
                                                                    "f:uid or f:sopClass or f:number or f:title";
                                                                return next?.EvaluateCondition(conditionXpath) ==
                                                                       true &&
                                                                       itemNav.EvaluateCondition(conditionXpath);
                                                            },
                                                            new ThematicBreak()),
                                                    ],
                                                    flexContainerClasses: "gap-0"),
                                            ]))),
                                    ])
                                ];
                            }
                        )),
                ]), footer: navigator.EvaluateCondition("f:text or f:encounter")
                    ? new Concat([
                        new Optional("f:encounter",
                            new ShowMultiReference(".",
                                (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                                x =>
                                [
                                    new Collapser([new LocalizedLabel("node-names.Encounter")], x.ToList(),
                                        isCollapsed: true)
                                ]
                            )
                        ),
                        new Condition("f:text",
                            new NarrativeCollapser()
                        ),
                    ])
                    : null),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }

    public enum ImagingStudyInfrequentProperties
    {
        Started,
        Referrer,
        NumberOfSeries,
        NumberOfInstances
    }

    public enum SeriesInfrequentProperties
    {
        Number,
        Description,
        NumberOfInstances,
        BodySite,
        Laterality,
        Started,
        Specimen,
    }
}