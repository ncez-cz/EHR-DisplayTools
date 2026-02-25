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
    public static bool HasBorderedContainer(Widget widget) => true;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new AllergiesAndIntolerances(items)];
    }

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator navigator)
    {
        Widget? label = null;

        if (navigator.EvaluateCondition("f:type"))
        {
            label = new ChangeContext(navigator,
                new EnumLabel("f:type/@value", "http://hl7.org/fhir/ValueSet/allergy-intolerance-type"));
        }

        if (navigator.EvaluateCondition("f:code"))
        {
            return new ResourceSummaryModel
            {
                Label = label,
                Value = new ChangeContext(navigator, "f:code", new CodeableConcept()),
            };
        }

        if (navigator.EvaluateCondition("f:category"))
        {
            return new ResourceSummaryModel
            {
                Label = label,
                Value = new ChangeContext(navigator,
                    new EnumLabel("f:category/@value", "http://hl7.org/fhir/allergy-intolerance-category")),
            };
        }

        if (navigator.EvaluateCondition("f:reaction"))
        {
            var reactionNode = navigator.SelectSingleNode("f:reaction");

            if (reactionNode.EvaluateCondition("f:substance"))
            {
                return new ResourceSummaryModel
                {
                    Label = label,
                    Value = new ChangeContext(reactionNode, "f:substance", new CodeableConcept()),
                };
            }

            if (reactionNode.EvaluateCondition("f:manifestation"))
            {
                return new ResourceSummaryModel
                {
                    Label = label,
                    Value = new ChangeContext(reactionNode, "f:manifestation", new CodeableConcept()),
                };
            }
        }

        return null;
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
                            new TableCell([new EhdsiDisplayLabel(LabelCodes.ReactionType)], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(AllergiesAndIntolerancesInfrequentProperties.Code),
                            new TableCell([new EhdsiDisplayLabel(LabelCodes.Agent)], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentProperties.Contains(AllergiesAndIntolerancesInfrequentProperties
                                .Criticality),
                            new TableCell([new LocalizedLabel("allergy-intolerance.criticality")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentReactionProperties.Contains(
                                AllergiesAndIntolerancesReactionInfrequentProperties.Manifestation),
                            new TableCell(
                                [
                                    new EhdsiDisplayLabel(LabelCodes.ClinicalManifestation), new ConstantText(" ("),
                                    new EhdsiDisplayLabel(LabelCodes.Severity), new ConstantText(")")
                                ],
                                TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(AllergiesAndIntolerancesInfrequentProperties.Onset),
                            new TableCell([new EhdsiDisplayLabel(LabelCodes.DiagnosticDate)],
                                TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentProperties.ContainsAnyOf(
                                AllergiesAndIntolerancesInfrequentProperties.Abatement,
                                AllergiesAndIntolerancesInfrequentProperties.AbatementDateTime),
                            new TableCell([new LocalizedLabel("allergy-intolerance.abatement")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentProperties.ContainsAnyOf(
                                AllergiesAndIntolerancesInfrequentProperties.ClinicalStatus,
                                AllergiesAndIntolerancesInfrequentProperties.VerificationStatus),
                            new TableCell([new LocalizedLabel("general.other")], TableCellType.Header)
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