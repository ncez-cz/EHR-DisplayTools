using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.DocumentReference;

public class DocumentReferenceActorsCell(XmlDocumentNavigator item) : Widget
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
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Author), new NameValuePair(
                new ConstantText("Autor dokumentu"),
                new AnyReferenceNamingWidget("f:author"))),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Authenticator),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Ověřil(a)"),
                    new AnyReferenceNamingWidget("f:authenticator")))
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Custodian),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Správce dokumentu"),
                    new AnyReferenceNamingWidget("f:custodian")))
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.RelatesTo),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Souvisejíci záznam"),
                    new ItemListBuilder("f:relatesTo/f:target", ItemListType.Unordered,
                        _ => [new AnyReferenceNamingWidget()])))
            ),
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
        Author, //0..*	Reference(Practitioner | PractitionerRole | Organization | Device | Patient | RelatedPerson)	Who and/or what authored the document
        Authenticator, //0..1	Reference(Practitioner | PractitionerRole | Organization)	Who/what authenticated the document
        Custodian, //0..1	Reference(Organization)	Organization which maintains the document
        RelatesTo, /*0..*	BackboneElement	Relationships to other documents*/
    }
}