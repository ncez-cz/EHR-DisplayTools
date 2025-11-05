using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.FamilyMemberHistory;

public class FamilyMemberAdditionalInfoCell(XmlDocumentNavigator item) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions =
            InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([item]);

        var familyMemberTableCell = new TableCell(
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
                                new Optional("f:id", new Text("@value")),
                            ])
                            : new ConstantText("Identifikátor podání není specifikován")
                ),
                infrequentOptions.Contains(InfrequentPropertiesPaths.ReasonCode)
                    ? new NameValuePair([new ConstantText("Důvod provedení")],
                    [
                        new ItemListBuilder("f:reasonCode", ItemListType.Unordered, _ => [new CodeableConcept()]),
                    ])
                    : new NullWidget(),
                infrequentOptions.Contains(InfrequentPropertiesPaths.ReasonReference)
                    ? new NameValuePair([new ConstantText("Související záznam")],
                    [
                        new ItemListBuilder("f:reasonReference", ItemListType.Unordered, _ =>
                        [
                            new Optional(".", new AnyReferenceNamingWidget()),
                        ])
                    ])
                    : new NullWidget(),
                infrequentOptions.Contains(InfrequentPropertiesPaths.Date)
                    ? new NameValuePair([new ConstantText("Datum změny")],
                    [
                        new ShowDateTime("f:date"),
                    ])
                    : new NullWidget(),
            ], optionalClass: "name-value-pair-wrapper"),
        ]);
        if (infrequentOptions.Count == 0)
        {
            familyMemberTableCell = new TableCell([
                new TextContainer(TextStyle.Muted, [new ConstantText("Informace nejsou k dispozici")])
            ]);
        }

        return familyMemberTableCell.Render(item, renderer, context);
    }

    private enum InfrequentPropertiesPaths
    {
        /*Participant, //	0..*	BackboneElement	Members of the team*/
        Id,
        Identifier, //	0..*	Identifier	External Id(s) for this record
        Date, //0..1	dateTime	When history was recorded or last updated
        ReasonCode, //	0..*	CodeableConcept	Why was family member history performed?
        ReasonReference, //0..*	Reference(Condition | Observation | AllergyIntolerance | QuestionnaireResponse | DiagnosticReport | DocumentReference) Why was family member history performed?
    }
}