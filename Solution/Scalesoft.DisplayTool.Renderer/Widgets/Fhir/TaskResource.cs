using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class TaskResource : ColumnResourceBase<TaskResource>, IResourceWidget
{
    public static string ResourceType => "Task";

    public static bool HasBorderedContainer(Widget widget) => true;

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<TaskResourceInfrequentProperties>(navigator);

        var headerInfo = new Container([
            new Container([
                new LocalizedLabel("task"),
                infrequentProperties.Optional(TaskResourceInfrequentProperties.Code,
                    new ConstantText(" ("),
                    new CodeableConcept(),
                    new ConstantText(")")
                ),
            ], ContainerType.Span),
            new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/task-status",
                new LocalizedLabel("task.status"))
        ], ContainerType.Div, "d-flex align-items-center gap-1");


        var badge = new PlainBadge(new LocalizedLabel("general.basic-information"));
        var basicInfo = new Container([
            new NameValuePair(
                new LocalizedLabel("task.intent"),
                new EnumLabel("f:intent", "http://hl7.org/fhir/ValueSet/request-intent")
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.BusinessStatus,
                new NameValuePair(
                    new LocalizedLabel("task.businessStatus"),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.Priority,
                new NameValuePair(
                    new LocalizedLabel("task.priority"),
                    new EnumLabel(".", "http://hl7.org/fhir/ValueSet/request-priority")
                )
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.DoNotPerform,
                new ShowBoolean(
                    new NullWidget(),
                    new NameValuePair(
                        new LocalizedLabel("task.doNotPerform"),
                        new ShowDoNotPerform()
                    )
                )
            ),
        ]);

        var timeInfoBadge = new PlainBadge(new LocalizedLabel("task.chronometric-data"));
        var timeInfo = new Container([
            infrequentProperties.Optional(TaskResourceInfrequentProperties.ExecutionPeriod,
                new NameValuePair(
                    new LocalizedLabel("task.executionPeriod"),
                    new ShowPeriod()
                )
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.AuthoredOn,
                new NameValuePair(
                    new LocalizedLabel("task.authoredOn"),
                    new ShowDateTime()
                )
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.LastModified,
                new NameValuePair(
                    new LocalizedLabel("task.lastModified"),
                    new ShowDateTime()
                )
            ),
        ]);

        var taskBadge = new PlainBadge(new LocalizedLabel("task.task-details"));
        var taskDetail = new Container([
            infrequentProperties.Optional(TaskResourceInfrequentProperties.Code,
                new NameValuePair(
                    new LocalizedLabel("task.description"),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.Description,
                new NameValuePair(
                    new LocalizedLabel("task.description"),
                    new Text("@value")
                )
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.ReasonCode,
                new NameValuePair(
                    new LocalizedLabel("task.reasonCode"),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.Note,
                new NameValuePair(
                    new LocalizedLabel("task.note"),
                    new ShowAnnotationCompact()
                )
            ),
        ]);

        var participantsBadge = new PlainBadge(new LocalizedLabel("general.involvedParties"));
        var participants = new Container([
            infrequentProperties.Condition(TaskResourceInfrequentProperties.BasedOn,
                new NameValuePair(
                    new LocalizedLabel("task.basedOn"),
                    new ListBuilder("f:basedOn", FlexDirection.Column, _ => [new AnyReferenceNamingWidget()])
                )
            ),
            infrequentProperties.Condition(TaskResourceInfrequentProperties.PartOf,
                new NameValuePair(
                    new LocalizedLabel("task.partOf"),
                    new ListBuilder("f:partOf", FlexDirection.Column, _ => [new AnyReferenceNamingWidget()])
                )
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.Focus,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("task.focus"),
                    }
                )
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.For,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("task.for"),
                    }
                )
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.Requester,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("task.requester"),
                    }
                )
            ),
            infrequentProperties.Optional(TaskResourceInfrequentProperties.Owner,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("task.owner"),
                    }
                )
            ),
        ]);

        var complete =
            new Collapser([headerInfo], [
                    badge,
                    basicInfo,
                    new If(_ =>
                            infrequentProperties.ContainsAnyOf(TaskResourceInfrequentProperties.AuthoredOn,
                                TaskResourceInfrequentProperties.LastModified,
                                TaskResourceInfrequentProperties.ExecutionPeriod),
                        new ThematicBreak(),
                        timeInfoBadge,
                        timeInfo
                    ),
                    new If(_ =>
                            infrequentProperties.ContainsAnyOf(TaskResourceInfrequentProperties.Code,
                                TaskResourceInfrequentProperties.Description,
                                TaskResourceInfrequentProperties.ReasonCode, TaskResourceInfrequentProperties.Note),
                        new ThematicBreak(),
                        taskBadge,
                        taskDetail
                    ),
                    new If(_ =>
                            infrequentProperties.ContainsAnyOf(TaskResourceInfrequentProperties.BasedOn,
                                TaskResourceInfrequentProperties.Focus, TaskResourceInfrequentProperties.For,
                                TaskResourceInfrequentProperties.Requester, TaskResourceInfrequentProperties.Owner),
                        new ThematicBreak(),
                        participantsBadge,
                        participants
                    ),
                ], footer: infrequentProperties.ContainsAnyOf(TaskResourceInfrequentProperties.Encounter,
                    TaskResourceInfrequentProperties.Text)
                    ?
                    [
                        new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.Encounter),
                            new ShowMultiReference("f:encounter",
                                (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                                x =>
                                [
                                    new Collapser([new LocalizedLabel("node-names.Encounter")], x.ToList(),
                                        isCollapsed: true),
                                ]
                            )
                        ),
                        new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.Text),
                            new NarrativeCollapser()
                        ),
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()]
            );


        return await complete.Render(navigator, renderer, context);
    }
}

public enum TaskResourceInfrequentProperties
{
    BasedOn,
    Focus,
    For,
    BusinessStatus,
    Priority,
    Code,
    ExecutionPeriod,
    AuthoredOn,
    LastModified,
    Description,
    Requester,
    Owner,
    ReasonCode,
    Note,
    PartOf,
    DoNotPerform,
    Text,
    Encounter,
}