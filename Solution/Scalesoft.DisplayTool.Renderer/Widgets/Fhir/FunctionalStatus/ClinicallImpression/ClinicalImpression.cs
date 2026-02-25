using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.FunctionalStatus.ClinicallImpression;

public class ClinicalImpression(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "ClinicalImpression";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public static bool HasBorderedContainer(Widget widget) => true;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new ClinicalImpression(items)];
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<ClinicalImpressionInfrequentProperties>(items);

        var table = new Table(
            [
                new TableHead([
                    new TableRow([
                        new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Code),
                            new TableCell([new EhdsiDisplayLabel(LabelCodes.FunctionalAssessment)], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Effective),
                            new TableCell([new EhdsiDisplayLabel(LabelCodes.FunctionalAssessmentDate)], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Date),
                            new TableCell([new EhdsiDisplayLabel(LabelCodes.OnsetDate)], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Summary),
                            new TableCell([new EhdsiDisplayLabel(LabelCodes.FunctionalAssessmentResult)],
                                TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Summary),
                            new TableCell([new LocalizedLabel("general.related-condition")], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Status),
                            new TableCell([new EhdsiDisplayLabel(LabelCodes.Status)], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(ClinicalImpressionInfrequentProperties.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        ),
                    ])
                ]),
                ..items.Select(x => new TableBody([new ClinicalImpressionBuilder(x, infrequentProperties)])),
            ],
            true
        );
        return table.Render(navigator, renderer, context);
    }
}

public enum ClinicalImpressionInfrequentProperties
{
    Code,
    [OpenType("effective")] Effective,
    Date,
    Summary,
    Problem,
    Text,

    [EnumValueSet("http://hl7.org/fhir/event-status")]
    Status,
}