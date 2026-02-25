using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class EpisodeOfCare : SequentialResourceBase<EpisodeOfCare>, IResourceWidget
{
    public static string ResourceType => "EpisodeOfCare";

    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        Widget[] widgetTree =
        [
            // ignore identifier
            new Condition("f:statusHistory", new NameValuePair([new LocalizedLabel("episode-of-care.statusHistory")], [
                new ItemListBuilder("f:statusHistory", ItemListType.Unordered, _ =>
                [
                    new EnumLabel("f:status", "http://hl7.org/fhir/ValueSet/episode-of-care-status"),
                    new ConstantText(" ("),
                    new ShowPeriod("f:period"),
                    new ConstantText(")"),
                ]),
            ])),
            new Container([
                new Condition("f:type", new NameValuePair([new LocalizedLabel("episode-of-care.type")], [
                    new CommaSeparatedBuilder("f:type", _ =>
                    [
                        new CodeableConcept(),
                    ]),
                ])),
                new Optional("f:period",
                    new NameValuePair([new LocalizedLabel("episode-of-care.period")], [new ShowPeriod()])),
            ], optionalClass: "name-value-pair-wrapper w-fit-content"),
            new Condition("f:diagnosis", new Card(new LocalizedLabel("episode-of-care.diagnosis-plural"), new Container(
            [
                new ListBuilder("f:diagnosis", FlexDirection.Column, _ =>
                [
                    new Container([
                        new Optional("f:role",
                            new NameValuePair([new LocalizedLabel("episode-of-care.diagnosis.role")],
                                [new CodeableConcept()])),
                        new Optional("f:rank",
                            new NameValuePair([new LocalizedLabel("episode-of-care.diagnosis.rank")],
                                [new Text("@value")])),
                    ], optionalClass: "name-value-pair-wrapper w-fit-content"),
                    new Collapser([new LocalizedLabel("episode-of-care.diagnosis.condition")],
                    [
                        ShowSingleReference.WithDefaultDisplayHandler(
                            nav =>
                            [
                                new Container([new Conditions([nav], new LocalizedLabel("condition"))], idSource: nav)
                            ],
                            "f:condition"),
                    ], true),
                ], separator: new LineBreak(), flexContainerClasses: ""), // Overrides the default class
            ]))),
            // ignore patient
            new Optional("f:managingOrganization",
                new Collapser([new LocalizedLabel("episode-of-care.managingOrganization")],
                [
                    ShowSingleReference.WithDefaultDisplayHandler(nav =>
                        [new AnyResource(nav, displayResourceType: false)])
                ], true)),
            new Condition("f:referralRequest",
                new Collapser([new LocalizedLabel("episode-of-care.referralRequest")],
                    [new ShowMultiReference("f:referralRequest", displayResourceType: false)],
                    true)),
            new Condition("f:careManager",
                new Collapser([new LocalizedLabel("episode-of-care.careManager")],
                    [
                        ShowSingleReference.WithDefaultDisplayHandler(
                            nav => [new AnyResource(nav, displayResourceType: false)], "f:careManager")
                    ],
                    true)),
            new Condition("f:team",
                new Collapser([new LocalizedLabel("episode-of-care.team")],
                    [new ShowMultiReference("f:team", displayResourceType: false)])),
            new Condition("f:account",
                new Collapser([new LocalizedLabel("episode-of-care.account")],
                    [new ShowMultiReference("f:account", displayResourceType: false)])),
        ];

        var widgetCollapser = new Collapser([
            new Row([
                new LocalizedLabel("episode-of-care"),
                new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/episode-of-care-status",
                    new EhdsiDisplayLabel(LabelCodes.Status))
            ]),
        ], widgetTree, iconPrefix: [new NarrativeModal()]);

        return widgetCollapser.Render(navigator, renderer, context);
    }
}