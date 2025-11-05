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

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        Widget[] widgetTree =
        [
            // ignore identifier
            new Condition("f:statusHistory", new NameValuePair([new ConstantText("Historie stavu")], [
                new ItemListBuilder("f:statusHistory", ItemListType.Unordered, _ =>
                [
                    new EnumLabel("f:status", "http://hl7.org/fhir/ValueSet/episode-of-care-status"),
                    new ConstantText(" ("),
                    new ShowPeriod("f:period"),
                    new ConstantText(")"),
                ])
            ])),
            new Container([
                new Condition("f:type", new NameValuePair([new ConstantText("Typ")], [
                    new CommaSeparatedBuilder("f:type", _ =>
                    [
                        new CodeableConcept(),
                    ])
                ])),
                new Optional("f:period", new NameValuePair([new ConstantText("Období")], [new ShowPeriod()])),
            ], optionalClass: "name-value-pair-wrapper w-max-content"),
            new Condition("f:diagnosis", new Card(new ConstantText("Diagnózy"), new Container([
                new ListBuilder("f:diagnosis", FlexDirection.Column, _ =>
                [
                    new Container([
                        new Optional("f:role", new NameValuePair([new ConstantText("Role")], [new CodeableConcept()])),
                        new Optional("f:rank", new NameValuePair([new ConstantText("Pořadí")], [new Text("@value")])),
                    ], optionalClass: "name-value-pair-wrapper w-max-content"),
                    new Collapser([new ConstantText("Potíž / Událost")], [],
                    [
                        ShowSingleReference.WithDefaultDisplayHandler(
                            nav =>
                            [
                                new Container([new Conditions([nav], new ConstantText("Problém"))], idSource: nav)
                            ],
                            "f:condition")
                    ], true),
                ], separator: new LineBreak(), flexContainerClasses: ""),
            ]))),
            // ignore patient
            new Optional("f:managingOrganization",
                new Collapser([new ConstantText("Organizace - správce")], [],
                [
                    ShowSingleReference.WithDefaultDisplayHandler(nav =>
                        [new AnyResource(nav, displayResourceType: false)])
                ], true)),
            new Condition("f:referralRequest",
                new Collapser([new ConstantText("Zdrojové žádosti")], [],
                    [new ShowMultiReference("f:referralRequest", displayResourceType: false)],
                    true)),
            new Condition("f:careManager",
                new Collapser([new ConstantText("Správce péče")], [],
                    [
                        ShowSingleReference.WithDefaultDisplayHandler(
                            nav => [new AnyResource(nav, displayResourceType: false)], "f:careManager")
                    ],
                    true)),
            new Condition("f:team",
                new Collapser([new ConstantText("Pečovatelský tým")], [],
                    [new ShowMultiReference("f:team", displayResourceType: false)])),
            new Condition("f:account",
                new Collapser([new ConstantText("Účet")], [],
                    [new ShowMultiReference("f:account", displayResourceType: false)])),
        ];

        var widgetCollapser = new Collapser([
            new Row([
                new ConstantText("Epizoda péče"),
                new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/episode-of-care-status",
                    new DisplayLabel(LabelCodes.Status))
            ]),
        ], [], widgetTree, iconPrefix: [new NarrativeModal()]);

        return widgetCollapser.Render(navigator, renderer, context);
    }
}