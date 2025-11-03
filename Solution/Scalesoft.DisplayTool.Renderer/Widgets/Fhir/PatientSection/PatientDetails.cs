using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PatientSection;

public class PatientDetails : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var resourceConfiguration = new ResourceConfiguration();
        var configurations = resourceConfiguration.ProcessConfigurations(navigator).Results;
        var name = configurations.First(r => r.Name == ResourceNames.Name).FormattedPath;

        const string ridIdentifier = "https://ncez.mzcr.cz/fhir/sid/rid";
        const string insuranceCompanyCodeIdentifier = "https://ncez.mzcr.cz/fhir/sid/kp";
        const string clinicalGender = "http://hl7.org/fhir/StructureDefinition/patient-sexParameterForClinicalUse";
        const string birthPlace = "http://hl7.org/fhir/StructureDefinition/patient-birthPlace";
        const string nationality = "http://hl7.org/fhir/StructureDefinition/patient-nationality";
        const string registeringProvider = "https://hl7.cz/fhir/core/StructureDefinition/registering-provider-cz";
        const string patientAnimal = "http://hl7.org/fhir/StructureDefinition/patient-animal";
        const string recordedSexOrGender = "http://hl7.org/fhir/StructureDefinition/individual-recordedSexOrGender";

        var isAnimal = navigator.EvaluateCondition($"f:extension[@url='{patientAnimal}']");
        var hasRid = navigator.EvaluateCondition($"f:identifier[f:system/@value='{ridIdentifier}']");
        var nonRidIdentifiers =
            navigator.SelectAllNodes($"f:identifier[not(f:system/@value='{ridIdentifier}')]").ToList();

        var otherIdentifiers =
            new NameValuePair(
                [
                    new PlainBadge(new ConstantText("Ostatní identifikátory")),
                ],
                [
                    new ConcatBuilder(
                        nonRidIdentifiers,
                        (n, _, nav) =>
                        {
                            if (n == 0 && !hasRid)
                            {
                                return [new NullWidget()];
                            }


                            var official = nav.EvaluateCondition("f:use/@value='official'");
                            var showSystem = nav.EvaluateCondition(
                                "f:type/f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/v2-0203' and f:code/@value='PPN']");

                            return official
                                ? HandleIdentifierDisplay(nav, showSystem || !official,
                                    insuranceCompanyCodeIdentifier)
                                :
                                [
                                    new HideableDetails(HandleIdentifierDisplay(nav, showSystem || !official,
                                        insuranceCompanyCodeIdentifier))
                                ];
                        }),
                ], direction: FlexDirection.Column,
                optionalClasses: new NameValuePair.NameValuePairClasses
                    { ValueClass = "patient-identifier-grid" }
            );

        var tree = new List<Widget>
        {
            new Column([
                new Row([
                    //name
                    new ChangeContext(name,
                        new HumanName(".", true, nameWrapper: x => new HeadingNoMargin([
                                new TextContainer(TextStyle.Bold, [
                                    x,
                                ]),
                            ], HeadingSize.H6),
                            hideNominalLetters: isAnimal
                        )
                    ),
                    new Optional($"f:extension[@url='{patientAnimal}']",
                        new AnimalDetails()
                    ),
                    new Optional("f:birthDate",
                        new Container([
                            new NameValuePair(
                                new PlainBadge(
                                    new DisplayLabel(LabelCodes.DateOfBirth)
                                ),
                                new HeadingNoMargin([
                                    new TextContainer(TextStyle.Bold, [
                                        new ShowDateTime()
                                    ])
                                ], HeadingSize.H6), direction: FlexDirection.Column
                            )
                        ])
                    ),
                    //Gender
                    new Container([
                        new If(_ => navigator.EvaluateCondition("f:gender"),
                                new NameValuePair(
                                    new PlainBadge(
                                        new DisplayLabel(LabelCodes.AdministrativeGender)
                                    ),
                                    new HeadingNoMargin(
                                        [
                                            new TextContainer(TextStyle.Bold, [
                                                new EnumLabel("f:gender",
                                                    "https://hl7.cz/fhir/ValueSet/administrative-gender-cz")
                                            ])
                                        ],
                                        HeadingSize.H6), direction: FlexDirection.Column
                                )
                            )
                            .Else(
                                new Optional($"f:extension[@url='{recordedSexOrGender}']",
                                    new HideableDetails(new NameValuePair(
                                        [
                                            new PlainBadge(
                                                new DisplayLabel(LabelCodes.AdministrativeGender)
                                            )
                                        ],
                                        [
                                            new Optional("f:extension[@url='value']/f:valueCodeableConcept",
                                                new HeadingNoMargin([
                                                    new TextContainer(TextStyle.Bold, [
                                                        new CodeableConcept()
                                                    ])
                                                ], HeadingSize.H6)
                                            ),
                                            new Optional("f:extension[@url='type']/f:valueCodeableConcept",
                                                new NameValuePair(
                                                    new ConstantText("Typ"),
                                                    new CodeableConcept()
                                                )
                                            )
                                        ], direction: FlexDirection.Column
                                    ))
                                )
                            ),
                    ]),
                    // Nationality
                    new If(_ => navigator.EvaluateCondition($"f:extension[@url='{nationality}']"),
                        new HideableDetails(new Container([
                                new NameValuePair(
                                    new PlainBadge(
                                        new ConstantText("Národnost")
                                    ),
                                    new ConcatBuilder(
                                        $"f:extension[@url='{nationality}']", _ =>
                                        [
                                            new Concat([
                                                new Optional("f:extension[@url='code']",
                                                    new HeadingNoMargin([
                                                            new TextContainer(TextStyle.Bold, [
                                                                new OpenTypeElement(null)
                                                            ])
                                                        ],
                                                        HeadingSize.H6)), // CodeableConcept
                                                new If(
                                                    _ => navigator.EvaluateCondition("f:extension[@url='period']"),
                                                    new ConstantText("-"),
                                                    new Optional("f:extension[@url='period']",
                                                        new OpenTypeElement(null))), // Period
                                            ]),
                                        ]),
                                    direction: FlexDirection.Column
                                )
                            ]
                        ))),
                    new If(_ => navigator.EvaluateCondition($"f:identifier[f:system/@value='{ridIdentifier}']"),
                        new ChangeContext($"f:identifier[f:system/@value='{ridIdentifier}']",
                            new NameValuePair(
                                new PlainBadge(
                                    new ConstantText("Resortní Identifikátor")
                                ),
                                new HeadingNoMargin([
                                    new TextContainer(TextStyle.Bold, [
                                        new ShowIdentifier(showSystem: false)
                                    ])
                                ], HeadingSize.H6), direction: FlexDirection.Column
                            )
                        )
                    ).Else(
                        new ChangeContext("f:identifier",
                            new NameValuePair(
                                new PlainBadge(new ConstantText("Identifikátor pacienta")),
                                new HeadingNoMargin([
                                    new TextContainer(TextStyle.Bold, [
                                        new ShowIdentifier(showSystem: false)
                                    ])
                                ], HeadingSize.H6), direction: FlexDirection.Column
                            )
                        )
                    ),
                ], flexContainerClasses: "justify-content-between column-gap-4", flexWrap: false),
                new ThematicBreak(),
                new Column([
                    new Row([
                        //Clinical Gender
                        new If(_ => navigator.EvaluateCondition($"f:extension[@url='{clinicalGender}']"),
                            new HideableDetails(new Container([
                                new NameValuePair(
                                    new PlainBadge(new ConstantText("Pohlaví pro klinické použití")),
                                    new ConcatBuilder($"f:extension[@url='{clinicalGender}']", _ =>
                                    [
                                        new Concat([
                                            new NameValuePair(
                                                new ConstantText("Pohlaví"),
                                                new TextContainer(TextStyle.Bold,
                                                [
                                                    new Optional("f:extension[@url='value']", new OpenTypeElement(null))
                                                ]) // CodeableConcept
                                            ),
                                            new NameValuePair(
                                                [new ConstantText("Období")],
                                                [
                                                    new Optional("f:extension[@url='period']",
                                                        new TextContainer(TextStyle.Bold, [
                                                            new OpenTypeElement(null)
                                                        ])
                                                    )
                                                ] // Period
                                            ),
                                            new NameValuePair(
                                                new ConstantText("Komentář"),
                                                new Optional("f:extension[@url='comment']",
                                                    new TextContainer(TextStyle.Bold, [
                                                        new OpenTypeElement(null)
                                                    ])
                                                ) // string
                                            ),
                                        ], string.Empty),
                                    ]), direction: FlexDirection.Column
                                )
                            ]))),
                        new Container([
                            //Contacts
                            new ContactInformation(),
                        ]),
                        new Optional(
                            $"f:extension[@url='{patientAnimal}']/f:extension[@url='genderStatus']/f:valueCodeableConcept",
                            new NameValuePair(
                                new PlainBadge(new ConstantText("Stav pohlaví")),
                                new TextContainer(TextStyle.Bold, new CodeableConcept())
                            )
                        ),

                        //Identifiers
                        new If(
                            _ => (nonRidIdentifiers.Count != 0 && hasRid) || (!hasRid && nonRidIdentifiers.Count > 1),
                            navigator.EvaluateCondition("not(f:identifier/f:use/@value='official')")
                                ? new HideableDetails(otherIdentifiers)
                                : otherIdentifiers
                        ),
                    ], flexContainerClasses: "column-gap-6", flexWrap: false),
                    new Row([
                        //Birth Place
                        new If(_ => navigator.EvaluateCondition($"f:extension[@url='{birthPlace}']"),
                            new HideableDetails(new Container([
                                new PlainBadge(new ConstantText("Místo narození")), new LineBreak(), new ChangeContext(
                                    $"f:extension[@url='{birthPlace}']",
                                    new TextContainer(TextStyle.Bold,
                                        new OpenTypeElement(null,
                                            hints: OpenTypeElementRenderingHints.HideAddressLabel)) // CZ_Address
                                )
                            ]))),
                        new If(_ => navigator.EvaluateCondition($"f:extension[@url='{registeringProvider}']"),
                            new HideableDetails(new Container([
                                new NameValuePair(
                                    new PlainBadge(new ConstantText("Registrující poskytovatel")),
                                    new ConcatBuilder($"f:extension[@url='{registeringProvider}']", _ =>
                                        [
                                            new Concat([
                                                new NameValuePair(
                                                    new Optional("f:extension[@url='category']",
                                                        new OpenTypeElement(null)), // CodeableConcept
                                                    new Optional("f:extension[@url='value']",
                                                        new OpenTypeElement(
                                                            null)) // Reference(Organization | Practitioner Role)
                                                )
                                            ]),
                                        ]
                                    ), direction: FlexDirection.Column
                                )
                            ]))
                        ),
                    ], flexContainerClasses: "column-gap-11"),
                ], flexContainerClasses: "row-gap-1")
            ], flexContainerClasses: "mb-1"),
        };

        return tree.RenderConcatenatedResult(navigator, renderer, context);
    }

    private Widget[] HandleIdentifierDisplay(
        XmlDocumentNavigator nav,
        bool showSystem,
        string insuranceCompanyCodeIdentifier,
        bool? isInsuranceCoIdOfficial = null
    )
    {
        if (nav.EvaluateCondition("f:system[@value='https://ncez.mzcr.cz/fhir/sid/cpoj']"))
            return
            [
                new Concat([
                    new NameValuePair([new IdentifierSystemLabel(),], [new ShowIdentifier(showSystem: false),]),
                    new ShowSingleReference(issuerNav =>
                    {
                        if (!issuerNav.ResourceReferencePresent)
                        {
                            return [];
                        }

                        var indentifierUseCondition = string.Empty;
                        if (isInsuranceCoIdOfficial == true)
                        {
                            indentifierUseCondition = "and f:use/@value='official'";
                        }

                        if (isInsuranceCoIdOfficial == false)
                        {
                            indentifierUseCondition = "and not(f:use/@value='official')";
                        }

                        return
                        [
                            new ChangeContext(issuerNav.Navigator,
                                new Optional(
                                    $"f:identifier[f:system/@value='{insuranceCompanyCodeIdentifier}' {indentifierUseCondition}]",
                                    new NameValuePair([new IdentifierSystemLabel(),],
                                        [new ShowIdentifier(showSystem: false),])
                                )
                            )
                        ];
                    }, "f:assigner"),
                ]),
            ];

        return
        [
            new NameValuePair([
                    new IdentifierSystemLabel()
                ],
                [
                    new ShowIdentifier(showSystem: showSystem),
                ]
            ),
        ];
    }
}