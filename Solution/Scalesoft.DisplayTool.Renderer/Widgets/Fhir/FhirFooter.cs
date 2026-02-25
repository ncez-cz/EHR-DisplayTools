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
        Widget authorResourceTitle = new EhdsiDisplayLabel(LabelCodes.Author);

        if (context.DocumentType is DocumentType.ImagingOrder
            or DocumentType.LaboratoryOrder)
        {
            authorResourceTitle = new LocalizedLabel("general.requester");
        }

        var bundle = navigator.SelectSingleNode("ancestor::f:Bundle[1]");
        var bundleSignorNav = ReferenceHandler.GetSingleNodeNavigatorFromReference(bundle, "f:signature/f:who", ".");
        Widget? summaryValue = null;
        if (bundleSignorNav != null)
        {
            var summary = ReferenceHandler.GetResourceSummary(bundleSignorNav);
            summaryValue = summary?.Value;
        }

        var signatureData = bundle.SelectSingleNode("f:signature/f:data/@value").Node?.Value;
        var signatureMimeType = bundle.SelectSingleNode("f:signature/f:sigFormat/@value").Node?.Value;

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
                                        x.ToList(),
                                        isCollapsed: true
                                    ),
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
                                        collapserTitle: new EhdsiDisplayLabel(LabelCodes.Custodian)),
                                ], idSource: x)).ToList(),
                                x =>
                                [
                                    new Collapser(
                                        [new EhdsiDisplayLabel(LabelCodes.Custodian)],
                                        x.ToList(),
                                        isCollapsed: true
                                    ),
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
                                        collapserTitle: new EhdsiDisplayLabel(LabelCodes.RepresentedOrganization)),
                                ], idSource: x)).ToList(),
                                x =>
                                [
                                    new Collapser(
                                        [new EhdsiDisplayLabel(LabelCodes.RepresentedOrganization)],
                                        x.ToList(),
                                        isCollapsed: true
                                    ),
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
                                                new LocalizedLabel("composition.attester"),

                                                ShowSingleReference.WithDefaultDisplayHandler(
                                                    nav =>
                                                    [
                                                        new Optional("f:name",
                                                            new TextContainer(TextStyle.Bold,
                                                                new Choose([
                                                                        new When("@value",
                                                                            new Text("@value")),
                                                                    ],
                                                                    new HumanNameCompact(".")),
                                                                optionalClass: "black ms-4", idSource: nav
                                                            )
                                                        ),
                                                    ],
                                                    "f:party"),
                                            ], [
                                                new HideableDetails(
                                                    new Row([
                                                        new NameValuePair(
                                                            [new LocalizedLabel("composition.attester.mode")],
                                                            [
                                                                new EnumLabel("f:mode",
                                                                    "http://hl7.org/fhir/ValueSet/composition-attestation-mode"),
                                                            ]),
                                                        new Choose([
                                                            new When("f:time",
                                                                new NameValuePair(
                                                                    [new LocalizedLabel("composition.attester.time")],
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
                                            ], isCollapsed: true, footer: attesterNarrative != null
                                                ?
                                                [
                                                    new NarrativeCollapser(attesterNarrative.GetFullPath()),
                                                ]
                                                : null, iconPrefix:
                                            [
                                                new If(_ => attesterNarrative != null,
                                                    new NarrativeModal(attesterNarrative?.GetFullPath()!)
                                                ),
                                            ]),
                                    ],
                                    optionalClass: !x.EvaluateCondition("f:party/f:reference")
                                        ? HideableDetails.HideableDetailsClass
                                        : string.Empty);


                                return [tree];
                            }
                        ),

                        #endregion

                        #region Encounter

                        new Optional(
                            "f:encounter",
                            // multireference widget is used only for customising broken references builder, semantically the reference is x..1
                            new If(x => !context.IsResourceRendered(x),
                                new ShowMultiReference(
                                    ".",
                                    (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                                    x =>
                                    [
                                        new Collapser(
                                            [new LocalizedLabel("node-names.Encounter")],
                                            x.ToList(),
                                            isCollapsed: true
                                        ),
                                    ]
                                )
                            )
                        ),

                        #endregion

                        #region Presented form

                        new Condition(
                            "f:extension[@url='http://hl7.eu/fhir/StructureDefinition/presentedForm']",
                            new Collapser(
                                [new LocalizedLabel("composition.presented-form")],
                                [
                                    new CommaSeparatedBuilder(
                                        "f:extension[@url='http://hl7.eu/fhir/StructureDefinition/presentedForm']",
                                        _ => [new OpenTypeElement(null)]
                                    ),
                                ],
                                customClass: "no-print"
                            )
                        ),

                        #endregion

                        #region Bundle signature

                        new ChangeContext(bundle, new Optional(
                            "f:signature",
                            new Collapser(
                                [
                                    new LocalizedLabel("bundle.signature"), new If(_ => summaryValue != null,
                                        new TextContainer(TextStyle.Bold, [
                                                summaryValue!,
                                            ], optionalClass: "black ms-4"
                                        )
                                    ),
                                ],
                                [
                                    new Row(
                                        [
                                            new NameValuePair(
                                                new LocalizedLabel("bundle.signature.type"),
                                                new CommaSeparatedBuilder("f:type", _ => new Coding()),
                                                direction: FlexDirection.Column,
                                                style: NameValuePair.NameValuePairStyle.Primary
                                            ),
                                            new NameValuePair(
                                                new LocalizedLabel("bundle.signature.when"),
                                                new ShowInstant("f:when"),
                                                direction: FlexDirection.Column,
                                                style: NameValuePair.NameValuePairStyle.Primary
                                            ),
                                            new AnyReferenceNamingWidget("f:who",
                                                widgetModel: new ReferenceNamingWidgetModel
                                                {
                                                    Type = ReferenceNamingWidgetType.NameValuePair,
                                                    LabelOverride = new LocalizedLabel("bundle.signature.who"),
                                                    Direction = FlexDirection.Column,
                                                    Style = NameValuePair.NameValuePairStyle.Primary,
                                                }
                                            ),
                                            new NameValuePair(
                                                new LocalizedLabel("bundle.signature.data"),
                                                new If(_ => signatureMimeType?.StartsWith("image/") == true, new Image(
                                                        $"data:{signatureMimeType};base64,{signatureData}",
                                                        optionalClass: "bundle-signature-img"))
                                                    .Else(new Link(
                                                        new LocalizedLabel("general.download-link"),
                                                        $"data:{signatureMimeType};base64,{signatureData}",
                                                        contentType: signatureMimeType, downloadInfo: string.Empty)),
                                                direction: FlexDirection.Column,
                                                style: NameValuePair.NameValuePairStyle.Primary
                                            ),
                                        ],
                                        flexContainerClasses: "column-gap-6 row-gap-1"
                                    ),
                                ],
                                isCollapsed: true
                            )
                        )),

                        #endregion

                        new UnrenderedResourcesSection(),
                    ],
                    title: [new LocalizedLabel("general.other-document-information")],
                    severity: Severity.Gray
                ),
            ],
            optionalClass: "document-footer"
        );

        return widget.Render(navigator, renderer, context);
    }
}