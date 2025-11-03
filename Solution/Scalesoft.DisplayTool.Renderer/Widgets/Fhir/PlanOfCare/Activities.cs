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
public class Activities(XmlDocumentNavigator item) : Widget
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
            widgetsToRender.Add(new HideableDetails(timelineWidget));
        }

        // Add unscheduled activities card
        if (unscheduledActivityItems.Count > 0)
        {
            widgetsToRender.Add(new HideableDetails(CreateUnscheduledActivitiesSection(unscheduledActivityItems)));
        }

        // Handle accumulated fatal errors before rendering
        if (processingErrors.Count != 0 && processingErrors.MaxSeverity() >= ErrorSeverity.Fatal)
        {
            return processingErrors;
        }

        // Render all collected widgets (timelines and unscheduled activities)
        var finalRenderResult = await widgetsToRender.RenderConcatenatedResult(item, renderer, context);

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
            var activityTitle = new ActivityTitle(activityNavigator);
            var scheduledTime = GetActivityScheduledTime(activityNavigator, errors); // Pass errors list for logging

            var (isRequestGroup, authoredOnDate) = CheckIfRepresentsRequestGroup(activityNavigator, renderer, context);

            var timelineItem = new TimelineCard([new CarePlanActivityBuilder(activityNavigator)],
                activityTitle, scheduledTime, isNested: isRequestGroup);

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

        var card = new Card(
            new ConstantText("Aktivity bez určeného času"),
            new Concat(timelineCards), optionalClass: "unscheduled-activities-container");
        return card;
    }

    private class ActivityTitle(XmlDocumentNavigator item) : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator _,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var choose = new Choose([
                new When("f:detail/f:code/f:coding/f:text/@value", new Text()),
            ], new ConstantText("Aktivita"));

            return choose.Render(item, renderer, context);
        }
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
                new ConstantText("Skupina žádanek"),
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
            Widget[] widgetTree =
            [
                new Choose([
                    new When("f:outcomeCodeableConcept", new NameValuePair(
                        [new DisplayLabel(LabelCodes.Outcome)],
                        [
                            new ItemListBuilder("f:outcomeCodeableConcept", ItemListType.Unordered,
                                _ => [new CodeableConcept()])
                        ]
                    )),
                    new When("f:outcomeReference", new NameValuePair(
                        [new DisplayLabel(LabelCodes.Outcome)],
                        [
                            new Collapser([new ConstantText("Detail výsledků")], [],
                                [new ShowMultiReference("f:outcomeReference", displayResourceType: false)]),
                        ]
                    ))
                ]),
                new Choose([
                    new When("f:progress", new NameValuePair(
                        [new ConstantText("Stav / pokrok")],
                        [
                            new ItemListBuilder("f:progress", ItemListType.Unordered,
                                (_, x) => [new Container([new ShowAnnotationCompact()], idSource: x)])
                        ]
                    ))
                ]),
                new Choose([
                    new When("f:reference", new NameValuePair(
                        [new ConstantText("Detail")],
                        [
                            new ShowSingleReference(x =>
                            {
                                if (x.ResourceReferencePresent)
                                {
                                    return [new AnyResource(x.Navigator, displayResourceType: false)];
                                }

                                return
                                [
                                    new Container([new ConstantText(x.ReferenceDisplay)], ContainerType.Span,
                                        idSource: x.Navigator ?? (IdentifierSource?)null),
                                ];
                            }, "f:reference")
                        ]
                    )),
                    new When("f:detail", new ChangeContext("f:detail",
                        new Choose([
                            // ignore kind
                            // ignore instantiatesCanonical
                            // ignore instantiatesUri
                            new When("f:code", new NameValuePair(
                                [new ConstantText("Typ")],
                                [new ChangeContext("f:code", new CodeableConcept())]
                            ))
                        ]), new Choose([
                            new When("f:reasonCode|f:reasonReference", new NameValuePair([new ConstantText("Důvod")], [
                                new Choose([
                                    new When("f:reasonCode", new ChangeContext("f:reasonCode", new CodeableConcept())),
                                    new When("f:reasonReference",
                                        new Collapser([new ConstantText("Detail důvodů")], [],
                                            [new ShowMultiReference("f:reasonReference", displayResourceType: false)]))
                                ]),
                            ]))
                        ]), new Choose([
                            new When("f:goal", new NameValuePair([new ConstantText("Cíl")], [
                                new ItemListBuilder("f:goal", ItemListType.Unordered, _ =>
                                [
                                    ShowSingleReference.WithDefaultDisplayHandler(x =>
                                    [
                                        new ChangeContext(x,
                                            FhirCarePlan.NarrativeAndOrChildren([
                                                new Container(
                                                    [new ChangeContext("f:description", new CodeableConcept())],
                                                    ContainerType.Span, idSource: x)
                                            ])
                                        )
                                    ])
                                ])
                            ]))
                        ]), new Choose([
                            new When("f:status", new NameValuePair(
                                [new DisplayLabel(LabelCodes.Status)],
                                [
                                    new EnumLabel("f:status",
                                        "http://hl7.org/fhir/ValueSet/care-plan-activity-status")
                                ]
                            ))
                        ]), new Choose([
                            new When("f:statusReason", new NameValuePair([new ConstantText("Důvod stavu")],
                                [new ChangeContext("f:statusReason", new CodeableConcept())]))
                        ]), new Choose([
                            new When("f:scheduledTiming|f:scheduledPeriod|f:scheduledString",
                                new NameValuePair([new ConstantText("Čas")], [new Chronometry("scheduled")]))
                        ]), new Choose([
                            new When("f:location", new NameValuePair([new ConstantText("Lokace")], [
                                ShowSingleReference.WithDefaultDisplayHandler(x =>
                                [
                                    new TextContainer(TextStyle.Regular, [
                                        FhirCarePlan.NarrativeAndOrChildren([
                                            new Container([
                                                new Choose([
                                                    new When("f:name", new Text("f:name/@value"), new LineBreak())
                                                ]),
                                                new Choose([
                                                    new When("f:alias", new ConcatBuilder("f:alias",
                                                        _ => [new Text("@value")],
                                                        ", "), new LineBreak())
                                                ]),
                                                new Choose([
                                                    new When("f:address", new Address("f:address"), new LineBreak())
                                                ]),
                                                new Choose([
                                                    new When("f:description", new Text("f:description/@value"),
                                                        new LineBreak())
                                                ])
                                            ], ContainerType.Span, idSource: x),
                                        ]),
                                    ])
                                ], "f:location")
                            ]))
                        ]), new Choose([
                            new When(
                                "f:performer",
                                new NameValuePair(
                                    [new ConstantText("Odpovědná osoba/tým/zařízení")],
                                    [
                                        new ItemListBuilder(
                                            "f:performer",
                                            ItemListType.Unordered,
                                            _ => [new AnyReferenceNamingWidget()]
                                        ),
                                    ]
                                )
                            ),
                        ]), new Choose([
                            new When("f:productCodeableConcept|f:productReference", new NameValuePair(
                                [new DisplayLabel(LabelCodes.MedicalProduct)], [
                                    new Choose([
                                        new When("f:productCodeableConcept",
                                            new ChangeContext("f:productCodeableConcept", new CodeableConcept()))
                                    ]),
                                    new Choose([
                                        new When("f:productReference", ShowSingleReference.WithDefaultDisplayHandler(
                                            x =>
                                            [
                                                new Container([new ChangeContext("f:code", new CodeableConcept())],
                                                    ContainerType.Span,
                                                    idSource: x)
                                            ],
                                            "f:productReference"))
                                    ]),
                                ]))
                        ]), new Choose([
                            new When("f:dailyAmount", new NameValuePair([new ConstantText("Denní množství")],
                                [new ShowQuantity("f:dailyAmount")]))
                        ]), new Choose([
                            new When("f:quantity",
                                new NameValuePair([new ConstantText("Množství")], [new ShowQuantity("f:quantity")]))
                        ]), new ShowDoNotPerform(), new Choose([
                            new When("f:description", new NameValuePair(
                                [new DisplayLabel(LabelCodes.Description)],
                                [new Text("f:description/@value")]
                            ))
                        ])))
                ])
            ];

            return widgetTree.RenderConcatenatedResult(item, renderer, context);
        }
    }
}