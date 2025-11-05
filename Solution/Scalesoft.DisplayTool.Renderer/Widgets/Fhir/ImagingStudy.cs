using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ImagingStudy : SequentialResourceBase<ImagingStudy>, IResourceWidget
{
    public static string ResourceType => "ImagingStudy";
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        Widget[] widgetTree =
        [
            new Card(
                new Concat([
                    new Row([
                        new ConstantText("Obrazová studie"),
                        new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/imagingstudy-status",
                            new DisplayLabel(LabelCodes.Status))
                    ], flexContainerClasses: "gap-1 align-items-center"),
                    new NarrativeModal()
                ]),
                new Container([
                    new Container([
                        new Condition("f:modality", new NameValuePair([new ConstantText("Modality")], [
                            new ItemListBuilder("f:modality", ItemListType.Unordered, _ => [new Coding()])
                        ])),
                        new Optional("f:started",
                            new NameValuePair([new ConstantText("Zahájeno")], [new ShowDateTime()])),
                        // ignore subject
                        // ignore identifier
                        new Condition("f:referrer",
                            new HideableDetails(new NameValuePair([new ConstantText("Odkazující lékař"),],
                                [new AnyReferenceNamingWidget("f:referrer")]))),
                        new Condition("f:interpreter", new HideableDetails(
                            new NameValuePair(
                                [new ConstantText("Interpretující lékaři")],
                                [
                                    new ItemListBuilder("f:interpreter", ItemListType.Unordered,
                                        _ => [new AnyReferenceNamingWidget()])
                                ]
                            )
                        )),
                        // ignore endpoint - little value to the end user - reader
                        new Optional("f:numberOfSeries",
                            new NameValuePair([new ConstantText("Počet řad")], [new Text("@value")])),
                        new Optional("f:numberOfInstances",
                            new NameValuePair([new ConstantText("Počet instancí")], [new Text("@value")])),
                        new Condition("f:location", new HideableDetails(new Collapser([new ConstantText("Lokace"),], [],
                        [
                            ShowSingleReference.WithDefaultDisplayHandler(
                                nav => [new AnyResource(nav, displayResourceType: false)],
                                "f:location")
                        ], true))),
                        new Condition("f:reasonCode",
                            new NameValuePair([new ConstantText("Důvody")],
                                [new CommaSeparatedBuilder("f:reasonCode", _ => new CodeableConcept())])),
                        new Condition("f:reasonReference", new HideableDetails(new Collapser([new ConstantText("Důvody"),],
                            [],
                            [
                                new ListBuilder("f:reasonReference", FlexDirection.Column,
                                    _ =>
                                    [
                                        ShowSingleReference.WithDefaultDisplayHandler(nav =>
                                            [new AnyResource(nav, displayResourceType: false)])
                                    ],
                                    separator: new LineBreak()),
                            ], true))),
                        //ignore note
                        new Condition("f:procedureCode",
                            new NameValuePair([new ConstantText("Procedury")],
                                [new CommaSeparatedBuilder("f:procedureCode", _ => new CodeableConcept())])),
                    ], optionalClass: "name-value-pair-wrapper w-max-content"),
                    new Condition("f:procedureReference", new HideableDetails(new Collapser(
                        [new ConstantText("Procedura"),], [],
                        [
                            ShowSingleReference.WithDefaultDisplayHandler(
                                nav => [new AnyResource(nav, displayResourceType: false)],
                                "f:procedureReference")
                        ], true))),
                    new Condition("f:basedOn", new Collapser([new ConstantText("Založeno na"),], [],
                        [new ShowMultiReference("f:basedOn", displayResourceType: false)], true)),
                    new Condition("f:series", new ThematicBreak(),
                        new TextContainer(TextStyle.Bold, new ConstantText("Řada")),
                        new ListBuilder(
                            "f:series", FlexDirection.Column, (_, _) =>
                            [
                                new Collapser(
                                    [
                                        new NameValuePair(new ConstantText("Identifikátor instance řady DICOM"),
                                            new Text("f:uid/@value"))
                                    ],
                                    [], [
                                        new Container([
                                            new Optional("f:number",
                                            new NameValuePair([new ConstantText("Identifikátor")],
                                                [new Text("@value")])),
                                        new HideableDetails(new NameValuePair([new ConstantText("Modalita")], [
                                            new ChangeContext("f:modality", new Coding()),
                                        ])),
                                        new Optional("f:description",
                                            new NameValuePair([new ConstantText("Popis")], [new Text("@value")])),
                                        new Optional("f:numberOfInstances",
                                            new HideableDetails(new NameValuePair([new ConstantText("Počet instancí")],
                                                [new Text("@value")]))),
                                        // ignore endpoint
                                        new Optional("f:bodySite",
                                            new NameValuePair([new DisplayLabel(LabelCodes.BodySite)], [new Coding()])),
                                        new Optional("f:laterality",
                                            new HideableDetails(new NameValuePair([new ConstantText("Lateralita")],
                                                [new Coding()]))),
                                        new Condition("f:specimen", new HideableDetails(new ListBuilder("f:specimen",
                                            FlexDirection.Column,
                                            _ =>
                                            [
                                                new Card(new ConstantText("Vzorek"), new Container([
                                                    ShowSingleReference.WithDefaultDisplayHandler(nav =>
                                                        [new AnyResource(nav, displayResourceType: false)]),
                                                ])),
                                            ]))),
                                        new Optional("f:started",
                                            new HideableDetails(new NameValuePair([new ConstantText("Zahájeno")],
                                                [new ShowDateTime()]))),
                                        ], optionalClass: "name-value-pair-wrapper w-max-content"),
                                        new Condition("f:performer", new Collapser([new ConstantText("Provedl/a")], [],
                                        [
                                            new ListBuilder("f:performer", FlexDirection.Column,
                                                _ =>
                                                [
                                                    new Optional("f:function",
                                                        new NameValuePair([new ConstantText("Funkce")],
                                                            [new CodeableConcept()])),
                                                    new AnyReferenceNamingWidget("f:actor"),
                                                ],
                                                separator: new ThematicBreak(), flexContainerClasses: "gap-0"),
                                        ])),
                                        new Condition("f:instance", new HideableDetails(new Collapser(
                                            [new ConstantText("Instance")], [], [
                                                new ListBuilder("f:instance", FlexDirection.Column,
                                                    _ =>
                                                    [
                                                        new Container([
                                                            new ChangeContext("f:uid",
                                                                new NameValuePair([new ConstantText("Identifikátor")],
                                                                    [new Text("@value")])),
                                                            new ChangeContext("f:sopClass",
                                                                new NameValuePair([new ConstantText("Typ")],
                                                                    [new Coding()])),
                                                            new Optional("f:number",
                                                                new NameValuePair([new ConstantText("Identifikátor")],
                                                                    [new Text("@value")])),
                                                            new Optional("f:title",
                                                                new NameValuePair([new ConstantText("Popis")],
                                                                    [new Text("@value")])),
                                                        ], optionalClass: "name-value-pair-wrapper w-max-content"),
                                                    ],
                                                    separator: new ThematicBreak(), flexContainerClasses: "gap-0"),
                                            ]))),
                                    ]),
                            ])),
                ]), footer: navigator.EvaluateCondition("f:text or f:encounter")
                    ? new Concat([
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
                    ])
                    : null),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}