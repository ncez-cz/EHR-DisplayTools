using System.Globalization;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PlanOfCare;

/// <summary>
///     Processes and renders activities associated with a Care Plan.
///     This includes scheduled activities (in a timeline), unscheduled activities,
///     and activities from RequestGroups.
/// </summary>
public class PlanActivities(XmlDocumentNavigator item) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator _,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var processingErrors = new List<ParseError>();
        var widgetsToRender = new List<Widget>();

        // Process activity nodes
        var (regularActivityItems, requestGroupActivityItems, unscheduledActivityItems, activityErrors) =
            await ProcessActivityNodesAsync(item, renderer, context);
        processingErrors.AddRange(activityErrors);

        // Group RequestGroup items by their 'authoredOn' date
        var groupedRequestGroupItems = GroupRequestGroupItemsByDate(requestGroupActivityItems);

        // Combine regular activities and grouped RequestGroups
        var combinedActivityAndGroupItems = new List<DateSortableWidget>();
        combinedActivityAndGroupItems.AddRange(regularActivityItems);
        combinedActivityAndGroupItems.AddRange(groupedRequestGroupItems);

        // Sort the combined activities chronologically by their scheduled time
        combinedActivityAndGroupItems = WidgetSorter.Sort(combinedActivityAndGroupItems);

        // If there are timeline items (activities), create and add the Timeline widget
        if (combinedActivityAndGroupItems.Count > 0)
        {
            var timelineWidget = new Timeline(combinedActivityAndGroupItems, "care-plan-timeline");
            widgetsToRender.Add(timelineWidget);
        }

        // Add unscheduled activities card
        if (unscheduledActivityItems.Count > 0)
        {
            widgetsToRender.Add(CreateUnscheduledActivitiesSection(unscheduledActivityItems));
        }

        // Handle accumulated fatal errors before rendering
        if (processingErrors.Count != 0 && processingErrors.MaxSeverity() >= ErrorSeverity.Fatal)
        {
            return processingErrors;
        }

        var resultWidget = new HideableDetails(new Section(".", null, [new LocalizedLabel("care-plan.activity")], widgetsToRender));

        // Render all collected widgets (timelines and unscheduled activities)
        var finalRenderResult = await resultWidget.Render(item, renderer, context);

        // Add non-fatal errors to the final rendered result
        finalRenderResult.Errors.AddRange(processingErrors);
        return finalRenderResult;
    }

    private Task<(List<TimelineCard> RegularActivities,
        List<TimelineCard> RequestGroupActivities,
        List<TimelineCard> UnscheduledActivities,
        List<ParseError> Errors)> ProcessActivityNodesAsync(
        XmlDocumentNavigator carePlanNavigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var regularActivities = new List<TimelineCard>();
        var requestGroupActivities = new List<TimelineCard>();
        var unscheduledActivities = new List<TimelineCard>();
        var errors = new List<ParseError>();
        var activityNavigators = carePlanNavigator.SelectAllNodes("f:activity"); // Get all activity nodes

        foreach (var activityNavigator in activityNavigators)
        {
            var scheduledTime = GetActivityScheduledTime(activityNavigator, errors); // Pass errors list for logging

            var (isRequestGroup, authoredOnDate) = CheckIfRepresentsRequestGroup(activityNavigator, renderer, context);

            var timelineItem = new TimelineCard([new CarePlanActivityBuilder(activityNavigator)],
                null, scheduledTime, isNested: isRequestGroup);

            if (isRequestGroup)
            {
                timelineItem.CssClass += " request-group-item"; // Add specific class

                if (authoredOnDate.HasValue)
                {
                    timelineItem.SortDate =
                        authoredOnDate.Value.Date; // Store authoredOn date (Date part only) for grouping
                    requestGroupActivities.Add(timelineItem);
                }
                else
                {
                    // RequestGroups without authoredOn date should go to unscheduled category
                    unscheduledActivities.Add(timelineItem);
                }
            }
            else if (scheduledTime == null)
            {
                unscheduledActivities.Add(timelineItem);
            }
            else
            {
                regularActivities.Add(timelineItem);
            }
        }

        return Task.FromResult((regularActivities, requestGroupActivities, unscheduledActivities, errors));
    }

    private Widget CreateUnscheduledActivitiesSection(List<TimelineCard> unscheduledActivities)
    {
        if (unscheduledActivities.Count == 0)
        {
            return new NullWidget();
        }


        List<Widget> timelineCards = [];
        timelineCards.AddRange(unscheduledActivities);

        var resultWidget = new Concat([
            new Row(
                [
                    new Heading(
                        [
                            new LocalizedLabel("care-plan.activity.without-specified-time"),
                        ],
                        HeadingSize.H5,
                        "m-0 blue-color"
                    ),
                ]
            ),
            ..timelineCards,
        ]);

        return resultWidget;
    }

    private DateTimeOffset? GetActivityScheduledTime(XmlDocumentNavigator activityNavigator, List<ParseError> errors)
    {
        // Attempts to get the scheduled time from primary path or a single reference
        var scheduledDateString = GetDateValueFromPaths(activityNavigator);

        if (scheduledDateString == null)
        {
            // Fallback: Try getting date from a single reference node
            var referencesWithContent = ReferenceHandler.GetContentFromReferences(activityNavigator, "f:reference");
            switch (referencesWithContent.Count)
            {
                case 1:
                    scheduledDateString = GetDateValueFromPaths(referencesWithContent[0]);
                    break;
                case > 1:
                    // Log a warning if multiple references exist
                    errors.Add(new ParseError
                    {
                        Kind = ErrorKind.TooManyValues,
                        Path = activityNavigator.GetFullPath(),
                        Message =
                            "Multiple references found for activity date; unable to determine scheduled time reliably.",
                        Severity = ErrorSeverity.Warning
                    });
                    break;
            }
        }

        return DateTimeOffset.TryParse(scheduledDateString, CultureInfo.InvariantCulture, out var parsedDate)
            ? parsedDate
            : null;
    }

    private (bool IsRequestGroup, DateTimeOffset? AuthoredOnDate) CheckIfRepresentsRequestGroup(
        XmlDocumentNavigator activityNavigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        // Checks if the activity references a RequestGroup and extracts its authoredOn date
        var referenceNode = activityNavigator.SelectSingleNode("f:reference");

        if (referenceNode.Node == null) // Check if the node itself exists
        {
            return (false, null); // Not a RequestGroup reference or reference node doesn't exist
        }

        var resourceType =
            ReferenceHandler.GetReferenceName("f:reference/f:reference", activityNavigator, renderer, context);
        if (resourceType != "RequestGroup")
        {
            return (false, null); // Not a RequestGroup reference or reference node doesn't exist
        }

        DateTimeOffset? authoredOnDate = null;
        var authoredOnPath =
            ReferenceHandler.GetSingleNodeNavigatorFromReference(activityNavigator, "f:reference",
                "f:authoredOn/@value");
        if (authoredOnPath == null)
        {
            return (true, authoredOnDate);
        }

        var authoredOnDateString = authoredOnPath.Node?.Value;
        if (DateTimeOffset.TryParse(authoredOnDateString, CultureInfo.InvariantCulture, out var parsedDate))
        {
            authoredOnDate = parsedDate;
        }

        return (true, authoredOnDate);
    }

    private List<TimelineCard> GroupRequestGroupItemsByDate(
        List<TimelineCard> requestGroupItems
    )
    {
        // Groups RequestGroup items into containers based on their authoredOn date (stored in MetaData)
        var requestGroupsByDate = requestGroupItems
            .GroupBy(x => x.SortDate); // Group by the stored authoredOn date

        var requestGroupContainerItems = new List<TimelineCard>();
        foreach (var group in requestGroupsByDate)
        {
            var groupDate = group.Key;
            // Create a container item for the group
            requestGroupContainerItems.Add(new TimelineCard(
                [new NullWidget()],
                new LocalizedLabel("request-group"),
                groupDate,
                "request-group-container",
                group
                    .OrderBy(x => x.SortDate ?? DateTimeOffset.MaxValue)
                    .ToList()
            ));
        }

        return requestGroupContainerItems;
    }

    private static string? GetDateValueFromPaths(XmlDocumentNavigator navigator)
    {
        var datePaths = new[]
        {
            "f:detail/f:scheduledPeriod/f:start/@value",
            "f:period/f:start/@value",
            "f:start/@value",
            "f:occurrenceDateTime/@value",
            "f:occurrencePeriod/f:start/@value",
            "f:restriction/f:period/f:start/@value",
            "f:dateWritten/@value",
            "f:dateTime/@value",
            "f:authoredOn/@value",
            "f:created/@value"
        };

        return datePaths
            .Select(path => navigator
                .SelectSingleNode(path).Node?.Value)
            .FirstOrDefault(nodeValue => !string
                .IsNullOrEmpty(nodeValue));
    }

    private class CarePlanActivityBuilder(XmlDocumentNavigator item) : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator _,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var innerContentWidget = new Concat(
                [
                    new Column(
                        [
                            new Condition("f:outcomeCodeableConcept or f:outcomeReference or f:progress",
                                new Row(
                                    [
                                        new Condition("f:outcomeCodeableConcept", new NameValuePair(
                                            [new EhdsiDisplayLabel(LabelCodes.Outcome)],
                                            [
                                                new CommaSeparatedBuilder("f:outcomeCodeableConcept",
                                                    _ => [new CodeableConcept()])
                                            ],
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary
                                        )),
                                        new Condition("f:outcomeReference", new NameValuePair(
                                            [new EhdsiDisplayLabel(LabelCodes.Outcome)],
                                            [
                                                new Collapser([new LocalizedLabel("care-plan.activity.outcomeReference")],
                                                [
                                                    new ShowMultiReference("f:outcomeReference",
                                                        displayResourceType: false)
                                                ]),
                                            ],
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary
                                        )),
                                        new Condition("f:progress", new NameValuePair(
                                            [new LocalizedLabel("care-plan.activity.progress")],
                                            [
                                                new ItemListBuilder("f:progress", ItemListType.Unordered,
                                                    (_, x) =>
                                                    [
                                                        new Container([new ShowAnnotationCompact()], idSource: x)
                                                    ])
                                            ],
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary
                                        ))
                                    ],
                                    flexContainerClasses: "column-gap-6 row-gap-1"
                                )
                            ),
                            new Condition(
                                "(f:outcomeCodeableConcept or f:outcomeReference or f:progress) and (f:reference or f:detail)",
                                new ThematicBreak()),
                            new Choose([
                                new When("f:reference", new ShowSingleReference(x =>
                                {
                                    if (x.ResourceReferencePresent)
                                    {
                                        return
                                        [
                                            new AnyResource(x.Navigator, displayResourceType: false,
                                                displayBorder: true)
                                        ];
                                    }

                                    return
                                    [
                                        new Container([new ConstantText(x.ReferenceDisplay)],
                                            ContainerType.Span,
                                            idSource: x.Navigator ?? (IdentifierSource?)null),
                                    ];
                                }, "f:reference")),
                                new When("f:detail", new ChangeContext("f:detail",
                                    new Choose([
                                        // ignore kind
                                        // ignore instantiatesCanonical
                                        // ignore instantiatesUri
                                        new When("f:code", new NameValuePair(
                                            [new LocalizedLabel("care-plan.activity.detail.code")],
                                            [new ChangeContext("f:code", new CodeableConcept())],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row
                                        ))
                                    ]), new Choose([
                                        new When("f:reasonCode|f:reasonReference", new NameValuePair(
                                            [new LocalizedLabel("care-plan.activity.detail.reason")], [
                                                new Choose([
                                                    new When("f:reasonCode",
                                                        new ChangeContext("f:reasonCode", new CodeableConcept())),
                                                    new When("f:reasonReference",
                                                        new Collapser([new LocalizedLabel("care-plan.activity.detail.reasonReference")],
                                                        [
                                                            new ShowMultiReference("f:reasonReference",
                                                                displayResourceType: false)
                                                        ]))
                                                ]),
                                            ],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row
                                        ))
                                    ]), new Choose([
                                        new When("f:goal", new NameValuePair([new LocalizedLabel("care-plan.activity.goal")], [
                                                new ItemListBuilder("f:goal", ItemListType.Unordered, _ =>
                                                [
                                                    ShowSingleReference.WithDefaultDisplayHandler(x =>
                                                    [
                                                        new ChangeContext(x,
                                                            FhirCarePlan.NarrativeAndOrChildren([
                                                                new Container(
                                                                    [
                                                                        new ChangeContext("f:description",
                                                                            new CodeableConcept())
                                                                    ],
                                                                    ContainerType.Span, idSource: x)
                                                            ])
                                                        )
                                                    ])
                                                ])
                                            ],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row
                                        ))
                                    ]), new Choose([
                                        new When("f:status", new NameValuePair(
                                            [new EhdsiDisplayLabel(LabelCodes.Status)],
                                            [
                                                new EnumLabel("f:status",
                                                    "http://hl7.org/fhir/ValueSet/care-plan-activity-status")
                                            ],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row
                                        ))
                                    ]), new Choose([
                                        new When("f:statusReason", new NameValuePair([new LocalizedLabel("care-plan.activity.statusReason")],
                                            [new ChangeContext("f:statusReason", new CodeableConcept())],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row
                                        ))
                                    ]), new Choose([
                                        new When("f:scheduledTiming|f:scheduledPeriod|f:scheduledString",
                                            new NameValuePair([new LocalizedLabel("care-plan.activity.scheduled")],
                                                [new Chronometry("scheduled")],
                                                style: NameValuePair.NameValuePairStyle.Primary,
                                                direction: FlexDirection.Row
                                            ))
                                    ]), new Choose([
                                        new When("f:location", new NameValuePair([new LocalizedLabel("care-plan.activity.location")], [
                                                ShowSingleReference.WithDefaultDisplayHandler(x =>
                                                [
                                                    new TextContainer(TextStyle.Regular, [
                                                        FhirCarePlan.NarrativeAndOrChildren([
                                                            new Container([
                                                                new Choose([
                                                                    new When("f:name", new Text("f:name/@value"),
                                                                        new LineBreak())
                                                                ]),
                                                                new Choose([
                                                                    new When("f:alias", new ConcatBuilder("f:alias",
                                                                        _ => [new Text("@value")],
                                                                        ", "), new LineBreak())
                                                                ]),
                                                                new Choose([
                                                                    new When("f:address", new Address("f:address"),
                                                                        new LineBreak())
                                                                ]),
                                                                new Choose([
                                                                    new When("f:description",
                                                                        new Text("f:description/@value"),
                                                                        new LineBreak())
                                                                ])
                                                            ], ContainerType.Span, idSource: x),
                                                        ]),
                                                    ])
                                                ], "f:location")
                                            ],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row
                                        ))
                                    ]), new Choose([
                                        new When(
                                            "f:performer",
                                            new NameValuePair(
                                                [new LocalizedLabel("care-plan.activity.performer")],
                                                [
                                                    new ItemListBuilder(
                                                        "f:performer",
                                                        ItemListType.Unordered,
                                                        _ => [new AnyReferenceNamingWidget()]
                                                    ),
                                                ],
                                                style: NameValuePair.NameValuePairStyle.Primary,
                                                direction: FlexDirection.Row
                                            )
                                        ),
                                    ]), new Choose([
                                        new When("f:productCodeableConcept|f:productReference", new NameValuePair(
                                            [new EhdsiDisplayLabel(LabelCodes.MedicalProduct)], [
                                                new Choose([
                                                    new When("f:productCodeableConcept",
                                                        new ChangeContext("f:productCodeableConcept",
                                                            new CodeableConcept()))
                                                ]),
                                                new Choose([
                                                    new When("f:productReference",
                                                        ShowSingleReference.WithDefaultDisplayHandler(
                                                            x =>
                                                            [
                                                                new Container(
                                                                    [
                                                                        new ChangeContext("f:code",
                                                                            new CodeableConcept())
                                                                    ],
                                                                    ContainerType.Span,
                                                                    idSource: x)
                                                            ],
                                                            "f:productReference"))
                                                ]),
                                            ],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row
                                        ))
                                    ]), new Choose([
                                        new When("f:dailyAmount", new NameValuePair(
                                            [new LocalizedLabel("care-plan.activity.dailyAmount")],
                                            [new ShowQuantity("f:dailyAmount")],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row
                                        ))
                                    ]), new Choose([
                                        new When("f:quantity",
                                            new NameValuePair([new LocalizedLabel("care-plan.activity.quantity")],
                                                [new ShowQuantity("f:quantity")],
                                                style: NameValuePair.NameValuePairStyle.Primary,
                                                direction: FlexDirection.Row
                                            ))
                                    ]), new ShowDoNotPerform(), new Choose([
                                        new When("f:description", new NameValuePair(
                                            [new EhdsiDisplayLabel(LabelCodes.Description)],
                                            [new Text("f:description/@value")],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row
                                        ))
                                    ])))
                            ])
                        ],
                        flexContainerClasses: "column-gap-1"
                    ),
                ]
            );

            Widget resultWidget =
                item.EvaluateCondition("f:outcomeCodeableConcept or f:outcomeReference or f:progress or f:detail")
                    ? new Card(null, innerContentWidget)
                    : innerContentWidget;

            return resultWidget.Render(item, renderer, context);
        }
    }
}