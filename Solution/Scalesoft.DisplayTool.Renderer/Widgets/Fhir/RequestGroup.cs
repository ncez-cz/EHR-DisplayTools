using System.Web;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.DocumentNavigation;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class RequestGroup : SequentialResourceBase<RequestGroup>, IResourceWidget
{
    public static string ResourceType => "RequestGroup";
    
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
            new NameValuePair([new DisplayLabel(LabelCodes.Status)],
                [new EnumLabel("f:status", "http://hl7.org/fhir/ValueSet/request-status")]),
            new NameValuePair([new ConstantText("Záměr")],
                [new EnumLabel("f:intent", "http://hl7.org/fhir/ValueSet/request-intent")]),
            new Optional("f:priority", new NameValuePair([new ConstantText("Priorita")],
                [new EnumLabel(".", "http://hl7.org/fhir/ValueSet/request-priority")])),
            new Optional("f:code",
                new NameValuePair([new ConstantText("Předmět žádosti/objednávky")], [new CodeableConcept()])),
            // ignore subject
            new Choose([
                new When("f:authoredOn",
                    new NameValuePair([new ConstantText("Datum vytvoření")], [new ShowDateTime("f:authoredOn")]))
            ]),
            new Choose([
                new When(
                    "f:author",
                    new NameValuePair(
                        new ConstantText("Autor"),
                        new AnyReferenceNamingWidget()
                    )
                )
            ]),
            new Condition("f:reasonCode", new NameValuePair([new ConstantText("Důvod")],
                [new ItemListBuilder("f:reasonCode", ItemListType.Unordered, _ => [new CodeableConcept()])])),
        ];
        var labelCollapser =
            ReferenceHandler.BuildCollapserByMultireference(RequestResourceUtils.ReasonBuilder, navigator, context,
                "f:reasonReference",
                "Důvod");
        widgetTree.AddRange(labelCollapser);
        widgetTree.AddRange([
            // ignore note
            new Condition("f:action", new Container([
                new Card(new ConstantText("Akce"), new Container([
                    new ConcatBuilder("f:action", _ =>
                    [
                        new Container([
                            new ActionBuilder(navigator),
                        ], ContainerType.Div, "my-2")
                    ]),
                ]))
            ], ContainerType.Div, "my-2")),
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
            new Choose([
                new When("f:text",
                    new NarrativeCollapser()
                )
            ]),
        ]);

        var requestGroupCollapser = new Collapser(
            [new ConstantText("Skupina požadavků")],
            [],
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
                    new Condition("@id", new NameValuePair([new ConstantText("Identifikátor")], [new Text("@id")])),
                    new Optional("f:prefix", new NameValuePair([new ConstantText("Prefix")], [new Text("@value")])),
                    new Optional("f:title", new NameValuePair([new ConstantText("Nadpis")], [new Text("@value")])),
                    new Optional("f:description", new NameValuePair([new ConstantText("Popis")], [new Text("@value")])),
                    new Optional("f:textEquivalent",
                        new NameValuePair([new ConstantText("Textový ekvivalent")], [new Text("@value")])),
                    new Optional("f:priority", new NameValuePair([new ConstantText("Priorita")],
                        [new EnumLabel(".", "http://hl7.org/fhir/ValueSet/request-priority")])),
                    new Condition("f:code", new NameValuePair([new ConstantText("Předmět žádosti")],
                        [new ItemListBuilder("f:code", ItemListType.Unordered, _ => [new CodeableConcept()])])),
                    // ignore documentation
                    new Optional("f:condition", new NameValuePair(
                        [new ConstantText("Podmínka/Aplikovatelnost")],
                        [
                            new ItemListBuilder(".", ItemListType.Unordered, _ =>
                            [
                                new Card(null, new Container([
                                    new NameValuePair([new ConstantText("Typ")],
                                    [
                                        new EnumLabel("f:kind", "http://hl7.org/fhir/ValueSet/action-condition-kind")
                                    ]),
                                    new Optional("f:expression", new AdditionalInfoWidget(new Container([
                                        new Optional("f:description",
                                            new NameValuePair([new ConstantText("Popis")], [new Text("@value")])),
                                        new Optional("f:expression",
                                            new NameValuePair([new ConstantText("Výraz")], [new Text("@value")])),
                                        new Optional("f:reference",
                                            new NameValuePair([new ConstantText("Odkaz")], [new Text("@value")])),
                                    ])))
                                ]))
                            ])
                        ]
                    )),
                    new Condition("f:relatedAction", new NameValuePair([new ConstantText("Související akce")],
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
                                    new NameValuePair([new ConstantText("Akce")],
                                    [
                                        new Link(
                                            new Text("f:actionId/@value"),
                                            new ConstantText(actionUrl)),
                                    ]),
                                    new NameValuePair([new ConstantText("Typ vztahu")],
                                    [
                                        new EnumLabel("f:relationship",
                                            "http://hl7.org/fhir/ValueSet/action-relationship-type")
                                    ]),
                                    new Condition("f:offsetDuration or f:offsetRange", new NameValuePair(
                                        [new ConstantText("Časový posun vztahu")],
                                        [new Chronometry("offset")])),
                                ])),
                            ];
                        })
                    ])),
                    new Condition(
                        "f:timingDateTime or f:timingAge or f:timingPeriod or f:timingDuration or f:timingRange or f:timingTiming",
                        new NameValuePair([new ConstantText("Čas")],
                            [new Chronometry("timing")])),
                    new Condition("f:participant", new NameValuePair([new ConstantText("Vykonavatel")],
                    [
                        new ItemListBuilder("f:participant", ItemListType.Unordered,
                            _ => [new AnyReferenceNamingWidget()] )
                    ])),
                    // ignore type
                    // ignore groupingBehavior - display everything visually
                    new Optional("f:selectionBehavior", new NameValuePair([new ConstantText("Vybrat")],
                        [new EnumLabel(".", "http://hl7.org/fhir/ValueSet/action-selection-behavior")])),
                    new Optional("f:requiredBehavior", new NameValuePair([new ConstantText("Povinnost")],
                        [new EnumLabel(".", "http://hl7.org/fhir/ValueSet/action-required-behavior")])),
                    // ignore precheckBehavior - the most used action in group should be pre-checked
                    // ignore cardinalityBehavior
                    new Optional("f:resource", new Card(new ConstantText("Akce"), new Container([
                        new ShowSingleReference(x =>
                        {
                            if (x.ResourceReferencePresent)
                            {
                                return [new Card(null, new AnyResource(x.Navigator, displayResourceType: false))];
                            }

                            return [new Card(null, new ConstantText(x.ReferenceDisplay))];
                        }),
                    ]))),
                    new Condition("f:action", new Card(new ConstantText("Akce"), new Container([
                        new ConcatBuilder("f:action", _ =>
                        [
                            new Container([
                                new ActionBuilder(m_requestGroupNav),
                            ], ContainerType.Div, "my-2")
                        ]),
                    ]))),
                ]), idSource: GenerateActionUrl(actionNav))
            ];

            return widgetTree.RenderConcatenatedResult(actionNav, renderer, context);
        }
    }

    private static string GenerateActionUrl(XmlDocumentNavigator? action)
    {
        return action?.Node == null ? string.Empty : HttpUtility.UrlEncode(action.Node.UniqueId());
    }
}