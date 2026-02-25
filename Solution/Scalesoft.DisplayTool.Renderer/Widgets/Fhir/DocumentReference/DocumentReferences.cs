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
    public static bool HasBorderedContainer(Widget widget) => true;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new DocumentReferences(items)];
    }

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        Widget? label = null;
        Widget? value = null;
        if (item.EvaluateCondition("f:content/f:attachment/f:title"))
        {
            value = new ChangeContext(item,
                new CommaSeparatedBuilder("f:content/f:attachment/f:title", _ => new Text("@value")));
        }
        else if (item.EvaluateCondition("f:type"))
        {
            value = new ChangeContext(item, "f:type", new CodeableConcept());
        }
        else if (item.EvaluateCondition("f:category"))
        {
            value = new ChangeContext(item, new CommaSeparatedBuilder("f:category", _ => new CodeableConcept()));
        }

        return value != null ? new ResourceSummaryModel(label, value) : null;
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions =
            InfrequentProperties.Evaluate<DocumentReferenceInfrequentProperties>(items);

        var table = new Table(
            [
                new TableHead([
                    new TableRow([
                        new If(
                            _ => infrequentOptions.HasAnyOfGroup("BasicInfoCell"),
                            new TableCell([new LocalizedLabel("general.basic-information")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentOptions.HasAnyOfGroup("ActorsCell"),
                            new TableCell([new LocalizedLabel("general.involvedParties")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentOptions.ContainsAnyOf(DocumentReferenceInfrequentProperties.Status,
                                DocumentReferenceInfrequentProperties.DocStatus),
                            new TableCell([new LocalizedLabel("general.other")], TableCellType.Header)
                        ),
                        new If(_ => infrequentOptions.Contains(DocumentReferenceInfrequentProperties.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        ),
                    ]),
                ]),
                ..items.Select(x => new TableBody([new DocumentReferenceRowBuilder(x, infrequentOptions)])),
            ],
            true
        );
        return table.Render(navigator, renderer, context);
    }

    private class DocumentReferenceRowBuilder(
        XmlDocumentNavigator item,
        InfrequentPropertiesData<DocumentReferenceInfrequentProperties> infrequentProperties
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
                rowDetails.Add(
                    new CollapsibleDetail(
                        new LocalizedLabel("document-reference.content"),
                        new ItemListBuilder("f:content/f:attachment", ItemListType.Unordered, _ => [new Attachment()])
                    )
                );
            }

            if (item.EvaluateCondition("f:context"))
            {
                rowDetails.Add(
                    new CollapsibleDetail(
                        new LocalizedLabel("document-reference.context"),
                        new DocumentReferenceContext(item.SelectAllNodes("f:context").ToList())
                    )
                );
            }

            if (item.EvaluateCondition("f:text"))
            {
                rowDetails.Add(
                    new CollapsibleDetail(
                        new EhdsiDisplayLabel(LabelCodes.OriginalNarrative),
                        new Narrative("f:text")
                    )
                );
            }

            var tableRowContent = new List<Widget>
            {
                new If(
                    _ => infrequentProperties.HasAnyOfGroup("BasicInfoCell"),
                    new DocumentReferenceBasicInfoCell(item, infrequentProperties)
                ),
                new If(
                    _ => infrequentProperties.HasAnyOfGroup("ActorsCell"),
                    new DocumentReferenceActorsCell(item)
                ),
                new If(
                    _ => infrequentProperties.ContainsAnyOf(DocumentReferenceInfrequentProperties.Status,
                        DocumentReferenceInfrequentProperties.DocStatus),
                    new TableCell([
                        new Concat([
                            new EnumIconTooltip("f:status", "http://hl7.org/fhir/document-reference-status",
                                new EhdsiDisplayLabel(LabelCodes.Status)),
                            new EnumIconTooltip("f:docStatus", "http://hl7.org/fhir/composition-status",
                                new LocalizedLabel("document-reference.docStatus"))
                        ]),
                    ])
                ),
                new If(_ => infrequentProperties.Contains(DocumentReferenceInfrequentProperties.Text),
                    new NarrativeCell()
                ),
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

    public enum DocumentReferenceInfrequentProperties
    {
        [Group("BasicInfoCell")]
        Type, //0..1	CodeableConcept	Kind of document (LOINC if possible) http://hl7.org/fhir/ValueSet/c80-doc-typecodes
        [Group("BasicInfoCell")] Category, //0..*	CodeableConcept	Categorization of document
        [Group("BasicInfoCell")] Date, //	0..1	instant	When this document reference was created
        [Group("BasicInfoCell")] Description,
        [Group("BasicInfoCell")] SecurityLabel, //0..*	CodeableConcept	Document security-tags

        [Group("BasicInfoCell")] [Extension("https://hl7.cz/fhir/img/StructureDefinition/modality-cz")]
        ModalityExtension, //1..1	Complex	DocumentReference.modality extension for R4

        [Group("BasicInfoCell")] [Extension("http://hl7.org/fhir/StructureDefinition/note")]
        NoteExtension, //0..1	Annotation	 	Additional notes that apply to this resource or element

        [Group("BasicInfoCell")]
        [Extension("https://hl7.cz/fhir/img/StructureDefinition/CrossVersionMediaViewExtension")]
        ViewExtension, //0..1	CodeableConcept	 	 	Media.view extension

        [Group("BasicInfoCell")] Context, //0..1	BackboneElement	 Clinical context of document
        [Group("BasicInfoCell")] Content, //0..1	BackboneElement	 Document referenced

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