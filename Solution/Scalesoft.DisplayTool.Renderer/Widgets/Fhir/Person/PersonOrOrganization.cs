using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Person;

/// <summary>
///     Widget for rendering the following FHIR resources:
///     Practitioner, Patient, RelatedPerson, PractitionerRole, Organization.
///     Can also be used to render the "contact" BackboneElement of some resources.
/// </summary>
/// <param name="customSelectionRules">
///     This parameter is used to provide custom selection rules for the resource
///     configuration. If not provided, default rules will be used.
/// </param>
/// <param name="skipWhenInactive">
///     This parameter is used to skip rendering when the end of the "period" element is in the
///     past or "active" is false.
/// </param>
/// <param name="collapserTitle">
///     This parameter is used to provide a title for the collapser widget. If provided, the
///     widget will be wrapped in a collapser.
/// </param>
/// <param name="showNarrative">
///     This parameter dictates whether to add narrative text, keep in mind that if you turn this
///     off, it's expected you handle the narrative text some other way.
/// </param>
/// <param name="showCollapser">
///     This parameter dictates whether to put the data into a collapser with an automatically
///     selected title. If `collapserTitle` is set, this parameter is ignored.
/// </param>
public class PersonOrOrganization(
    XmlDocumentNavigator navigator,
    List<ResourceSelectionRule>? customSelectionRules = null,
    bool skipWhenInactive = false,
    Widget? collapserTitle = null,
    bool showNarrative = true,
    bool showCollapser = false,
    bool noFormat = false,
    bool noPhoto = false
) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator _,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<ParseError> errors = [];

        var configManager = new ResourceConfiguration(customSelectionRules);

        var selectionRulesParseResult = configManager.ProcessConfigurations(navigator);
        errors.AddRange(selectionRulesParseResult.Errors);

        var selectionRules = selectionRulesParseResult.Results;

        var name = selectionRules.First(r => r.Name == ResourceNames.Name).FormattedPath;


        if (navigator.EvaluateCondition("f:period/f:end") && skipWhenInactive)
        {
            var periodEnd = navigator.SelectSingleNode("f:period/f:end/@value").Node?.ValueAsDateTime ??
                            DateTime.MaxValue;
            if (periodEnd < DateTime.Now)
            {
                return RenderResult.NullResult;
            }
        }

        if (navigator.EvaluateCondition("f:active[@value='false']") && skipWhenInactive)
        {
            return RenderResult.NullResult;
        }

        var practitionerNavigator =
            ReferenceHandler.GetSingleNodeNavigatorFromReference(navigator, "f:practitioner", ".");

        var photosFromReference =
            ReferenceHandler.GetNodeNavigatorsFromReferences(navigator, "f:practitioner", "f:photo");

        var photos = photosFromReference ?? navigator.SelectAllNodes("f:photo").ToList();
        var newestPhoto = photos
            .Select(photo => new
            {
                Photo = photo,
                CreationDate = photo
                    .SelectSingleNode("f:creation/@value").Node?.ValueAsDateTime
            })
            .OrderByDescending(x => x.CreationDate.HasValue) // photos with date first
            .ThenByDescending(x => x.CreationDate) // newest first
            .Select(x => x.Photo) // back to original navigator
            .FirstOrDefault();

        List<Widget> innerWidgets =
        [
            new Choose([
                new When("f:name/@value",
                    new NameValuePair(
                        new PlainBadge(new ConstantText("Název")),
                        new Text("f:name/@value"),
                        direction: FlexDirection.Column
                    )),
                new When(name, new HumanName("f:name"))
            ]),
            new Optional("f:gender",
                new NameValuePair(
                    new PlainBadge(new DisplayLabel(LabelCodes.AdministrativeGender)),
                    new EnumLabel(".", "http://hl7.org/fhir/ValueSet/administrative-gender"),
                    direction: FlexDirection.Column
                )
            ),
            new If(_ => !showCollapser && navigator.EvaluateCondition("f:code"),
                new HideableDetails(new NameValuePair(
                    new PlainBadge(new ConstantText("Role")),
                    new CommaSeparatedBuilder("f:code", _ => [new CodeableConcept()]),
                    direction: FlexDirection.Column
                ))
            ),
            new Optional("f:organization",
                new NameValuePair(
                    new PlainBadge(new DisplayLabel(LabelCodes.RepresentedOrganization)),
                    new AnyReferenceNamingWidget(), direction: FlexDirection.Column
                )
            ),
            new Condition("f:specialty",
                new NameValuePair(
                    new PlainBadge(new ConstantText("Specializace")),
                    new CommaSeparatedBuilder("f:specialty", _ => [new CodeableConcept()]),
                    direction: FlexDirection.Column
                )
            ),
            new Optional("f:birthDate",
                new NameValuePair(
                    new PlainBadge(new DisplayLabel(LabelCodes.DateOfBirth)),
                    new ShowDateTime(), direction: FlexDirection.Column
                )
            ),
            new Condition("f:qualification",
                new HideableDetails(new NameValuePair(
                    new PlainBadge(new ConstantText("Kvalifikace")),
                    new CommaSeparatedBuilder(
                        "f:qualification", (_, _, x) =>
                        {
                            if (x.EvaluateCondition("f:period/f:end"))
                            {
                                var periodEnd =
                                    x.SelectSingleNode("f:period/f:end/@value").Node
                                        ?.ValueAsDateTime ??
                                    DateTime.MaxValue;
                                if (periodEnd < DateTime.Now)
                                {
                                    return
                                    [
                                        new TextContainer(TextStyle.Strike,
                                            new ChangeContext("f:code", new CodeableConcept()))
                                    ];
                                }
                            }

                            var code = new ChangeContext("f:code",
                                new CodeableConcept()
                            );

                            return [code];
                        }
                    ), direction: FlexDirection.Column
                ))
            ),
            new Condition("f:communication",
                new HideableDetails(new NameValuePair(
                    new PlainBadge(new ConstantText("Jazyky komunikace")),
                    new CommaSeparatedBuilder(
                        "f:communication",
                        (_, _, _) =>
                        [
                            new Choose([
                                new When("f:language",
                                    new ChangeContext("f:language", new CodeableConcept()))
                            ], new CodeableConcept())
                        ], orderer: elements =>
                        {
                            return elements
                                .OrderByDescending(e =>
                                    e.EvaluateCondition("f:preferred[@value='true']"))
                                .ToList();
                        }
                    ), direction: FlexDirection.Column
                ))
            ),
            new Container([
                new ContactInformation(),
            ]),
            new Condition("f:type",
                new HideableDetails(new NameValuePair(
                    new PlainBadge(new ConstantText("Druhy zařízení")),
                    new CommaSeparatedBuilder(
                        "f:type",
                        _ => [new CodeableConcept()]
                    ), direction: FlexDirection.Column
                ))
            ),
            new Optional("f:partOf",
                new HideableDetails(new NameValuePair(
                    new PlainBadge(new ConstantText("Součástí")),
                    new AnyReferenceNamingWidget(), direction: FlexDirection.Column
                ))
            ),
            new If(
                _ => navigator.EvaluateCondition("f:text") && collapserTitle == null &&
                     !showCollapser && showNarrative,
                new NarrativeCollapser()
            ),
            new Condition("f:identifier",
                new NameValuePair(
                    new PlainBadge(new ConstantText("Identifikátory")),
                    new ListBuilder(
                        "f:identifier",
                        FlexDirection.Column, _ =>
                        [
                            new NameValuePair([new IdentifierSystemLabel()], [new ShowIdentifier()]),
                        ], flexContainerClasses: "gap-0"
                    ), direction: FlexDirection.Column
                )
            ),
            new Optional("f:practitioner",
                ShowSingleReference.WithDefaultDisplayHandler(x =>
                    [new PersonOrOrganization(x, noFormat: true, noPhoto: true)])
            )
        ];

        Widget photoWidget =
            new If(
                _ => newestPhoto != null && !noPhoto,
                new ChangeContext(newestPhoto!,
                    new Attachment(onlyContentOrUrl: true, imageOptionalClass: "ms-auto person-photo")
                )
            );

        List<Widget> tree;

        if (noFormat)
        {
            tree =
            [
                ..innerWidgets,
                photoWidget,
            ];
        }
        else
        {
            tree =
            [
                new Row([
                    new Row([
                        ..innerWidgets,
                    ], flexContainerClasses: "column-gap-6"),
                    photoWidget,
                ], flexWrap: false, idSource: practitionerNavigator),
            ];
        }

        var (_, displayName) =
            ReferenceHandler.GetFallbackDisplayName(navigator);

        var (_, summaryValue) = ReferenceHandler.GetResourceSummary(navigator);

        if (collapserTitle != null || showCollapser)
        {
            tree =
            [
                new Collapser(
                    toggleLabelTitle:
                    [
                        new TextContainer(TextStyle.CapitalizeFirst, [
                                collapserTitle ?? new Choose([
                                    new When("f:code",
                                        new CommaSeparatedBuilder("f:code", _ => [new CodeableConcept()])),
                                ], new LocalNodeName()),
                                new If(_ => summaryValue != null,
                                    new TextContainer(TextStyle.Bold, [
                                            summaryValue!,
                                        ], optionalClass: "black ms-4"
                                    )
                                ).Else(
                                    new If(_ => displayName != null,
                                        new TextContainer(TextStyle.Bold, [
                                                displayName!,
                                            ], optionalClass: "black ms-4"
                                        )
                                    )
                                ),
                            ]
                        )
                    ],
                    title: [],
                    content: tree,
                    footer: navigator.EvaluateCondition("f:text") && showNarrative
                        ?
                        [
                            new NarrativeCollapser()
                        ]
                        : null,
                    iconPrefix: showNarrative ? [new NarrativeModal()] : null,
                    isCollapsed: true
                )
            ];
        }

        var result = await tree.RenderConcatenatedResult(navigator, renderer, context);
        errors.AddRange(result.Errors);

        if (!result.HasValue || errors.MaxSeverity() >= ErrorSeverity.Fatal)
        {
            return errors;
        }

        return new RenderResult(result.Content, errors);
    }
}