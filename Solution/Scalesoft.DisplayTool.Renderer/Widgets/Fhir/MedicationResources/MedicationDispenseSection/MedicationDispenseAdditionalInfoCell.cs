using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicationResources.MedicationDispenseSection;

public class MedicationDispenseAdditionalInfoCell(XmlDocumentNavigator item) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions = InfrequentProperties.Evaluate<InfrequentPropertiesPaths>([item]);

        var supportingInfoTableCell = new TableCell(
        [
            new Container([
                new HideableDetails(
                infrequentOptions.Contains(InfrequentPropertiesPaths.Identifier)
                    ? new NameValuePair([new ConstantText("Identifikátor podáni")],
                    [
                        new CommaSeparatedBuilder("f:identifier", _ => [new ShowIdentifier()])
                    ])
                    : infrequentOptions.Contains(InfrequentPropertiesPaths.Id)
                        ? new NameValuePair([new ConstantText("Technický identifikátor podani")],
                        [
                            new Optional("f:id", new Text("@value"))
                        ])
                        : new ConstantText("Identifikátor podání není specifikován")
            ),
            infrequentOptions.Contains(InfrequentPropertiesPaths.StatusReasonCodeableConcept)
                ? new NameValuePair([new ConstantText("Důvod stavu")],
                [
                    new CommaSeparatedBuilder("f:statusReasonCodeableConcept", _ => [new CodeableConcept()])
                ])
                : new NullWidget(),
            infrequentOptions.Contains(InfrequentPropertiesPaths.StatusReasonReference)
                ? new NameValuePair([new ConstantText("Důvod stavu dle reference")],
                [
                    new CommaSeparatedBuilder("f:statusReasonReference", _ => [new AnyReferenceNamingWidget()])
                ])
                : new NullWidget(),
            infrequentOptions.Contains(InfrequentPropertiesPaths.Category)
                ? new NameValuePair([new ConstantText("Kategorie")],
                [
                    new Optional("f:category", new CodeableConcept())
                ])
                : new ConstantText(""),
            infrequentOptions.Contains(InfrequentPropertiesPaths.Type)
                ? new NameValuePair([new ConstantText("Typ")],
                [
                    new Optional("f:type", new CodeableConcept())
                ])
                : new NullWidget(),
            infrequentOptions.Contains(InfrequentPropertiesPaths.PartOf)
                ? new NameValuePair([new ConstantText("Související úkony")],
                [
                    new CommaSeparatedBuilder("f:partOf", _ => [new AnyReferenceNamingWidget()])
                ])
                : new NullWidget(),
            infrequentOptions.Contains(InfrequentPropertiesPaths.Note)
                ? new NameValuePair([new ConstantText("Poznámka")],
                [
                    new CommaSeparatedBuilder("f:note", _ => [new Optional("f:text", new Text("@value"))])
                ])
                : new NullWidget()
            ], optionalClass: "name-value-pair-wrapper"),
        ]);

        if (infrequentOptions.Count == 0)
        {
            supportingInfoTableCell = new TableCell([
                new TextContainer(TextStyle.Muted, [new ConstantText("Informace nejsou k dispozici")]),
            ]);
        }

        return supportingInfoTableCell.Render(item, renderer, context);
    }

    private enum InfrequentPropertiesPaths
    {
        Id,
        Identifier,
        Category,
        Type,
        PartOf,
        Note,
        StatusReasonCodeableConcept,
        StatusReasonReference
    }
}