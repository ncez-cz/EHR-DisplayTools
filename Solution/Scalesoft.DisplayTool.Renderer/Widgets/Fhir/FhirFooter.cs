using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Person;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class FhirFooter : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        Widget authorResourceTitle = new DisplayLabel(LabelCodes.Author);

        if (context.DocumentType is DocumentType.ImagingOrder
            or DocumentType.LaboratoryOrder)
        {
            authorResourceTitle = new ConstantText("Žadatel");
        }

        var widget = new Container(
            content:
            [
                new ThematicBreak(),
                new Section(
                    select: ".",
                    requiredSectionMissingTitle: null,
                    titleAbbreviations: SectionTitleAbbreviations.AdditionalDocumentInformation,
                    content:
                    [
                        #region Author

                        new If(
                            _ => navigator.EvaluateCondition(
                                "/f:Bundle/f:entry/f:resource/f:Composition/f:author/f:reference"),
                            new ShowMultiReference("/f:Bundle/f:entry/f:resource/f:Composition/f:author", (items, _) =>
                                    items.Select(Widget (nav) => new Container([
                                        new PersonOrOrganization(
                                            nav,
                                            skipWhenInactive: true,
                                            collapserTitle: authorResourceTitle
                                        ),
                                    ], idSource: nav)).ToList(),
                                x =>
                                [
                                    new Collapser(
                                        [authorResourceTitle],
                                        [],
                                        x.ToList(),
                                        isCollapsed: true
                                    )
                                ])
                        ),

                        #endregion

                        #region Custodian

                        new If(
                            _ => navigator.EvaluateCondition("/f:Bundle/f:entry/f:resource/f:Composition/f:custodian"),
                            // multireference widget is used only for customising broken references builder, semantically the reference is x..1
                            new ShowMultiReference(
                                "/f:Bundle/f:entry/f:resource/f:Composition/f:custodian",
                                (items, _) => items.Select(Widget (x) => new Container([
                                    new PersonOrOrganization(x, skipWhenInactive: true,
                                        collapserTitle: new DisplayLabel(LabelCodes.Custodian)),
                                ], idSource: x)).ToList(),
                                x =>
                                [
                                    new Collapser(
                                        [new DisplayLabel(LabelCodes.Custodian)],
                                        [],
                                        x.ToList(),
                                        isCollapsed: true
                                    )
                                ]
                            )
                        ),

                        #endregion

                        #region Managing Organization

                        new If(_ => navigator.EvaluateCondition("f:managingOrganization"),
                            new ShowMultiReference(
                                "f:managingOrganization",
                                (items, _) => items.Select(Widget (x) => new Container([
                                    new PersonOrOrganization(x, skipWhenInactive: true,
                                        collapserTitle: new DisplayLabel(LabelCodes.RepresentedOrganization)),
                                ], idSource: x)).ToList(),
                                x =>
                                [
                                    new Collapser(
                                        [new DisplayLabel(LabelCodes.RepresentedOrganization)],
                                        [],
                                        x.ToList(),
                                        isCollapsed: true
                                    )
                                ]
                            )),

                        #endregion

                        #region Attester

                        new ConcatBuilder(
                            "/f:Bundle/f:entry/f:resource/f:Composition/f:attester[f:mode and (f:party/f:reference or f:time)]",
                            (_, _, x) =>
                            {
                                var attesterNarrative = ReferenceHandler.GetSingleNodeNavigatorFromReference(x,
                                    "f:party", "f:text");

                                var tree = new Container([
                                        new Collapser(
                                            [
                                                new ConstantText("Ověřitel pravosti dokumentu"),

                                                ShowSingleReference.WithDefaultDisplayHandler(
                                                    nav =>
                                                    [
                                                        new Optional("f:name",
                                                            new TextContainer(TextStyle.Bold,
                                                                new Choose([
                                                                        new When("@value",
                                                                            new Text("@value"))
                                                                    ],
                                                                    new HumanNameCompact(".")),
                                                                optionalClass: "black ms-4", idSource: nav
                                                            )
                                                        )
                                                    ],
                                                    "f:party"),
                                            ], [], [
                                                new HideableDetails(
                                                    new Row([
                                                        new NameValuePair([new ConstantText("Režim ověření")],
                                                        [
                                                            new EnumLabel("f:mode",
                                                                "http://hl7.org/fhir/ValueSet/composition-attestation-mode"),
                                                        ]),
                                                        new Choose([
                                                            new When("f:time",
                                                                new NameValuePair([new ConstantText("Datum ověření")],
                                                                    [new ShowDateTime("f:time")])
                                                            ),
                                                        ]),
                                                    ], flexContainerClasses: "justify-content-between mb-2")
                                                ),
                                                new Choose([
                                                    new When("f:party", new Container([
                                                        ShowSingleReference.WithDefaultDisplayHandler(
                                                            nav =>
                                                            [
                                                                new Container(
                                                                    [
                                                                        new PersonOrOrganization(nav,
                                                                            showNarrative: false),
                                                                    ],
                                                                    idSource: nav),
                                                            ],
                                                            "f:party"),
                                                    ])),
                                                ]),
                                            ],
                                            footer: attesterNarrative != null
                                                ?
                                                [
                                                    new NarrativeCollapser(attesterNarrative.GetFullPath()),
                                                ]
                                                : null,
                                            iconPrefix:
                                            [
                                                new If(_ => attesterNarrative != null,
                                                    new NarrativeModal(attesterNarrative?.GetFullPath()!)
                                                ),
                                            ], isCollapsed: true
                                        ),
                                    ],
                                    optionalClass: !x.EvaluateCondition("f:party/f:reference")
                                        ? "optional-detail"
                                        : string.Empty);


                                return [tree];
                            }
                        ),

                        #endregion

                        #region Encounter

                        new Optional(
                            "f:encounter",
                            // multireference widget is used only for customising broken references builder, semantically the reference is x..1
                            new ShowMultiReference(
                                ".",
                                (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                                x =>
                                [
                                    new Collapser(
                                        [new ConstantText(Labels.Encounter)],
                                        [],
                                        x.ToList(),
                                        isCollapsed: true
                                    )
                                ]
                            )
                        ),

                        #endregion

                        #region Presented form

                        new Condition(
                            "f:extension[@url='http://hl7.eu/fhir/StructureDefinition/presentedForm']",
                            new Collapser(
                                [new ConstantText("Jiné formy dokumentu")],
                                [],
                                [
                                    new CommaSeparatedBuilder(
                                        "f:extension[@url='http://hl7.eu/fhir/StructureDefinition/presentedForm']",
                                        _ => [new OpenTypeElement(null)]
                                    )
                                ],
                                customClass: "no-print"
                            )
                        ),

                        #endregion

                        new UnrenderedResourcesSection(),
                    ],
                    title: [new ConstantText("Další informace o dokumentu")],
                    severity: Severity.Gray
                ),
            ],
            optionalClass: "document-footer"
        );

        return widget.Render(navigator, renderer, context);
    }
}