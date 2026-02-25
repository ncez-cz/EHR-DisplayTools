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

    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<ListInfrequentProperties>(navigator);

        var basicBadge = new PlainBadge(new LocalizedLabel("general.basic-information"));
        var basicInfo = new Container([
            new NameValuePair(
                new LocalizedLabel("list.mode"),
                new EnumLabel("f:mode", "http://hl7.org/fhir/ValueSet/list-mode")
            ),
            infrequentProperties.Optional(ListInfrequentProperties.Title,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.Name),
                    new Text("@value")
                )
            ),
            infrequentProperties.Optional(ListInfrequentProperties.Code,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.Code),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Optional(ListInfrequentProperties.Subject,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("list.subject"),
                    }
                )
            ),
            infrequentProperties.Optional(ListInfrequentProperties.Encounter,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("list.encounter"),
                    }
                )
            ),
            infrequentProperties.Optional(ListInfrequentProperties.Date,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.Date),
                    new ShowDateTime()
                )
            ),
            infrequentProperties.Optional(ListInfrequentProperties.Source,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new EhdsiDisplayLabel(LabelCodes.Author),
                    }
                )
            ),
            infrequentProperties.Optional(ListInfrequentProperties.OrderedBy,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("list.orderedBy"),
                    }
                )
            ),
        ]);

        var entriesBadge = new PlainBadge(new LocalizedLabel("list.entry-plural"));

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
                                    new LocalizedLabel("list.entry.deleted"),
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
                                    new LocalizedLabel("list.entry.deleted")
                                ], icon: new Icon(SupportedIcons.Trash))
                            )
                        ]);

                        var card =
                            new Card(title,
                                new Container([
                                    new Optional("f:flag",
                                        new NameValuePair(
                                            new LocalizedLabel("list.entry.flag"),
                                            new CodeableConcept()
                                        )
                                    ),
                                    new Optional("f:date",
                                        new NameValuePair(
                                            new EhdsiDisplayLabel(LabelCodes.Date),
                                            new ShowDateTime()
                                        )
                                    ),
                                ]), optionalClass: deleted ? "deleted-list-item" : null
                            );

                        return [card];
                    }, flexContainerClasses: "gap-2"),
                new If(_ => simple.Count > 0,
                    new If(_ => complex.Count > 0,
                            new Card(new LocalizedLabel("list.other-items"), simpleWidgets,
                                optionalClass: "mt-2"))
                        .Else(new Container([simpleWidgets], ContainerType.Div, "mt-2"))
                ),
            ], FlexDirection.Row),
            new Optional("f:emptyReason",
                new NameValuePair(
                    new LocalizedLabel("list.emptyReason"),
                    new CodeableConcept()
                )
            ),
        ]);


        var complete =
            new Collapser(
                [
                    new Container([
                        new LocalizedLabel("list"),
                        new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/list-status",
                            new EhdsiDisplayLabel(LabelCodes.Status)),
                    ], ContainerType.Div, "d-flex align-items-center gap-1"),
                ], [
                    basicBadge,
                    basicInfo,
                    new Condition("f:entry or f:emptyReason",
                        new ThematicBreak(),
                        entriesBadge,
                        entriesInfo
                    ),
                    new Condition("f:note",
                        new ThematicBreak(),
                        new PlainBadge(new LocalizedLabel("list.note")),
                        new ListBuilder("f:note", FlexDirection.Column, _ => [new ShowAnnotationCompact()],
                            flexContainerClasses: "") // Overrides the default class
                    ),
                ], footer: navigator.EvaluateCondition("f:text") || navigator.EvaluateCondition("f:encounter")
                    ?
                    [
                        new Optional("f:encounter",
                            new ShowMultiReference(".",
                                (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                                x =>
                                [
                                    new Collapser([new LocalizedLabel("node-names.Encounter")], x.ToList(),
                                        isCollapsed: true),
                                ]
                            )
                        ),
                        new Condition("f:text",
                            new NarrativeCollapser()
                        ),
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()]
            );


        return complete.Render(navigator, renderer, context);
    }

    public enum ListInfrequentProperties
    {
        Subject,
        Encounter,
        Date,
        Source,
        OrderedBy,
        Title,
        Code,
    }
}