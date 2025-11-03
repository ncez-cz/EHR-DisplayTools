using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.DocumentReference;

public class DocumentReferences(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "DocumentReference";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new DocumentReferences(items)];
    }
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions =
            InfrequentProperties.Evaluate<InfrequentPropertiesPaths>(items);

        var table = new Table(
            [
                new TableHead([
                    new TableRow([
                        new If(
                            _ => infrequentOptions.HasAnyOfGroup("BasicInfoCell"),
                            new TableCell([new ConstantText("Základní informace")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentOptions.HasAnyOfGroup("ActorsCell"),
                            new TableCell([new ConstantText("Zainteresované strany")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentOptions.ContainsAnyOf(InfrequentPropertiesPaths.Status,
                                InfrequentPropertiesPaths.DocStatus),
                            new TableCell([new ConstantText("Další")], TableCellType.Header)
                        ),
                        new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        )
                    ])
                ]),
                ..items.Select(x => new TableBody([new DocumentReferenceRowBuilder(x, infrequentOptions)])),
            ],
            true
        );
        return table.Render(navigator, renderer, context);
    }

    private class DocumentReferenceRowBuilder(
        XmlDocumentNavigator item,
        InfrequentPropertiesData<InfrequentPropertiesPaths> infrequentProperties
    ) : Widget
    {
        public override async Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var rowDetails = new StructuredDetails();

            if (item.EvaluateCondition("f:content"))
            {
                rowDetails.AddCollapser(
                    new ConstantText("Soubory"),
                    new ItemListBuilder("f:content/f:attachment", ItemListType.Unordered, _ => [new Attachment()])
                );
            }

            if (item.EvaluateCondition("f:context"))
            {
                rowDetails.AddCollapser(new ConstantText("Kontext"),
                    new DocumentReferenceContext(item.SelectAllNodes("f:context").ToList()));
            }

            if (item.EvaluateCondition("f:text"))
            {
                rowDetails.AddCollapser(new DisplayLabel(LabelCodes.OriginalNarrative), new Narrative("f:text"));
            }

            var tableRowContent = new List<Widget>
            {
                new If(
                    _ => infrequentProperties.HasAnyOfGroup("BasicInfoCell"),
                    new DocumentReferenceBasicInfoCell(item)
                ),
                new If(
                    _ => infrequentProperties.HasAnyOfGroup("ActorsCell"),
                    new DocumentReferenceActorsCell(item)
                ),
                new If(
                    _ => infrequentProperties.ContainsAnyOf(InfrequentPropertiesPaths.Status,
                        InfrequentPropertiesPaths.DocStatus),
                    new TableCell([
                        new Concat([
                            new EnumIconTooltip("f:status", "http://hl7.org/fhir/document-reference-status",
                                new DisplayLabel(LabelCodes.Status)),
                            new EnumIconTooltip("f:docStatus", "http://hl7.org/fhir/composition-status",
                                new ConstantText("Stav dokumentu"))
                        ])
                    ])
                ),
                new If(_ => infrequentProperties.Contains(InfrequentPropertiesPaths.Text),
                    new NarrativeCell()
                )
            };

            var result =
                await new TableRow(tableRowContent, rowDetails, idSource: item).Render(item, renderer, context);

            var isCode = item.EvaluateCondition("f:code");
            if (!isCode)
            {
                result.Errors.Add(ParseError.MissingValue(item.SelectSingleNode("f:code").GetFullPath()));
            }

            return result;
        }
    }

    public enum InfrequentPropertiesPaths
    {
        [Group("BasicInfoCell")]
        Type, //0..1	CodeableConcept	Kind of document (LOINC if possible) http://hl7.org/fhir/ValueSet/c80-doc-typecodes
        [Group("BasicInfoCell")] Category, //0..*	CodeableConcept	Categorization of document
        [Group("BasicInfoCell")] Date, //	0..1	instant	When this document reference was created
        [Group("BasicInfoCell")] Description,
        [Group("BasicInfoCell")] SecurityLabel, //0..*	CodeableConcept	Document security-tags

        [Group("ActorsCell")]
        Author, //0..*	Reference(Practitioner | PractitionerRole | Organization | Device | Patient | RelatedPerson)	Who and/or what authored the document

        [Group("ActorsCell")]
        Authenticator, //0..1	Reference(Practitioner | PractitionerRole | Organization)	Who/what authenticated the document
        [Group("ActorsCell")] Custodian, //0..1	Reference(Organization)	Organization which maintains the document
        [Group("ActorsCell")] RelatesTo, /*0..*	BackboneElement	Relationships to other documents*/

        Text,

        [EnumValueSet("http://hl7.org/fhir/document-reference-status")]
        Status, //1..1	code	current | superseded | entered-in-error http://hl7.org/fhir/ValueSet/document-reference-status

        [EnumValueSet("http://hl7.org/fhir/composition-status")]
        DocStatus, //0..1	code	preliminary | final | amended | entered-in-error  http://hl7.org/fhir/ValueSet/composition-status
    }
}