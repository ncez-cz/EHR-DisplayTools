using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert.RiskAssessment;

public class RiskAssessments(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "RiskAssessment";
    [UsedImplicitly]
    public static bool RequiresExternalTitle => true;
    
    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new RiskAssessments(items)];
    }
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = Widgets.InfrequentProperties.Evaluate<InfrequentProperties>(items);

        var infrequentPredictionProperties =
            Widgets.InfrequentProperties.Evaluate<InfrequentPredictionProperties>(
                items.SelectMany(x => x
                        .SelectAllNodes("f:prediction"))
                    .ToList()
            );

        var table = new Table(
            [
                new TableHead([
                    new TableRow([
                        new If(_ => infrequentPredictionProperties.Count != 0,
                            new TableCell([new ConstantText("Predikce")], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.HasAnyOfGroup("BasicInfoCell"),
                            new TableCell([new ConstantText("Základní informace")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentProperties.HasAnyOfGroup("ActorsCell"),
                            new TableCell([new ConstantText("Zainteresované strany")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentProperties.HasAnyOfGroup("AdditionalInfoCell"),
                            new TableCell([new ConstantText("Doplňujíci informace")], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(InfrequentProperties.Status),
                            new TableCell([new DisplayLabel(LabelCodes.Status)], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(InfrequentProperties.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        )
                    ])
                ]),
                ..items.Select(x => new TableBody([
                    new RiskAssessmentRowBuilder(x, infrequentProperties, infrequentPredictionProperties)
                ])),
            ],
            true
        );

        return table.Render(navigator, renderer, context);
    }

    public enum InfrequentProperties
    {
        [Group("BasicInfoCell")] Condition,
        [Group("BasicInfoCell")] Code,
        [Group("BasicInfoCell")] Method,
        [Group("BasicInfoCell")] ReasonCode,
        [Group("BasicInfoCell")] ReasonReference,

        [Group("ActorsCell")] Performer,
        [Group("ActorsCell")] Parent,
        [Group("ActorsCell")] Subject,
        [Group("ActorsCell")] BasedOn,

        [Group("AdditionalInfoCell")] Identifier,
        [Group("AdditionalInfoCell")] Id,
        [Group("AdditionalInfoCell")] Occurence,
        [Group("AdditionalInfoCell")] Basis,
        [Group("AdditionalInfoCell")] Mitigation,
        [Group("AdditionalInfoCell")] Note,

        Encounter,
        Text,

        [EnumValueSet("http://hl7.org/fhir/observation-status")]
        Status
    }

    public enum InfrequentPredictionProperties
    {
        Outcome,
        Probability,
        QualitativeRisk,
        RelativeRisk,
        When,
        Rationale
    }
}