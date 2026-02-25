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

        var infrequentProperties = InfrequentProperties.Evaluate<PersonOrOrganizationInfrequentProperties>(navigator);

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

        var organizationLogoNavigator = navigator.SelectSingleNode(
            "f:extension[@url='https://hl7.cz/fhir/core/StructureDefinition/cz-organization-logo']");

        List<Widget> innerWidgets =
        [
            new Choose([
                new When("f:name/@value",
                    new NameValuePair(
                        new PlainBadge(new LocalizedLabel("organization.name")),
                        new Text("f:name/@value"),
                        direction: FlexDirection.Column
                    )),
                new When(name, new HumanName("f:name"))
            ]),
            infrequentProperties.Optional(PersonOrOrganizationInfrequentProperties.Gender,
                new NameValuePair(
                    new PlainBadge(new EhdsiDisplayLabel(LabelCodes.AdministrativeGender)),
                    new EnumLabel(".", "http://hl7.org/fhir/ValueSet/administrative-gender"),
                    direction: FlexDirection.Column
                )
            ),
            new If(_ => !showCollapser,
                infrequentProperties.Condition(PersonOrOrganizationInfrequentProperties.Code,
                    new HideableDetails(
                        new NameValuePair(
                            new PlainBadge(new LocalizedLabel("practitioner-role.code")),
                            new CommaSeparatedBuilder("f:code", _ => [new CodeableConcept()]),
                            direction: FlexDirection.Column
                        )
                    )
                )
            ),
            infrequentProperties.Optional(PersonOrOrganizationInfrequentProperties.Organization,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        Direction = FlexDirection.Column,
                        LabelOverride = new EhdsiDisplayLabel(LabelCodes.RepresentedOrganization),
                    }
                )
            ),
            infrequentProperties.Condition(PersonOrOrganizationInfrequentProperties.Specialty,
                new NameValuePair(
                    new PlainBadge(new LocalizedLabel("practitioner-role.specialty")),
                    new CommaSeparatedBuilder("f:specialty", _ => [new CodeableConcept()]),
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(PersonOrOrganizationInfrequentProperties.BirthDate,
                new NameValuePair(
                    new PlainBadge(new EhdsiDisplayLabel(LabelCodes.DateOfBirth)),
                    new ShowDateTime(),
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Condition(PersonOrOrganizationInfrequentProperties.Qualification,
                new HideableDetails(
                    new NameValuePair(
                        new PlainBadge(new LocalizedLabel("practitioner.qualifiaction")),
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
            infrequentProperties.Condition(PersonOrOrganizationInfrequentProperties.Communication,
                new HideableDetails(
                    new NameValuePair(
                        new PlainBadge(new LocalizedLabel("practitioner.communication")),
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
                    )
                )
            ),
            new Container([
                new ContactInformation(),
            ]),
            infrequentProperties.Condition(PersonOrOrganizationInfrequentProperties.Type,
                new HideableDetails(
                    new NameValuePair(
                        new PlainBadge(new LocalizedLabel("organization.type")),
                        new CommaSeparatedBuilder(
                            "f:type",
                            _ => [new CodeableConcept()]
                        ), direction: FlexDirection.Column
                    )
                )
            ),
            infrequentProperties.Optional(PersonOrOrganizationInfrequentProperties.PartOf,
                new HideableDetails(
                    new AnyReferenceNamingWidget(
                        widgetModel: new ReferenceNamingWidgetModel
                        {
                            Type = ReferenceNamingWidgetType.NameValuePair,
                            Direction = FlexDirection.Column,
                            LabelOverride = new LocalizedLabel("organization.partOf"),
                        }
                    )
                )
            ),
            new If(_ => collapserTitle == null && !showCollapser && showNarrative,
                infrequentProperties.Condition(PersonOrOrganizationInfrequentProperties.PartOf,
                    new NarrativeCollapser()
                )
            ),
            infrequentProperties.Condition(PersonOrOrganizationInfrequentProperties.Identifier,
                new NameValuePair(
                    new PlainBadge(new LocalizedLabel("practitioner.identifier")),
                    new ListBuilder(
                        "f:identifier",
                        FlexDirection.Column, _ =>
                        [
                            new NameValuePair(
                                [new IdentifierSystemLabel()],
                                [new ShowIdentifier()]
                            ),
                        ], flexContainerClasses: "gap-0"
                    ),
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(PersonOrOrganizationInfrequentProperties.Practitioner,
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

        Widget logoWidget =
            new If(
                _ => organizationLogoNavigator.Node != null && !noPhoto,
                new ChangeContext(organizationLogoNavigator,
                    new HospitalLogo(imageOptionalClass: "ms-auto header-image"))
            );

        List<Widget> tree;

        if (noFormat)
        {
            tree =
            [
                ..innerWidgets,
                photoWidget,
                logoWidget,
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
                    logoWidget,
                ], flexWrap: false, idSource: practitionerNavigator),
            ];
        }

        var (_, displayName) =
            ReferenceHandler.GetFallbackDisplayName(navigator);

        var summary = ReferenceHandler.GetResourceSummary(navigator);

        if (collapserTitle != null || showCollapser)
        {
            tree =
            [
                new Collapser(
                    title:
                    [
                        new TextContainer(TextStyle.CapitalizeFirst, [
                                collapserTitle ?? new Choose([
                                    new When("f:code",
                                        new CommaSeparatedBuilder("f:code", _ => [new CodeableConcept()])),
                                ], new LocalNodeName()),
                                new If(_ => summary?.Value != null,
                                    new TextContainer(TextStyle.Bold, [
                                            summary?.Value!,
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
                    content: tree,
                    isCollapsed: true, footer: navigator.EvaluateCondition("f:text") && showNarrative
                        ?
                        [
                            new NarrativeCollapser()
                        ]
                        : null, iconPrefix: showNarrative ? [new NarrativeModal()] : null)
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

    public enum PersonOrOrganizationInfrequentProperties
    {
        Gender,
        Code,
        Organization,
        Specialty,
        BirthDate,
        Qualification,
        Communication,
        Type,
        PartOf,
        Identifier,
        Practitioner,
    }
}