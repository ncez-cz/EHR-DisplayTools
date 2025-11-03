using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.FamilyMemberHistory;

public class FamilyMembersHistory(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "FamilyMemberHistory";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new FamilyMembersHistory(items)];
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var hasNote = items.Any(x => x.EvaluateCondition("f:note"));

        var infrequentOptions =
            InfrequentProperties.Evaluate<InfrequentPropertiesPaths>(items);

        var table = new Table(
            [
                new TableHead([
                    new TableRow([
                        new If(_ => infrequentOptions.HasAnyOfGroup("DescriptionCell"),
                            new TableCell([new ConstantText("Rodinný příslušník")], TableCellType.Header)
                        ),
                        new If(_ => hasNote, new TableCell([new ConstantText("Poznámka")], TableCellType.Header)),
                        new If(
                            _ => infrequentOptions.HasAnyOfGroup("AdditionalInfoCell"),
                            new TableCell([new ConstantText("Doplňujíci informace")], TableCellType.Header)
                        ),
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Status),
                            new TableCell([new DisplayLabel(LabelCodes.Status)], TableCellType.Header)
                        ),
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        )
                    ])
                ]),
                ..items.Select(x => new TableBody([new FamilyMemberHistoryRowBuilder(x, hasNote, infrequentOptions)])),
            ],
            true
        );

        return table.Render(navigator, renderer, context);
    }

    private class FamilyMemberHistoryRowBuilder(
        XmlDocumentNavigator item,
        bool isNote,
        InfrequentPropertiesData<InfrequentPropertiesPaths> infrequentOptions
    ) : Widget
    {
        public override async Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var rowDetails = new StructuredDetails();

            if (item.EvaluateCondition("f:condition"))
            {
                rowDetails.AddCollapser(new ConstantText("Problémy rodinného příslušníka"),
                    new FamilyMemberCondition(item.SelectAllNodes("f:condition").ToList()));
            }

            if (item.EvaluateCondition("f:text"))
            {
                rowDetails.AddCollapser(new DisplayLabel(LabelCodes.OriginalNarrative), new Narrative("f:text"));
            }

            var tableRowContent = new List<Widget>
            {
                //If section has data
                new If(_ => !item.EvaluateCondition("f:dataAbsentReason"),
                    new Concat([
                        new If(_ => infrequentOptions.HasAnyOfGroup("DescriptionCell"),
                            new FamilyMemberDescriptionCell(item)
                        ),
                        new If(_ => isNote, new TableCell([
                            new Optional("f:note", new ShowAnnotationCompact())
                        ])),
                        new If(
                            _ => infrequentOptions.HasAnyOfGroup("AdditionalInfoCell"),
                            new FamilyMemberAdditionalInfoCell(item)
                        ),
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Status),
                            new TableCell([
                                new EnumIconTooltip("f:status",
                                    "http://hl7.org/fhir/history-status",
                                    new DisplayLabel(LabelCodes.Status)
                                )
                            ])
                        ),
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Text),
                            new NarrativeCell()
                        )
                    ])).Else( //If section is empty
                    new TableCell([new OpenTypeElement(null)],
                        colspan: isNote ? 3 : 2) // used only to render dataAbsentReason
                )
            };

            var result =
                await new TableRow(tableRowContent, rowDetails, idSource: item).Render(item, renderer, context);

            var isRelationship = item.EvaluateCondition("f:relationship");
            if (!isRelationship)
            {
                result.Errors.Add(ParseError.MissingValue(item.SelectSingleNode("f:relationship").GetFullPath()));
            }

            return result;
        }
    }

    private enum InfrequentPropertiesPaths
    {
        [Group("DescriptionCell")] Name, //0..1	string	The family member described

        [Group("DescriptionCell")]
        Relationship, //1..1	CodeableConcept	Relationship to the subject.  http://terminology.hl7.org/CodeSystem/v3-RoleCode

        [Group("DescriptionCell")] [OpenType("born")]
        Born, /*0..1		(approximate) date of birth - Chronometry*/

        [Group("DescriptionCell")] [OpenType("age")]
        Age, /*0..1		(approximate) age - opentype*/

        [Group("DescriptionCell")] [OpenType("deceased")]
        Deceased, /*0..1		Dead? How old/when? - opentype*/
        [Group("DescriptionCell")] Sex, //0..1	CodeableConcept	male | female | other | unknown

        /*Participant, //	0..*	BackboneElement	Members of the team*/
        [Group("AdditionalInfoCell")] Id,
        [Group("AdditionalInfoCell")] Identifier, //	0..*	Identifier	External Id(s) for this record

        [Group("AdditionalInfoCell")] Date, //0..1	dateTime	When history was recorded or last updated
        [Group("AdditionalInfoCell")] ReasonCode, //	0..*	CodeableConcept	Why was family member history performed?

        [Group("AdditionalInfoCell")]
        ReasonReference, //0..*	Reference(Condition | Observation | AllergyIntolerance | QuestionnaireResponse | DiagnosticReport | DocumentReference) Why was family member history performed?
        [Group("AdditionalInfoCell")] Text,

        [EnumValueSet("http://hl7.org/fhir/history-status")]
        Status
    }
}