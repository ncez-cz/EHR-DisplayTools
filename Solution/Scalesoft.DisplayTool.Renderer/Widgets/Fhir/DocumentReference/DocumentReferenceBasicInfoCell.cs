using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.DocumentReference;

public class DocumentReferenceBasicInfoCell(XmlDocumentNavigator item) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions =
            InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([item]);

        var participantTableCell = new TableCell(
        [
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Description),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Popis"),
                    new Text("f:description/@value")))
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Status),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Stav záznamu"),
                    new EnumLabel("f:status", "http://hl7.org/fhir/ValueSet/document-reference-status")))
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.DocStatus),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Stav dokumentu"),
                    new EnumLabel("f:docStatus", "http://hl7.org/fhir/ValueSet/composition-status")))
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Type),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Typ dokumentu"),
                    new Optional("f:type", new CodeableConcept())))
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Category),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Kategorie dokumentu"),
                    new Optional("f:category", new CodeableConcept())))
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.SecurityLabel),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Citlivost dokumentu"),
                    new Optional("f:securityLabel", new CodeableConcept())))
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Date), new NameValuePair(
                new ConstantText("Datum vytvoření záznamu"),
                new Optional("f:date", new ShowDateTime())))
        ]);

        if (infrequentOptions.Count == 0)
        {
            participantTableCell = new TableCell([
                new TextContainer(TextStyle.Muted, [new ConstantText("Informace nejsou k dispozici")])
            ]);
        }

        return participantTableCell.Render(item, renderer, context);
    }

    private enum InfrequentPropertiesPaths
    {
        Status, //1..1	code	current | superseded | entered-in-error http://hl7.org/fhir/ValueSet/document-reference-status
        DocStatus, //0..1	code	preliminary | final | amended | entered-in-error  http://hl7.org/fhir/ValueSet/composition-status
        Type, //0..1	CodeableConcept	Kind of document (LOINC if possible) http://hl7.org/fhir/ValueSet/c80-doc-typecodes
        Category, //0..*	CodeableConcept	Categorization of document
        Date, //	0..1	instant	When this document reference was created
        Description,
        SecurityLabel, //0..*	CodeableConcept	Document security-tags
    }
}