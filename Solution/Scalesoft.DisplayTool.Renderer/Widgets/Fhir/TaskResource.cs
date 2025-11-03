using Scalesoft.DisplayTool.Renderer.Constants;
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

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<TaskResourceInfrequentProperties>([navigator]);

        var headerInfo = new Container([
            new Container([
                new ConstantText("Úkol"),
                new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.Code),
                    new ConstantText(" ("),
                    new ChangeContext("f:code", new CodeableConcept()),
                    new ConstantText(")")
                ),
            ], ContainerType.Span),
            new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/task-status",
                new ConstantText("Stav žádosti"))
        ], ContainerType.Div, "d-flex align-items-center gap-1");


        var badge = new PlainBadge(new ConstantText("Základní informace"));
        var basicInfo = new Container([
            new NameValuePair(
                new ConstantText("Záměr"),
                new EnumLabel("f:intent", "http://hl7.org/fhir/ValueSet/request-intent")
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.BusinessStatus),
                new NameValuePair(
                    new ConstantText("Obchodní stav"),
                    new ChangeContext("f:businessStatus", new CodeableConcept())
                )
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.Priority),
                new NameValuePair(
                    new ConstantText("Priorita"),
                    new EnumLabel("f:priority", "http://hl7.org/fhir/ValueSet/request-priority")
                )
            ),
            new If(
                _ => infrequentProperties.Contains(TaskResourceInfrequentProperties.DoNotPerform) &&
                     navigator.EvaluateCondition("f:doNotPerform[@value='true']"),
                new NameValuePair(
                    new ConstantText("Zákaz"),
                    new ShowDoNotPerform()
                )
            ),
        ]);

        var timeInfoBadge = new PlainBadge(new ConstantText("Časové údaje"));
        var timeInfo = new Container([
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.ExecutionPeriod),
                new NameValuePair(
                    new ConstantText("Doba provedení"),
                    new ShowPeriod("f:executionPeriod")
                )
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.AuthoredOn),
                new NameValuePair(
                    new ConstantText("Vytvořeno"),
                    new ShowDateTime("f:authoredOn")
                )
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.LastModified),
                new NameValuePair(
                    new ConstantText("Upraveno"),
                    new ShowDateTime("f:lastModified")
                )
            ),
        ]);

        var taskBadge = new PlainBadge(new ConstantText("Detail úkolu"));
        var taskDetail = new Container([
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.Code),
                new NameValuePair(
                    new ConstantText("Typ"),
                    new ChangeContext("f:code", new CodeableConcept())
                )
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.Description),
                new NameValuePair(
                    new ConstantText("Popis"),
                    new Text("f:description/@value")
                )
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.ReasonCode),
                new NameValuePair(
                    new ConstantText("Důvod"),
                    new ChangeContext("f:reasonCode", new CodeableConcept())
                )
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.Note),
                new NameValuePair(
                    new ConstantText("Poznámka"),
                    new ChangeContext("f:note", new ShowAnnotationCompact())
                )
            ),
        ]);

        var participantsBadge = new PlainBadge(new ConstantText("Zainteresované strany"));
        var participants = new Container([
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.BasedOn),
                new NameValuePair(
                    new ConstantText("Na základě"),
                    new ListBuilder("f:basedOn", FlexDirection.Column, _ => [new AnyReferenceNamingWidget()])
                )
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.PartOf),
                new NameValuePair(
                    new ConstantText("Na základě"),
                    new ListBuilder("f:partOf", FlexDirection.Column, _ => [new AnyReferenceNamingWidget()])
                )
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.Focus),
                new NameValuePair(
                    new ConstantText("Zaměření"),
                    new AnyReferenceNamingWidget("f:focus")
                )
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.For),
                new NameValuePair(
                    new ConstantText("Pro subjekt"),
                    new AnyReferenceNamingWidget("f:for")
                )
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.Requester),
                new NameValuePair(
                    new ConstantText("Žadatel"),
                    new AnyReferenceNamingWidget("f:requester")
                )
            ),
            new If(_ => infrequentProperties.Contains(TaskResourceInfrequentProperties.Owner),
                new NameValuePair(
                    new ConstantText("Owner"),
                    new AnyReferenceNamingWidget("f:owner")
                )
            ),
        ]);

        var complete =
            new Collapser([headerInfo], [], [
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
                                    new Collapser([new ConstantText(Labels.Encounter)], [], x.ToList(),
                                        isCollapsed: true)
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