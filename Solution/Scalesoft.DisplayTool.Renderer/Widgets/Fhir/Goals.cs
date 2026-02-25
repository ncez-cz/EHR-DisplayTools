using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Goals : AlternatingBackgroundColumnResourceBase<Goals>, IResourceWidget
{
    public static string ResourceType => "Goal";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public static bool HasBorderedContainer(Widget widget) => false;


    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<GoalsInfrequentProperties>(navigator);

        return new GoalResource(navigator, infrequentProperties).Render(navigator, renderer, context);
    }

    private class GoalResource(
        XmlDocumentNavigator navigator,
        InfrequentPropertiesDataInContext<GoalsInfrequentProperties> infrequentProperties
    ) : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator _,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var resultWidget = new Concat(
                [
                    new Row(
                        [
                            new Heading(
                                [
                                    new ChangeContext("f:description", new CodeableConcept()),
                                ],
                                HeadingSize.H5,
                                "m-0 blue-color"
                            ),
                            new EnumIconTooltip("f:lifecycleStatus", "http://hl7.org/fhir/ValueSet/goal-status",
                                new EhdsiDisplayLabel(LabelCodes.Status)),
                            infrequentProperties.Optional(GoalsInfrequentProperties.AchievementStatus,
                                new CodeableConceptIconTooltip(new LocalizedLabel("goal.achievementStatus"))),
                            infrequentProperties.Optional(GoalsInfrequentProperties.Priority,
                                new CodeableConceptIconTooltip(
                                    new LocalizedLabel("goal.priority"))),
                            new NarrativeModal(alignRight: false),
                        ],
                        flexContainerClasses: "gap-1 align-items-center", idSource: navigator, flexWrap: false
                    ),
                    new Column(
                        [
                            new Row(
                                [
                                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Category),
                                        new NameValuePair([new LocalizedLabel("goal.category")],
                                            [new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()])],
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary)
                                    ),
                                    new AnyReferenceNamingWidget("f:subject",
                                        showOptionalDetails: false,
                                        widgetModel: new ReferenceNamingWidgetModel
                                        {
                                            Type = ReferenceNamingWidgetType.NameValuePair,
                                            Direction = FlexDirection.Column,
                                            Style = NameValuePair.NameValuePairStyle.Primary,
                                            LabelOverride = new LocalizedLabel("goal.subject"),
                                        }
                                    ),
                                    new NameValuePair([new EhdsiDisplayLabel(LabelCodes.Description)], [
                                            new ChangeContext("f:description", new CodeableConcept())
                                        ],
                                        direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary),
                                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Target),
                                        new NameValuePair([new LocalizedLabel("goal")], [
                                                new Condition("f:target",
                                                    new ConcatBuilder("f:target",
                                                        (_, _, x) =>
                                                        {
                                                            var infrequentTargetProperties =
                                                                InfrequentProperties
                                                                    .Evaluate<GoalsTargetInfrequentProperties>(x);

                                                            Widget[] output =
                                                            [
                                                                new If(
                                                                    _ => infrequentTargetProperties.Contains(
                                                                        GoalsTargetInfrequentProperties.Measure),
                                                                    new NameValuePair(
                                                                        new LocalizedLabel("goal.target.measure"),
                                                                        new Optional("f:measure",
                                                                            new CodeableConcept()),
                                                                        direction: FlexDirection.Row,
                                                                        style: NameValuePair.NameValuePairStyle
                                                                            .Secondary
                                                                    )
                                                                ),
                                                                new If(
                                                                    _ => infrequentTargetProperties.Contains(
                                                                        GoalsTargetInfrequentProperties
                                                                            .Due),
                                                                    new NameValuePair(
                                                                        new LocalizedLabel("goal.target.due"),
                                                                        new Chronometry("due"),
                                                                        direction: FlexDirection.Row,
                                                                        style: NameValuePair.NameValuePairStyle
                                                                            .Secondary
                                                                    )
                                                                ),
                                                                new If(
                                                                    _ => infrequentTargetProperties.Contains(
                                                                        GoalsTargetInfrequentProperties
                                                                            .Detail),
                                                                    new NameValuePair(
                                                                        new LocalizedLabel("goal.target.detail"),
                                                                        new OpenTypeElement(null,
                                                                            "detail"), // Quantity | Range | CodeableConcept | string | boolean | integer | Ratio
                                                                        direction: FlexDirection.Row,
                                                                        style: NameValuePair.NameValuePairStyle
                                                                            .Secondary
                                                                    )
                                                                )
                                                            ];

                                                            return output;
                                                        }, separator: new LineBreak())
                                                )
                                            ],
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary)
                                    ),
                                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Start),
                                        new NameValuePair(
                                            new LocalizedLabel("goal.start"),
                                            new OpenTypeElement(null, "start"),
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary // date | CodeableConcept
                                        )
                                    ),
                                    infrequentProperties.Optional(GoalsInfrequentProperties.StatusDate,
                                        new NameValuePair(
                                            new LocalizedLabel("goal.statusDate"),
                                            new ShowDateTime(),
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary
                                        )
                                    ),
                                    infrequentProperties.Optional(GoalsInfrequentProperties.StatusReason,
                                        new NameValuePair(
                                            new LocalizedLabel("goal.statusReason"),
                                            new Text("@value"),
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary
                                        )
                                    ),
                                    infrequentProperties.Optional(GoalsInfrequentProperties.ExpressedBy,
                                        new AnyReferenceNamingWidget(
                                            widgetModel: new ReferenceNamingWidgetModel
                                            {
                                                Type = ReferenceNamingWidgetType.NameValuePair,
                                                LabelOverride = new LocalizedLabel("goal.expressedBy"),
                                                Direction = FlexDirection.Column,
                                                Style = NameValuePair.NameValuePairStyle.Primary,
                                            }
                                        )
                                    ),
                                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Addresses),
                                        new NameValuePair([new LocalizedLabel("goal.addresses")], [
                                                new Optional("f:addresses",
                                                    new LocalizedLabel("general.see-detail")
                                                ),
                                            ],
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary)
                                    ),
                                    new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Outcome),
                                        new NameValuePair([new EhdsiDisplayLabel(LabelCodes.Result)], [
                                                new Concat([
                                                    new ConcatBuilder("f:outcomeCode",
                                                        _ => [new CodeableConcept()], separator: new LineBreak()),
                                                    new Condition("f:outcomeReference and f:outcomeCode",
                                                        new LineBreak()
                                                    ),
                                                    new ConcatBuilder("f:outcomeReference",
                                                        _ => [new AnyReferenceNamingWidget()],
                                                        separator: new LineBreak()),
                                                ]),
                                            ],
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary)
                                    ),
                                ],
                                flexContainerClasses: "column-gap-6 row-gap-1"
                            ),
                            new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Addresses),
                                new Collapser(
                                    [new LocalizedLabel("condition-plural")],
                                    [new ShowMultiReference("f:addresses", displayResourceType: false)]
                                )),
                            new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Note),
                                new Collapser([new LocalizedLabel("goal.note")], [
                                    new ConcatBuilder("f:note",
                                        _ => [new ShowAnnotationCompact()], separator: new LineBreak())
                                ])),
                            new If(_ => infrequentProperties.Contains(GoalsInfrequentProperties.Text),
                                new NarrativeCollapser()),
                        ],
                        flexContainerClasses: "px-2 gap-1"
                    ),
                ]
            );

            return resultWidget.Render(navigator, renderer, context);
        }
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
    Text,
}

public enum GoalsTargetInfrequentProperties
{
    Measure,
    [OpenType("detail")] Detail,
    [OpenType("due")] Due,
}