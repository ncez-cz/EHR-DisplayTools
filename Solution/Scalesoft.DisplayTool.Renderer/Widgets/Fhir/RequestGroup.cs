using System.Web;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.DocumentNavigation;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class RequestGroup : SequentialResourceBase<RequestGroup>, IResourceWidget
{
    public static string ResourceType => "RequestGroup";
    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            // ignore identifier
            // ignore instantiatesCanonical
            // ignore instantiatesUri
            // ignore basedOn
            // ignore replaces
            // ignore groupIdentifier
            new NameValuePair([new EhdsiDisplayLabel(LabelCodes.Status)],
                [new EnumLabel("f:status", "http://hl7.org/fhir/ValueSet/request-status")],
                style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row),
            new NameValuePair([new LocalizedLabel("request-group.intent")],
                [new EnumLabel("f:intent", "http://hl7.org/fhir/ValueSet/request-intent")],
                style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row),
            new Optional("f:priority", new NameValuePair([new LocalizedLabel("request-group.priority")],
                [new EnumLabel(".", "http://hl7.org/fhir/ValueSet/request-priority")],
                style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
            new Optional("f:code",
                new NameValuePair([new LocalizedLabel("request-group.code")], [new CodeableConcept()],
                    style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
            // ignore subject
            new Choose([
                new When("f:authoredOn",
                    new NameValuePair([new LocalizedLabel("request-group.authoredOn")],
                        [new ShowDateTime("f:authoredOn")], style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Row))
            ]),
            new Choose([
                new When(
                    "f:author",
                    new NameValuePair(
                        new LocalizedLabel("request-group.author"),
                        new AnyReferenceNamingWidget(),
                        style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row
                    )
                ),
            ]),
            new Condition("f:reasonCode",
                new NameValuePair([new LocalizedLabel("request-group.reason")],
                    [new CommaSeparatedBuilder("f:reasonCode", _ => new CodeableConcept())],
                    style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
            new Condition("f:reasonReference",
                new NameValuePair([new LocalizedLabel("request-group.reason")],
                    [new CommaSeparatedBuilder("f:reasonReference", _ => [new AnyReferenceNamingWidget()])],
                    style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
        ];
        widgetTree.AddRange([
            // ignore note
            new Condition("f:action", new Container([
                new Card(new LocalizedLabel("request-group.action"), new Container([
                    new ConcatBuilder("f:action", _ =>
                    [
                        new Container([
                            new ActionBuilder(navigator),
                        ], ContainerType.Div, "my-2")
                    ]),
                ])),
            ], ContainerType.Div, "my-2")),
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
            new Choose([
                new When("f:text",
                    new NarrativeCollapser()
                ),
            ]),
        ]);

        var requestGroupCollapser = new Collapser(
            [new LocalizedLabel("request-group")],
            widgetTree,
            iconPrefix: [new NarrativeModal()]
        );

        return requestGroupCollapser.Render(navigator, renderer, context);
    }

    private class ActionBuilder : Widget
    {
        private XmlDocumentNavigator m_requestGroupNav;

        public ActionBuilder(XmlDocumentNavigator requestGroupNav)
        {
            m_requestGroupNav = requestGroupNav;
        }

        public override Task<RenderResult> Render(
            XmlDocumentNavigator actionNav,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            Widget[] widgetTree =
            [
                new Card(null, new Container([
                    new Condition("@id",
                        new NameValuePair([new LocalizedLabel("request-group.prefix.id")], [new Text("@id")],
                            style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
                    new Optional("f:prefix",
                        new NameValuePair([new LocalizedLabel("request-group.action.prefix")], [new Text("@value")],
                            style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
                    new Optional("f:title",
                        new NameValuePair([new LocalizedLabel("request-group.action.title")], [new Text("@value")],
                            style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
                    new Optional("f:description",
                        new NameValuePair([new LocalizedLabel("request-group.action.description")],
                            [new Text("@value")], style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Row)),
                    new Optional("f:textEquivalent",
                        new NameValuePair([new LocalizedLabel("request-group.action.textEquivalent")],
                            [new Text("@value")], style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Row)),
                    new Optional("f:priority", new NameValuePair([new LocalizedLabel("request-group.action.priority")],
                        [new EnumLabel(".", "http://hl7.org/fhir/ValueSet/request-priority")],
                        style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
                    new Condition("f:code", new NameValuePair([new LocalizedLabel("request-group.action.code")],
                        [new ItemListBuilder("f:code", ItemListType.Unordered, _ => [new CodeableConcept()])],
                        style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
                    // ignore documentation
                    new Optional("f:condition", new NameValuePair(
                        [new LocalizedLabel("request-group.action.condition")],
                        [
                            new ItemListBuilder(".", ItemListType.Unordered, _ =>
                            [
                                new Card(null, new Container([
                                    new NameValuePair([new LocalizedLabel("request-group.action.condition.kind")],
                                    [
                                        new EnumLabel("f:kind", "http://hl7.org/fhir/ValueSet/action-condition-kind")
                                    ], style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row),
                                    new Optional("f:expression", new AdditionalInfoWidget(new Container([
                                        new Optional("f:description",
                                            new NameValuePair(
                                                [
                                                    new LocalizedLabel(
                                                        "request-group.action.condition.expression.description")
                                                ], [new Text("@value")],
                                                style: NameValuePair.NameValuePairStyle.Primary,
                                                direction: FlexDirection.Row)),
                                        new Optional("f:expression",
                                            new NameValuePair(
                                                [
                                                    new LocalizedLabel(
                                                        "request-group.action.condition.expression.expression")
                                                ], [new Text("@value")],
                                                style: NameValuePair.NameValuePairStyle.Primary,
                                                direction: FlexDirection.Row)),
                                        new Optional("f:reference",
                                            new NameValuePair(
                                                [
                                                    new LocalizedLabel(
                                                        "request-group.action.condition.expression.reference")
                                                ], [new Text("@value")],
                                                style: NameValuePair.NameValuePairStyle.Primary,
                                                direction: FlexDirection.Row)),
                                    ]))),
                                ])),
                            ]),
                        ], style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row
                    )),
                    new Condition("f:relatedAction", new NameValuePair(
                        [new LocalizedLabel("request-group.action.relatedAction")],
                        [
                            new ItemListBuilder("f:relatedAction", ItemListType.Unordered, (_, nav) =>
                            {
                                var actionId = nav.EvaluateString("f:actionId/@value");
                                XmlDocumentNavigator? referencedAction = null;
                                var actionUrl = string.Empty;
                                if (!string.IsNullOrEmpty(actionId))
                                {
                                    referencedAction =
                                        m_requestGroupNav.SelectSingleNode($"//f:action[@id = '{actionId}']");
                                }

                                if (referencedAction != null)
                                {
                                    actionUrl = "#" + GenerateActionUrl(referencedAction);
                                }

                                return
                                [
                                    new Card(null, new Container([
                                        new NameValuePair([new LocalizedLabel("request-group.action")],
                                            [
                                                new Link(
                                                    new Text("f:actionId/@value"),
                                                    actionUrl),
                                            ], style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row),
                                        new NameValuePair(
                                            [new LocalizedLabel("request-group.action.relatedAction.relationSHip")],
                                            [
                                                new EnumLabel("f:relationship",
                                                    "http://hl7.org/fhir/ValueSet/action-relationship-type")
                                            ], style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row),
                                        new Condition("f:offsetDuration or f:offsetRange", new NameValuePair(
                                            [new LocalizedLabel("request-group.action.relatedAction.offset")],
                                            [new Chronometry("offset")],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Row)),
                                    ])),
                                ];
                            }),
                        ], style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
                    new Condition(
                        "f:timingDateTime or f:timingAge or f:timingPeriod or f:timingDuration or f:timingRange or f:timingTiming",
                        new NameValuePair([new LocalizedLabel("request-group.action.timing")],
                            [new Chronometry("timing")], style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Row)),
                    new Condition("f:participant", new NameValuePair(
                        [new LocalizedLabel("request-group.action.participant")],
                        [
                            new ItemListBuilder("f:participant", ItemListType.Unordered,
                                _ => [new AnyReferenceNamingWidget()])
                        ], style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
                    // ignore type
                    // ignore groupingBehavior - display everything visually
                    new Optional("f:selectionBehavior", new NameValuePair(
                        [new LocalizedLabel("request-group.action.selectionBehavior")],
                        [new EnumLabel(".", "http://hl7.org/fhir/ValueSet/action-selection-behavior")],
                        style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
                    new Optional("f:requiredBehavior", new NameValuePair(
                        [new LocalizedLabel("request-group.action.requiredBehavior")],
                        [new EnumLabel(".", "http://hl7.org/fhir/ValueSet/action-required-behavior")],
                        style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Row)),
                    // ignore precheckBehavior - the most used action in group should be pre-checked
                    // ignore cardinalityBehavior
                    new Optional("f:resource", new NameValuePair(new LocalizedLabel("request-group.action.resource"),
                        new AnyReferenceNamingWidget(), style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Row)),
                    new Condition("f:action", new Card(new LocalizedLabel("request-group.action.action"), new Container(
                    [
                        new ConcatBuilder("f:action", _ =>
                        [
                            new Container([
                                new ActionBuilder(m_requestGroupNav),
                            ], ContainerType.Div, "my-2"),
                        ]),
                    ]))),
                ]), idSource: GenerateActionUrl(actionNav)),
            ];

            return widgetTree.RenderConcatenatedResult(actionNav, renderer, context);
        }
    }

    private static string GenerateActionUrl(XmlDocumentNavigator? action)
    {
        return action?.Node == null ? string.Empty : HttpUtility.UrlEncode(action.Node.UniqueId());
    }
}