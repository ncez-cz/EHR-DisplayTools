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
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var widget =
            new Concat(
                [
                    new Collapser(
                        [new ConstantText("Skupina")],
                        [],
                        [
                            new PlainBadge(new ConstantText("Základní informace")),
                            new Container(
                                [
                                    new Optional(
                                        "f:active",
                                        new TextContainer(TextStyle.Bold,
                                            new ShowBoolean(new ConstantText("Neaktivní"), new ConstantText("Aktivní"))
                                        )
                                    ),
                                    new NameValuePair(
                                        new ConstantText("Typ subjektů"),
                                        new EnumLabel("f:type", "http://hl7.org/fhir/ValueSet/group-type")
                                    ),
                                    new NameValuePair(
                                        new ConstantText("Povaha skupiny"),
                                        new ShowBoolean(
                                            new ConstantText("Skupina pouze popisuje odpovídající, vyžadovaný typ"),
                                            new ConstantText("Skupina popisuje konkrétní, skutečné subjekty"),
                                            "f:actual"
                                        )
                                    ),
                                    new Optional(
                                        "f:code",
                                        new NameValuePair(
                                            new ConstantText("Druh subjektů"),
                                            new CodeableConcept()
                                        )
                                    ),
                                    new Optional(
                                        "f:name",
                                        new NameValuePair(
                                            new DisplayLabel(LabelCodes.Name),
                                            new Text("@value")
                                        )
                                    ),
                                    new Optional(
                                        "f:quantity",
                                        new NameValuePair(
                                            new ConstantText("Počet subjektů"),
                                            new Text("@value")
                                        )
                                    ),
                                    new Optional(
                                        "f:managingEntity",
                                        new NameValuePair(
                                            new ConstantText("Správce skupiny"),
                                            new AnyReferenceNamingWidget()
                                        )
                                    ),
                                ]
                            ),
                            new Condition(
                                "f:characteristic",
                                new ThematicBreak(),
                                new PlainBadge(new ConstantText("Charakteristika skupiny")),
                                new Container(
                                    [
                                        new Column(
                                            [
                                                new OptionalGroupCharacteristicCard(
                                                    "f:characteristic[f:exclude/@value='false']",
                                                    "Požadované vlastnosti"
                                                ),
                                                new OptionalGroupCharacteristicCard(
                                                    "f:characteristic[f:exclude/@value='true']",
                                                    "Vylučující vlastnosti"
                                                ),
                                            ]
                                        ),
                                    ]
                                )
                            ),
                            new Condition(
                                "f:member",
                                new ThematicBreak(),
                                new PlainBadge(new ConstantText("Členové skupiny")),
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
                                                    new ConstantText("Období členství"),
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
                                            new ShowBoolean("Subjekt je součástí skupiny.",
                                                "Subjekt již není součástí skupiny.", "f:inactive")
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
                                new Optional(
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

public class OptionalGroupCharacteristicCard(string path, string title) : Widget
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
                new ConstantText(title),
                new ConcatBuilder(
                    path,
                    _ =>
                    [
                        new NameValuePair([new ChangeContext("f:code", new CodeableConcept())], [
                            new OpenTypeElement(null), // CodeableConcept | boolean | Quantity | Range | Reference()
                            new Optional(
                                "f:period",
                                new TextContainer(
                                    TextStyle.Muted,
                                    [
                                        new ConstantText(" (V období: "),
                                        new ShowPeriod(),
                                        new ConstantText(")"),
                                    ]
                                )
                            )
                        ])
                    ]
                )
            )
        );

        return widget.Render(navigator, renderer, context);
    }
}