using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Consent;

public class Consents(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "Consent";

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new Consents(items)];
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<ConsentInfrequentProperties>(items);

        var table =
            new Table([
                new TableHead([
                    new TableRow([
                        new TableCell([new ConstantText("Kategorie")], TableCellType.Header),
                        new TableCell([new ConstantText("Rozsah")], TableCellType.Header),
                        new If(
                            _ => infrequentProperties.ContainsAnyOf(ConsentInfrequentProperties.Policy,
                                ConsentInfrequentProperties.PolicyRule),
                            new TableCell([new ConstantText("Zásady")], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(ConsentInfrequentProperties.Status),
                            new TableCell([new DisplayLabel(LabelCodes.Status)], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(ConsentInfrequentProperties.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        ),
                    ])
                ]),
                new TableBody(
                    [..items.Select(x => new ConsentTableRow(x, infrequentProperties))]
                )
            ], true);

        return await table.Render(navigator, renderer, context);
    }

    public enum ConsentInfrequentProperties
    {
        Policy,
        PolicyRule,
        Text,

        [EnumValueSet("http://hl7.org/fhir/consent-state-codes")]
        Status
    }
}