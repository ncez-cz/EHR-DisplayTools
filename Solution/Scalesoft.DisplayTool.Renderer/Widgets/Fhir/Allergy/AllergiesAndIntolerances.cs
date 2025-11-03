using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Allergy;

public class AllergiesAndIntolerances(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "AllergyIntolerance";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new AllergiesAndIntolerances(items)];
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<AllergiesAndIntolerancesInfrequentProperties>(items);

        List<XmlDocumentNavigator> reactions = [];
        foreach (var item in items)
        {
            reactions.AddRange(item.SelectAllNodes("f:reaction").ToList());
        }

        var infrequentReactionProperties =
            InfrequentProperties
                .Evaluate<AllergiesAndIntolerancesReactionInfrequentProperties>(reactions);

        var tree = new Table(
            [
                new TableHead([
                    new TableRow([
                        new If(_ => infrequentProperties.Contains(AllergiesAndIntolerancesInfrequentProperties.Type),
                            new TableCell([new DisplayLabel(LabelCodes.ReactionType)], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentReactionProperties.Contains(
                                AllergiesAndIntolerancesReactionInfrequentProperties.Manifestation),
                            new TableCell([new DisplayLabel(LabelCodes.ClinicalManifestation)],
                                TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(AllergiesAndIntolerancesInfrequentProperties.Code),
                            new TableCell([new DisplayLabel(LabelCodes.Agent)], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(AllergiesAndIntolerancesInfrequentProperties.Onset),
                            new TableCell([new DisplayLabel(LabelCodes.DiagnosticDate)],
                                TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentProperties.ContainsAnyOf(
                                AllergiesAndIntolerancesInfrequentProperties.Abatement,
                                AllergiesAndIntolerancesInfrequentProperties.AbatementDateTime),
                            new TableCell([new ConstantText("Datum vyřešení/remise")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentReactionProperties.ContainsAnyOf(
                                AllergiesAndIntolerancesReactionInfrequentProperties
                                    .Severity) || infrequentProperties.ContainsAnyOf(
                                AllergiesAndIntolerancesInfrequentProperties.Criticality,
                                AllergiesAndIntolerancesInfrequentProperties.ClinicalStatus,
                                AllergiesAndIntolerancesInfrequentProperties.VerificationStatus),
                            new TableCell([new ConstantText("Další")], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(AllergiesAndIntolerancesInfrequentProperties.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        )
                    ])
                ]),
                ..items.Select(x =>
                    new AllergyIntoleranceBuilder(x, infrequentProperties, infrequentReactionProperties)),
            ],
            true
        );

        return tree.Render(navigator, renderer, context);
    }
}

public enum AllergiesAndIntolerancesInfrequentProperties
{
    Type,
    Code,
    [OpenType("onset")] Onset,

    [Extension("http://hl7.org/fhir/StructureDefinition/allergyintolerance-abatement")]
    Abatement,

    [Extension("http://hl7.org/fhir/uv/ips/StructureDefinition/abatement-dateTime-uv-ips")]
    AbatementDateTime,

    [EnumValueSet("http://hl7.org/fhir/allergy-intolerance-criticality")]
    Criticality,
    [EnumValueSet("")] ClinicalStatus,
    [EnumValueSet("")] VerificationStatus,
    Text
}

public enum AllergiesAndIntolerancesReactionInfrequentProperties
{
    Manifestation,
    Severity,
}