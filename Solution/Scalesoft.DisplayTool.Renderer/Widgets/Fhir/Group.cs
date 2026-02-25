using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Group : ColumnResourceBase<Group>, IResourceWidget
{
    public static string ResourceType => "Group";
    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<GroupInfrequentProperties>(navigator);

        var widget =
            new Concat(
                [
                    new Collapser(
                        [new LocalizedLabel("group")],
                        [
                            new PlainBadge(new LocalizedLabel("general.basic-information")),
                            new Container(
                                [
                                    infrequentProperties.Optional(GroupInfrequentProperties.Active,
                                        new TextContainer(TextStyle.Bold,
                                            new ShowBoolean(
                                                new LocalizedLabel("general.inactive"),
                                                new LocalizedLabel("general.active")
                                            ),
                                            optionalClass: "span-over-full-name-value-pair-cell"
                                        )
                                    ),
                                    new NameValuePair(
                                        new LocalizedLabel("group.type"),
                                        new EnumLabel("f:type", "http://hl7.org/fhir/ValueSet/group-type")
                                    ),
                                    new NameValuePair(
                                        new LocalizedLabel("group.actual"),
                                        new ShowBoolean(
                                            new LocalizedLabel("group.actual.false"),
                                            new LocalizedLabel("group.actual.true"),
                                            "f:actual"
                                        )
                                    ),
                                    infrequentProperties.Optional(GroupInfrequentProperties.Code,
                                        new NameValuePair(
                                            new LocalizedLabel("group.code"),
                                            new CodeableConcept()
                                        )
                                    ),
                                    infrequentProperties.Optional(GroupInfrequentProperties.Name,
                                        new NameValuePair(
                                            new EhdsiDisplayLabel(LabelCodes.Name),
                                            new Text("@value")
                                        )
                                    ),
                                    infrequentProperties.Optional(GroupInfrequentProperties.Quantity,
                                        new NameValuePair(
                                            new LocalizedLabel("group.quantity"),
                                            new Text("@value")
                                        )
                                    ),
                                    infrequentProperties.Optional(GroupInfrequentProperties.ManagingEntity,
                                        new AnyReferenceNamingWidget(
                                            widgetModel: new ReferenceNamingWidgetModel
                                            {
                                                Type = ReferenceNamingWidgetType.NameValuePair,
                                                LabelOverride = new LocalizedLabel("group.managingEntity"),
                                            }
                                        )
                                    ),
                                ], optionalClass: "name-value-pair-wrapper w-fit-content"
                            ),
                            new Condition(
                                "f:characteristic",
                                new ThematicBreak(),
                                new PlainBadge(new LocalizedLabel("group.characteristic")),
                                new Container(
                                    [
                                        new Column(
                                            [
                                                new OptionalGroupCharacteristicCard(
                                                    "f:characteristic[f:exclude/@value='false']",
                                                    new LocalizedLabel("group.required-properties")
                                                ),
                                                new OptionalGroupCharacteristicCard(
                                                    "f:characteristic[f:exclude/@value='true']",
                                                    new LocalizedLabel("group.exclusion-properties")
                                                ),
                                            ]
                                        ),
                                    ]
                                )
                            ),
                            new Condition(
                                "f:member",
                                new ThematicBreak(),
                                new PlainBadge(new LocalizedLabel("group.member")),
                                new ItemListBuilder(
                                    "f:member",
                                    ItemListType.Unordered,
                                    (_, x) =>
                                    [
                                        new AnyReferenceNamingWidget("f:entity"),
                                        new Condition(
                                            "f:period",
                                            new ConstantText(" "),
                                            new Tooltip(
                                                [],
                                                [
                                                    new LocalizedLabel("group.member.period"),
                                                ],
                                                icon: new Icon(SupportedIcons.CircleUser)
                                            ),
                                            new TextContainer(
                                                TextStyle.Muted,
                                                [
                                                    new ConstantText(" "),
                                                    new ShowPeriod("f:period"),
                                                ]
                                            )
                                        ),
                                        new ConstantText(" "),
                                        new Tooltip([],
                                        [
                                            new ShowBoolean(
                                                new LocalizedLabel("group.member.inactive.false"),
                                                new LocalizedLabel("group.member.inactive.true"),
                                                "f:inactive"
                                            )
                                        ], icon: x.EvaluateCondition(
                                            "f:inactive[@value='false'] or f:inactive[@value='true']")
                                            ? x.EvaluateCondition("f:inactive[@value='false']")
                                                ? new Icon(SupportedIcons.Check)
                                                : new Icon(SupportedIcons.Cross)
                                            : new Icon(SupportedIcons.TriangleExclamation)),
                                    ]
                                )
                            ),
                        ],
                        footer: navigator.EvaluateCondition("f:text")
                            ?
                            [
                                new Condition(
                                    "f:text",
                                    new NarrativeCollapser()
                                ),
                            ]
                            : null,
                        iconPrefix: [new NarrativeModal()]
                    ),
                ]
            );

        return widget.Render(navigator, renderer, context);
    }
}

public class OptionalGroupCharacteristicCard(string path, LocalizedLabel title) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var widget = new Condition(
            path,
            new Card(
                title,
                new Container([
                    new ConcatBuilder(
                        path,
                        _ =>
                        [
                            new NameValuePair([new ChangeContext("f:code", new CodeableConcept())], [
                                new OpenTypeElement(null), // CodeableConcept | boolean | Quantity | Range | Reference()
                                new Optional("f:period",
                                    new TextContainer(
                                        TextStyle.Muted,
                                        [
                                            new ConstantText("("),
                                            new LocalizedLabel("group.period"),
                                            new ConstantText(": "),
                                            new ShowPeriod(),
                                            new ConstantText(")"),
                                        ]
                                    )
                                ),
                            ]),
                        ]
                    ),
                ], optionalClass: "name-value-pair-wrapper w-fit-content")
            )
        );

        return widget.Render(navigator, renderer, context);
    }
}

public enum GroupInfrequentProperties
{
    Active,
    Code,
    Name,
    Quantity,
    ManagingEntity,
}