using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert.DetectedIssues;

public class DetectedIssue(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "DetectedIssue";

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new DetectedIssue(items)];
    }
    
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            Widgets.InfrequentProperties.Evaluate<InfrequentProperties>(items);

        var mitigationNavigators = items
            .Select(x => x
                .SelectAllNodes("f:mitigation"))
            .SelectMany(x => x)
            .ToList();

        var infrequentMitigationProperties =
            Widgets.InfrequentProperties.Evaluate<InfrequentMitigationProperties>(mitigationNavigators);

        var table = new Table(
            [
                new TableHead([
                    new TableRow([
                        new If(
                            _ => infrequentProperties.HasAnyOfGroup("ProblemDetailCell"),
                            new TableCell([new ConstantText("Detail Problému")], TableCellType.Header)
                        ),
                        new If(_ => infrequentMitigationProperties.Count != 0,
                            new TableCell([new ConstantText("Opatření")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentProperties.HasAnyOfGroup("ActorsCell"),
                            new TableCell([new ConstantText("Zainteresované strany")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentProperties.HasAnyOfGroup("AdditionalInfoCell"),
                            new TableCell([new ConstantText("Doplňujíci informace")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentProperties.ContainsAnyOf(InfrequentProperties.Status,
                                InfrequentProperties.Severity),
                            new TableCell([new ConstantText("Další")], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(InfrequentProperties.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        )
                    ])
                ]),
                ..items.Select(x => new TableBody([
                    new DetectedIssuesRowBuilder(x, infrequentProperties, infrequentMitigationProperties)
                ])),
            ],
            true
        );


        var result = await table.Render(navigator, renderer, context);
        var isStatus = navigator.EvaluateCondition("f:status");
        if (!isStatus)
        {
            result.Errors.Add(ParseError.MissingValue(navigator.SelectSingleNode("f:status").GetFullPath()));
        }

        return result;
    }

    public enum InfrequentProperties
    {
        [Group("ProblemDetailCell")] Code,
        [Group("ProblemDetailCell")] Detail,
        [Group("ProblemDetailCell")] Implicated,

        [Group("ActorsCell")] Patient,
        [Group("ActorsCell")] Author,
        [Group("ActorsCell")] Reference,

        [Group("AdditionalInfoCell")] Identifier,
        [Group("AdditionalInfoCell")] Id,
        [Group("AdditionalInfoCell")] IdentifiedDateTime,
        [Group("AdditionalInfoCell")] IdentifiedPeriod,
        [Group("AdditionalInfoCell")] Evidence,

        Text,

        [EnumValueSet("http://hl7.org/fhir/observation-status")]
        Status,

        [EnumValueSet("https://hl7.org/fhir/R4/valueset-detectedissue-severity.html")]
        Severity
    }

    public enum InfrequentMitigationProperties
    {
        Action,
        Date,
        Author,
    }
}