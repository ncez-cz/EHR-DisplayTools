using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class List : ColumnResourceBase<List>, IResourceWidget
{
    public static string ResourceType => "List";

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var basicBadge = new PlainBadge(new ConstantText("Základní informace"));
        var basicInfo = new Container([
            new NameValuePair(
                new ConstantText("Mód"),
                new EnumLabel("f:mode", "http://hl7.org/fhir/ValueSet/list-mode")
            ),
            new Optional("f:title",
                new NameValuePair(
                    new DisplayLabel(LabelCodes.Name),
                    new Text("@value")
                )
            ),
            new Optional("f:code",
                new NameValuePair(
                    new DisplayLabel(LabelCodes.Code),
                    new CodeableConcept()
                )
            ),
            new Optional("f:subject",
                new NameValuePair(
                    new ConstantText("Předmět"),
                    new AnyReferenceNamingWidget()
                )
            ),
            new Optional("f:encounter",
                new NameValuePair(
                    new ConstantText("Setkání"),
                    new AnyReferenceNamingWidget()
                )
            ),
            new Optional("f:date",
                new NameValuePair(
                    new DisplayLabel(LabelCodes.Date),
                    new ShowDateTime()
                )
            ),
            new Optional("f:source",
                new NameValuePair(
                    new DisplayLabel(LabelCodes.Author),
                    new AnyReferenceNamingWidget()
                )
            ),
            new Optional("f:orderedBy",
                new NameValuePair(
                    new ConstantText("Seřazeno podle"),
                    new AnyReferenceNamingWidget()
                )
            ),
        ]);

        var entriesBadge = new PlainBadge(new ConstantText("Položky seznamu"));

        var entries = navigator.SelectAllNodes("f:entry").ToList();
        var complex = entries.Where(x => x.EvaluateCondition("f:flag or f:date")).ToList();
        var simple = entries.Where(x => x.EvaluateCondition("not(f:flag or f:date)")).ToList();
        var simpleWidgets =
            new ItemList(ItemListType.Unordered,
                simple.Select(x =>
                        new ChangeContext(x,
                            new AnyReferenceNamingWidget("f:item"),
                            new Condition(
                                "f:deleted[@value='true']",
                                new ConstantText(" "),
                                new Tooltip([], [
                                    new ConstantText("Položka byla odstraněna"),
                                ], icon: new Icon(SupportedIcons.Trash))
                            )
                        )
                    )
                    .Cast<Widget>().ToList());


        var entriesInfo = new Container([
            new FlexList([
                new ListBuilder(complex,
                    FlexDirection.Row, (_, nav) =>
                    {
                        var deleted = nav.EvaluateCondition("f:deleted[@value='true']");

                        var title = new Concat([
                            new Optional("f:item",
                                new AnyReferenceNamingWidget()
                            ),
                            new If(_ => deleted,
                                new Tooltip([], [
                                    new ConstantText("Položka byla odstraněna")
                                ], icon: new Icon(SupportedIcons.Trash))
                            )
                        ]);

                        var card =
                            new Card(title,
                                new Container([
                                    new Optional("f:flag",
                                        new NameValuePair(
                                            new ConstantText("Značka"),
                                            new CodeableConcept()
                                        )
                                    ),
                                    new Optional("f:date",
                                        new NameValuePair(
                                            new DisplayLabel(LabelCodes.Date),
                                            new ShowDateTime()
                                        )
                                    )
                                ]), deleted ? "deleted-list-item" : null
                            );

                        return [card];
                    }, flexContainerClasses: "gap-2"),
                new If(_ => simple.Count > 0,
                    new If(_ => complex.Count > 0,
                            new Card(new ConstantText("Proste neco / ostatni ponozky"), simpleWidgets, "mt-2"))
                        .Else(new Container([simpleWidgets], ContainerType.Div, "mt-2"))
                ),
            ], FlexDirection.Row),
            new Optional("f:emptyReason",
                new NameValuePair(
                    new ConstantText("Důvod prázdného seznamu"),
                    new CodeableConcept()
                )
            ),
        ]);


        var complete =
            new Collapser(
                [
                    new Container([
                        new ConstantText("Seznam"),
                        new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/list-status",
                            new DisplayLabel(LabelCodes.Status))
                    ], ContainerType.Div, "d-flex align-items-center gap-1")
                ], [], [
                    basicBadge,
                    basicInfo,
                    new Condition("f:entry or f:emptyReason",
                        new ThematicBreak(),
                        entriesBadge,
                        entriesInfo
                    ),
                    new Condition("f:note",
                        new ThematicBreak(),
                        new PlainBadge(new ConstantText("Poznámky")),
                        new ListBuilder("f:note", FlexDirection.Column, _ => [new ShowAnnotationCompact()],
                            flexContainerClasses: "")
                    ),
                ], footer: navigator.EvaluateCondition("f:text") || navigator.EvaluateCondition("f:encounter")
                    ?
                    [
                        new Optional("f:encounter",
                            new ShowMultiReference(".",
                                (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                                x =>
                                [
                                    new Collapser([new ConstantText(Labels.Encounter)], [], x.ToList(),
                                        isCollapsed: true)
                                ]
                            )
                        ),
                        new Optional("f:text",
                            new NarrativeCollapser()
                        ),
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()]
            );


        return complete.Render(navigator, renderer, context);
    }
}