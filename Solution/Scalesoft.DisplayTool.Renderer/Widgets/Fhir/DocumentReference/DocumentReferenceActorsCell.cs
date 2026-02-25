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
            InfrequentProperties.Evaluate<InfrequentPropertiesPaths>(item);

        var participantTableCell = new TableCell(
        [
            new Container([
                infrequentOptions.Optional(InfrequentPropertiesPaths.Author,
                    new AnyReferenceNamingWidget(
                        widgetModel: new ReferenceNamingWidgetModel
                        {
                            Type = ReferenceNamingWidgetType.NameValuePair,
                            LabelOverride = new LocalizedLabel("document-reference.author"),
                        }
                    )
                ),
                infrequentOptions.Optional(InfrequentPropertiesPaths.Authenticator,
                    new HideableDetails(
                        new AnyReferenceNamingWidget(
                            widgetModel: new ReferenceNamingWidgetModel
                            {
                                Type = ReferenceNamingWidgetType.NameValuePair,
                                LabelOverride = new LocalizedLabel("document-reference.authenticator"),
                            }
                        )
                    )
                ),
                infrequentOptions.Optional(InfrequentPropertiesPaths.Custodian,
                    new HideableDetails(
                        new AnyReferenceNamingWidget(
                            widgetModel: new ReferenceNamingWidgetModel
                            {
                                Type = ReferenceNamingWidgetType.NameValuePair,
                                LabelOverride = new LocalizedLabel("document-reference.custodian"),
                            }
                        )
                    )
                ),
                infrequentOptions.Condition(InfrequentPropertiesPaths.RelatesTo,
                    new HideableDetails(
                        new NameValuePair(
                            new LocalizedLabel("document-reference.relatesTo"),
                            new ItemListBuilder("f:relatesTo/f:target", ItemListType.Unordered,
                                _ => [new AnyReferenceNamingWidget()]
                            )
                        )
                    )
                ),
            ], optionalClass: "name-value-pair-wrapper w-fit-content"),
        ]);

        if (infrequentOptions.Count == 0)
        {
            participantTableCell = new TableCell([
                new TextContainer(TextStyle.Muted, [new LocalizedLabel("general.information-unavailable")]),
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