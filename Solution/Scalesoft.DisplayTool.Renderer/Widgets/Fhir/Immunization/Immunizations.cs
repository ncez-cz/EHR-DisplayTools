using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Immunization;

public class Immunizations(List<XmlDocumentNavigator> items, bool skipIdPopulation = false) : Widget, IResourceWidget
{
    public static string ResourceType => "Immunization";

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new Immunizations(items)];
    }
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentOptions = InfrequentProperties.Evaluate<InfrequentPropertiesPaths>(items);


        List<XmlDocumentNavigator> protocols = [];
        foreach (var item in items)
        {
            protocols.AddRange(item.SelectAllNodes("f:protocolApplied").ToList());
        }

        var infrequentProtocolOptions =
            InfrequentProperties
                .Evaluate<InfrequentProtocolPropertiesPaths>(protocols);

        List<Widget> headerRow =
        [
            new TableCell([new DisplayLabel(LabelCodes.Vaccination)], TableCellType.Header),
            new TableCell([new DisplayLabel(LabelCodes.VaccinationDate)],
                TableCellType.Header),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.ExpirationDate),
                new TableCell([new ConstantText("Datum expirace")],
                    TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Route),
                new TableCell([new DisplayLabel(LabelCodes.AdministrationRoute)], TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Site),
                new TableCell([new DisplayLabel(LabelCodes.BodySite)], TableCellType.Header)
            ),
            new If(_ => infrequentProtocolOptions.Contains(InfrequentProtocolPropertiesPaths.TargetDisease),
                new TableCell([new DisplayLabel(LabelCodes.Agent)],
                    TableCellType.Header)
            ),
            new If(_ => infrequentProtocolOptions.Contains(InfrequentProtocolPropertiesPaths.DoseNumber),
                new TableCell([new DisplayLabel(LabelCodes.DoseNumber)],
                    TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.LotNumber),
                new TableCell([new DisplayLabel(LabelCodes.LotNumber)], TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Location),
                new TableCell([new DisplayLabel(LabelCodes.AdministeringCenter)], TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Performer),
                new TableCell([new DisplayLabel(LabelCodes.Performer)],
                    TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.IsSubpotent),
                new TableCell([new ConstantText("Je subpotentní")], TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Status),
                new TableCell([new DisplayLabel(LabelCodes.Administered)],
                    TableCellType.Header)
            ),
            new If(_ => infrequentOptions.Contains(InfrequentPropertiesPaths.Text),
                new NarrativeCell(false, TableCellType.Header)
            ),
        ];

        // implicitRules just contains a URL to a set of rules, and has little value to the end user 

        var tree = new Table(
            [
                new TableHead([
                    new TableRow(headerRow)
                ]),
                ..items.Select(x =>
                    new ImmunizationsBuilder(x, infrequentOptions, skipIdPopulation, infrequentProtocolOptions)),
            ],
            true
        );

        return tree.Render(navigator, renderer, context);
    }

    public enum InfrequentPropertiesPaths
    {
        ImplicitRules,
        Manufacturer,
        IsSubpotent,

        [PropertyPath("f:extension[@url='http://hl7.org/fhir/5.0/StructureDefinition/extension-Immunization.basedOn']")]
        BasedOnExtension,

        [PropertyPath(
            "f:extension[@url='http://hl7.eu/fhir/hdr/StructureDefinition/immunization-administeredProduct']")]
        AdministeredProductExtension,

        ExpirationDate,
        Route,
        Site,
        LotNumber,
        Location,

        [PropertyPath("f:performer[not(f:function/f:coding/f:code/@value='OP')]")]
        Performer,

        [EnumValueSet("http://hl7.org/fhir/ValueSet/immunization-status")]
        Status,

        Text
    }

    public enum InfrequentProtocolPropertiesPaths
    {
        TargetDisease,
        [OpenType("doseNumber")] DoseNumber
    }
}