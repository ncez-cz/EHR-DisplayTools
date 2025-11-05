using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.CareTeam;

public class CareTeamsAdditionalInfoCell(XmlDocumentNavigator item) : Widget
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
            new Container([
                new HideableDetails(
                    infrequentOptions.Contains(InfrequentPropertiesPaths.Identifier)
                        ? new NameValuePair([new ConstantText("Identifikátor týmu")],
                        [
                            new CommaSeparatedBuilder("f:identifier", _ => [new ShowIdentifier()]),
                        ])
                        : infrequentOptions.Contains(InfrequentPropertiesPaths.Id)
                            ? new NameValuePair([new ConstantText("Technický identifikátor týmu")],
                            [
                                new Optional("f:id", new Text("@value"))
                            ])
                            : new ConstantText("Identifikátor podání není specifikován")
                ),
                infrequentOptions.Contains(InfrequentPropertiesPaths.ReasonCode)
                    ? new NameValuePair([new ConstantText("Účel týmu")],
                    [
                        new CommaSeparatedBuilder("f:reasonCode", _ => [new CodeableConcept()]),
                    ])
                    : new NullWidget(),
                infrequentOptions.Contains(InfrequentPropertiesPaths.ManagingOrganization)
                    ? new NameValuePair([new ConstantText("Odpovědná organizace")],
                    [
                        new CommaSeparatedBuilder("f:managingOrganization", _ => [new AnyReferenceNamingWidget()]),
                    ])
                    : new NullWidget(),
                infrequentOptions.Contains(InfrequentPropertiesPaths.Note)
                    ? new NameValuePair([new ConstantText("Komentář")],
                    [
                        new ItemListBuilder("f:note", ItemListType.Unordered, _ => [new ShowAnnotationCompact()]),
                    ])
                    : new NullWidget()
            ], optionalClass: "name-value-pair-wrapper"),
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
        Identifier, //0..*	Identifier	External Ids for this team
        Id,
        ReasonCode, //	0..*	CodeableConcept	Why the care team exists
        ManagingOrganization, //0..*	Reference(Organization)	Organization responsible for the care team
        Note //0..*	Annotation	Comments made about the CareTeam
    }
}