using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Goals(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "Goal";
    [UsedImplicitly]
    public static bool RequiresExternalTitle => true;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new Goals(items)];
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<GoalsInfrequentProperties>(items);

        Widget tree = new Table([
            new TableHead([
                new TableRow([
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Category),
                        new TableCell([new ConstantText("Kategorie")], TableCellType.Header)
                    ),
                    new TableCell([new ConstantText("Předmět")], TableCellType.Header),
                    new TableCell([new DisplayLabel(LabelCodes.Description)], TableCellType.Header),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Target),
                        new TableCell([new ConstantText("Cíl")], TableCellType.Header)
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Start),
                        new TableCell([new ConstantText("Datum počátku")], TableCellType.Header)
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.StatusDate),
                        new TableCell([new ConstantText("Datum stavu")], TableCellType.Header)
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.StatusReason),
                        new TableCell([new ConstantText("Důvod stavu")], TableCellType.Header)
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.ExpressedBy),
                        new TableCell([new ConstantText("Vytvořil")], TableCellType.Header)
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Addresses),
                        new TableCell([new ConstantText("Problémy")], TableCellType.Header)
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Note),
                        new TableCell([new ConstantText("Poznámka")], TableCellType.Header)
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Outcome),
                        new TableCell([new DisplayLabel(LabelCodes.Result)], TableCellType.Header)
                    ),
                    new TableCell([new ConstantText("Další")], TableCellType.Header),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Text),
                        new NarrativeCell(false, TableCellType.Header)
                    ),
                ])
            ]),
            ..items.Select(item => new GoalsRow(item, infrequentProperties))
        ], true);

        return tree.Render(navigator, renderer, context);
    }
}

public class GoalsRow(
    XmlDocumentNavigator navigator,
    InfrequentPropertiesData<GoalsInfrequentProperties> infrequentProperties
) : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator _, IWidgetRenderer renderer, RenderContext context)
    {
        var collapsibleRow = new StructuredDetails();

        if (navigator.EvaluateCondition("f:addresses"))
        {
            collapsibleRow.AddCollapser(
                new ConstantText("Potíže"),
                new ShowMultiReference("f:addresses", displayResourceType: false)
            );
        }

        if (navigator.EvaluateCondition("f:note"))
        {
            collapsibleRow.AddCollapser(
                new ConstantText("Poznámky"),
                new ConcatBuilder("f:note",
                    _ => [new ShowAnnotationCompact()], separator: new LineBreak())
            );
        }

        if (navigator.EvaluateCondition("f:text"))
        {
            collapsibleRow.AddCollapser(new DisplayLabel(LabelCodes.OriginalNarrative), new Narrative("f:text"));
        }


        var row =
            new TableBody([
                new TableRow([
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Category),
                        new TableCell([
                            new Condition("f:category",
                                new ConcatBuilder("f:category",
                                    _ => [new CodeableConcept()], separator: new LineBreak())
                            )
                        ])
                    ),
                    new TableCell([
                        new AnyReferenceNamingWidget("f:subject")
                    ]),
                    new TableCell([
                        new Optional("f:description", new CodeableConcept())
                    ]),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Target),
                        new TableCell([
                            new Condition("f:target",
                                new ConcatBuilder("f:target",
                                    (_, _, x) =>
                                    {
                                        var infrequentTargetProperties =
                                            InfrequentProperties
                                                .Evaluate<GoalsTargetInfrequentProperties>([x]);

                                        Widget[] output =
                                        [
                                            new If(
                                                _ => infrequentTargetProperties.Contains(
                                                    GoalsTargetInfrequentProperties.Measure),
                                                new NameValuePair(
                                                    new ConstantText("Parametr"),
                                                    new Optional("f:measure", new CodeableConcept())
                                                )
                                            ),
                                            new If(
                                                _ => infrequentTargetProperties.Contains(GoalsTargetInfrequentProperties
                                                    .Due),
                                                new NameValuePair(
                                                    new ConstantText("Termín"),
                                                    new Chronometry("due")
                                                )
                                            ),
                                            new If(
                                                _ => infrequentTargetProperties.Contains(GoalsTargetInfrequentProperties
                                                    .Detail),
                                                new NameValuePair(
                                                    new ConstantText("Detail"),
                                                    new OpenTypeElement(null,
                                                        "detail") // Quantity | Range | CodeableConcept | string | boolean | integer | Ratio
                                                )
                                            )
                                        ];

                                        return output;
                                    }, separator: new LineBreak())
                            )
                        ])
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Start),
                        new TableCell([new OpenTypeElement(null, "start")]) // date | CodeableConcept
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.StatusDate),
                        new TableCell([
                            new Optional("f:statusDate", new ShowDateTime())
                        ])
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.StatusReason),
                        new TableCell([
                            new Optional("f:statusReason", new Text("@value")),
                        ])
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.ExpressedBy),
                        new TableCell([
                            new Optional("f:expressedBy",
                                new AnyReferenceNamingWidget()
                            )
                        ])
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Addresses),
                        new TableCell([
                            new Optional("f:addresses",
                                new ConstantText("viz detail")
                            )
                        ])
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Note),
                        new TableCell([
                            new Optional("f:note",
                                new ConstantText("viz detail")
                            )
                        ])
                    ),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Outcome),
                        new TableCell([
                            new Condition("f:outcomeReference or f:outcomeCode",
                                new Concat([
                                    new ConcatBuilder("f:outcomeCode",
                                        _ => [new CodeableConcept()], separator: new LineBreak()),
                                    new Condition("f:outcomeReference and f:outcomeCode",
                                        new LineBreak()
                                    ),
                                    new ConcatBuilder("f:outcomeReference",
                                        _ => [new AnyReferenceNamingWidget()], separator: new LineBreak()),
                                ])
                            )
                        ])),
                    new TableCell([
                        new Concat([
                            new EnumIconTooltip("f:lifecycleStatus", "http://hl7.org/fhir/ValueSet/goal-status",
                                new DisplayLabel(LabelCodes.Status)),
                            new Optional("f:achievementStatus",
                                new CodeableConceptIconTooltip(new ConstantText("Pokrok"))),
                            new Optional("f:priority",
                                new CodeableConceptIconTooltip(new ConstantText("Priorita"))), //perhaps not?
                        ])
                    ]),
                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Text),
                        new NarrativeCell()
                    ),
                ], collapsibleRow, idSource: navigator),
            ]);

        return row.Render(navigator, renderer, context);
    }
}

public enum GoalsInfrequentProperties
{
    AchievementStatus,
    Category,
    Priority,
    [OpenType("start")] Start,
    [OpenType("outcome")] Outcome,
    Target,
    StatusDate,
    StatusReason,
    ExpressedBy,
    Addresses,
    Note,
    Text
}

public enum GoalsTargetInfrequentProperties
{
    Measure,
    [OpenType("detail")] Detail,
    [OpenType("due")] Due
}