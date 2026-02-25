using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.DocumentReference;

public class DocumentReferenceBasicInfoCell(
    XmlDocumentNavigator item,
    InfrequentPropertiesData<DocumentReferences.DocumentReferenceInfrequentProperties> infrequentProperties
) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var participantTableCell = new TableCell(
        [
            new Container([
                new If(
                    _ => infrequentProperties.Contains(DocumentReferences.DocumentReferenceInfrequentProperties
                        .Description),
                    new HideableDetails(new NameValuePair(
                        new LocalizedLabel("document-reference.description"),
                        new Text("f:description/@value")))
                ),
                new If(
                    _ => infrequentProperties.Contains(DocumentReferences.DocumentReferenceInfrequentProperties.Status),
                    new HideableDetails(new NameValuePair(
                        new LocalizedLabel("document-reference.status"),
                        new EnumLabel("f:status", "http://hl7.org/fhir/ValueSet/document-reference-status")))
                ),
                new If(
                    _ => infrequentProperties.Contains(DocumentReferences.DocumentReferenceInfrequentProperties
                        .DocStatus),
                    new HideableDetails(new NameValuePair(
                        new LocalizedLabel("document-reference.docStatus"),
                        new EnumLabel("f:docStatus", "http://hl7.org/fhir/ValueSet/composition-status")))
                ),
                new If(
                    _ => infrequentProperties.Contains(DocumentReferences.DocumentReferenceInfrequentProperties.Type),
                    new HideableDetails(new NameValuePair(
                        new LocalizedLabel("document-reference.type"),
                        new Optional("f:type", new CodeableConcept())))
                ),
                new If(
                    _ => infrequentProperties.Contains(
                        DocumentReferences.DocumentReferenceInfrequentProperties.Category),
                    new HideableDetails(new NameValuePair(
                        new LocalizedLabel("document-reference.category"),
                        new Optional("f:category", new CodeableConcept())))
                ),
                new If(
                    _ => infrequentProperties.Contains(DocumentReferences.DocumentReferenceInfrequentProperties
                        .SecurityLabel),
                    new HideableDetails(new NameValuePair(
                        new LocalizedLabel("document-reference.securityLabel"),
                        new Optional("f:securityLabel", new CodeableConcept())))
                ),
                new If(
                    _ => infrequentProperties.Contains(DocumentReferences.DocumentReferenceInfrequentProperties.Date),
                    new NameValuePair(
                        new LocalizedLabel("document-reference.date"),
                        new Optional("f:date", new ShowDateTime()))),
                infrequentProperties.Optional(DocumentReferences
                        .DocumentReferenceInfrequentProperties
                        .ModalityExtension,
                    new NameValuePair(
                        new LocalizedLabel("document-reference.modality"),
                        new Optional("f:valueCodeableConcept", new CodeableConcept()))
                ),
                infrequentProperties.Optional(DocumentReferences
                        .DocumentReferenceInfrequentProperties
                        .NoteExtension, nav =>
                    {
                        var annotationNav = nav.SelectSingleNode("f:valueAnnotation");
                        return
                        [
                            new NameValuePair(
                                new LocalizedLabel("document-reference.note"),
                                new ChangeContext(annotationNav, new ShowAnnotationCompact())),
                        ];
                    }
                ),
                infrequentProperties.Optional(DocumentReferences
                        .DocumentReferenceInfrequentProperties
                        .ViewExtension,
                    new NameValuePair(
                        new LocalizedLabel("document-reference.view"),
                        new Optional("f:valueCodeableConcept", new CodeableConcept()))
                ),
                new Optional(
                    "f:content[f:extension[@url = 'http://hl7.org/fhir/StructureDefinition/documentreference-thumbnail' and f:valueBoolean/@value = 'false']]/f:attachment",
                    new NameValuePair(
                        new LocalizedLabel("document-reference.content"),
                        new Attachment())),
                new Optional(
                    "f:context/f:period",
                    new NameValuePair(
                        new LocalizedLabel("document-reference.context.period"),
                        new ShowPeriod())),
            ], optionalClass: "name-value-pair-wrapper w-fit-content"),
        ]);

        if (infrequentProperties.Count == 0)
        {
            participantTableCell = new TableCell([
                new TextContainer(TextStyle.Muted, [new LocalizedLabel("general.information-unavailable")]),
            ]);
        }

        return participantTableCell.Render(item, renderer, context);
    }
}