using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.CareTeam;

public class CareTeamsBasicInfoCell(XmlDocumentNavigator item) : Widget
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
                infrequentOptions.Contains(InfrequentPropertiesPaths.Category)
                    ? new NameValuePair([new LocalizedLabel("care-team.category")],
                    [
                        new Optional("f:category", new CommaSeparatedBuilder(".", _ => new CodeableConcept())),
                    ])
                    : new NullWidget(),
                infrequentOptions.Contains(InfrequentPropertiesPaths.Name)
                    ? new NameValuePair([new EhdsiDisplayLabel(LabelCodes.Name)],
                    [
                        new Optional("f:name", new Text("@value")),
                    ])
                    : new NullWidget(),
                infrequentOptions.Contains(InfrequentPropertiesPaths.Period)
                    ? new NameValuePair([new EhdsiDisplayLabel(LabelCodes.Duration)],
                    [
                        new ShowPeriod("f:period"),
                    ])
                    : new NullWidget(),
                infrequentOptions.Contains(InfrequentPropertiesPaths.Telecom)
                    ? new NameValuePair([new LocalizedLabel("care-team.telecom")],
                    [
                        new ItemListBuilder(".", ItemListType.Unordered, _ => [new ShowContactPoint()]),
                    ])
                    : new NullWidget()
            ], optionalClass: "name-value-pair-wrapper row-gap-1"),
        ]);

        if (infrequentOptions.Count == 0)
        {
            participantTableCell = new TableCell([
                new TextContainer(TextStyle.Muted, [new LocalizedLabel("general.information-unavailable")])
            ]);
        }

        return participantTableCell.Render(item, renderer, context);
    }

    private enum InfrequentPropertiesPaths
    {
        Status, //0..1	code	proposed | active | suspended | inactive | entered-in-error
        Category, //0..*	CodeableConcept	Type of team
        Name, //	0..1	string	Name of the team, such as crisis assessment team
        Period, //	0..1	Period	Time period team covers
        Telecom, //	0..*	ContactPoint	A contact detail for the care team (that applies to all members)
    }
}