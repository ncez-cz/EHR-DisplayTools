using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.CareTeam;

public class CareTeams(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "CareTeam";

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new CareTeams(items)];
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<CareTeamInfrequentProperties>(items);


        var participantNavigators = items
            .Select(x => x
                .SelectAllNodes("f:participant"))
            .SelectMany(x => x)
            .ToList();

        var participantInfrequentProperties =
            InfrequentProperties.Evaluate<CareTeamParticipantInfrequentProperties>(
                participantNavigators);

        var table = new Table(
            [
                new TableHead([
                    new TableRow([
                        new If(_ => participantInfrequentProperties.Count != 0,
                            new TableCell([new ConstantText("Členové týmu")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentProperties.HasAnyOfGroup("BasicInfoCell"),
                            new TableCell([new ConstantText("Základní informace")], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.HasAnyOfGroup("AdditionalInfoCell"),
                            new TableCell([new ConstantText("Doplňujíci informace")], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(CareTeamInfrequentProperties.Status),
                            new TableCell([new DisplayLabel(LabelCodes.Status)], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(CareTeamInfrequentProperties.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        )
                    ])
                ]),
                ..items.Select(x =>
                    new TableBody([new CareTeamRowBuilder(x, infrequentProperties, participantInfrequentProperties)])),
            ],
            true
        );

        return table.Render(navigator, renderer, context);
    }

    private class CareTeamRowBuilder(
        XmlDocumentNavigator item,
        InfrequentPropertiesData<CareTeamInfrequentProperties> infrequentProperties,
        InfrequentPropertiesData<CareTeamParticipantInfrequentProperties> participantInfrequentProperties
    ) : Widget
    {
        public override async Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var rowDetails = new StructuredDetails();
            if (item.EvaluateCondition("f:encounter"))
            {
                var encounterNarrative = ReferenceHandler.GetSingleNodeNavigatorFromReference(item,
                    "f:encounter", "f:text");

                rowDetails.AddCollapser(new ConstantText(Labels.Encounter),
                    ShowSingleReference.WithDefaultDisplayHandler(nav => [new EncounterCard(nav, false, false)],
                        "f:encounter"),
                    encounterNarrative != null
                        ?
                        [
                            new NarrativeCollapser(encounterNarrative.GetFullPath())
                        ]
                        : null,
                    encounterNarrative != null
                        ? new NarrativeModal(encounterNarrative.GetFullPath())
                        : null
                );
            }

            if (item.EvaluateCondition("f:reasonReference"))
            {
                var conditions = ReferenceHandler.GetContentFromReferences(item, "f:reasonReference");
                var unknownConditions = ReferenceHandler.GetReferencesWithoutContent(item, "f:reasonReference");
                // reference display name is being handled in AnyReferenceNamingWidget below
                if (conditions.Any())
                {
                    rowDetails.AddCollapser(new ConstantText("Související problém"),
                        new Conditions(conditions, new ConstantText("Problém")));
                }

                if (unknownConditions.Any())
                {
                    var displayValues = new List<Widget>();
                    foreach (var unknownCondition in unknownConditions)
                    {
                        displayValues.Add(new ChangeContext(unknownCondition, new AnyReferenceNamingWidget()));
                        displayValues.Add(new LineBreak());
                    }

                    rowDetails.AddCollapser(new ConstantText("Neznámý související problém"),
                        new Optional(".", displayValues.ToArray()));
                }
            }

            if (item.EvaluateCondition("f:text"))
            {
                rowDetails.AddCollapser(new DisplayLabel(LabelCodes.OriginalNarrative), new Narrative("f:text"));
            }

            var tableRowContent = new List<Widget>
            {
                new If(_ => participantInfrequentProperties.Count != 0,
                    new CareTeamsParticipantsCell(item)
                ),
                new If(_ => infrequentProperties.HasAnyOfGroup("BasicInfoCell"),
                    new CareTeamsBasicInfoCell(item)
                ),
                new If(_ => infrequentProperties.HasAnyOfGroup("AdditionalInfoCell"),
                    new CareTeamsAdditionalInfoCell(item)
                ),
                new If(_ => infrequentProperties.Contains(CareTeamInfrequentProperties.Status),
                    new TableCell([
                            new EnumIconTooltip("f:status", "http://hl7.org/fhir/care-team-status",
                                new DisplayLabel(LabelCodes.Status))
                        ]
                    )),
                new If(_ => infrequentProperties.Contains(CareTeamInfrequentProperties.Text),
                    new NarrativeCell()
                )
            };

            var result =
                await new TableRow(tableRowContent, rowDetails, idSource: item).Render(item, renderer, context);
            return result;
        }
    }

    public enum CareTeamParticipantInfrequentProperties
    {
        /*Participant, //	0..*	BackboneElement	Members of the team*/
        Role, //0..*	CodeableConcept	Type of involvement
        Member, //0..1	Reference(Practitioner | PractitionerRole | RelatedPerson | Patient | Organization | CareTeam)	Who is involved
        OnBehalfOf, //	0..1	Reference(Organization)	Organization of the practitioner
        Period //	0..1	Period	Time period of participant
    }

    public enum CareTeamInfrequentProperties
    {
        [Group("BasicInfoCell")] Category, //0..*	CodeableConcept	Type of team
        [Group("BasicInfoCell")] Name, //	0..1	string	Name of the team, such as crisis assessment team
        [Group("BasicInfoCell")] Period, //	0..1	Period	Time period team covers
        [Group("BasicInfoCell")] Telecom, //	0..*	        Identifier, //0..*	Identifier	External Ids for this team

        [Group("AdditionalInfoCell")] Id,
        [Group("AdditionalInfoCell")] ReasonCode, //	0..*	CodeableConcept	Why the care team exists

        [Group("AdditionalInfoCell")]
        ManagingOrganization, //0..*	Reference(Organization)	Organization responsible for the care team

        [Group("AdditionalInfoCell")]
        Note, //0..*	Annotation	Comments made about the CareTeamContactPoint	A contact detail for the care team (that applies to all members)

        Text,

        [EnumValueSet("http://hl7.org/fhir/care-team-status")]
        Status
    }
}