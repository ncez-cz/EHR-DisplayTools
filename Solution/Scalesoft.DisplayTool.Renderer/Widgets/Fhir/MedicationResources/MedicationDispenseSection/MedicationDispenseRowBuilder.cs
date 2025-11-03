using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert.DetectedIssues;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using ReferenceHandler = Scalesoft.DisplayTool.Renderer.Utils.ReferenceHandler;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicationResources.MedicationDispenseSection;

public class MedicationDispenseRowBuilder(
    XmlDocumentNavigator item,
    InfrequentPropertiesData<MedicationDispense.InfrequentPropertiesPaths> infrequentProperties,
    InfrequentPropertiesData<Dosage.InfrequentPropertiesPaths> dosageInfrequentProperties
) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var rowDetails = new StructuredDetails();

        if (item.EvaluateCondition("f:detectedIssue"))
        {
            var detectedIssueNavigator = ReferenceHandler.GetContentFromReferences(item, "f:detectedIssue");
            if (detectedIssueNavigator.Any())
            {
                rowDetails.AddCollapser(
                    new ConstantText("Zjištěné problémy"),
                    new DetectedIssue(detectedIssueNavigator)
                );
            }
        }

        if (item.EvaluateCondition("f:context"))
        {
            rowDetails.AddCollapser(new ConstantText("Kontext"),
                ShowSingleReference.WithDefaultDisplayHandler(x => [new AnyResource(x, displayResourceType: false)], "f:context"));
        }

        if (item.EvaluateCondition("f:text"))
        {
            rowDetails.AddCollapser(new DisplayLabel(LabelCodes.OriginalNarrative), new Narrative("f:text"));
        }

        Widget tree = new TableRow([
            new If(_ => infrequentProperties.HasAnyOfGroup("MedicationCell"),
                new MedicationDispenseMedicationCell(item)
            ),
            new If(_ => dosageInfrequentProperties.Count != 0,
                new TableCell(
                    [new Dosage(item)]
                )
            ),
            new If(_ => infrequentProperties.HasAnyOfGroup("ActorCell"),
                new MedicationDispenseActorCell(item)
            ),
            new If(_ => infrequentProperties.HasAnyOfGroup("AdditionalInfoCell"),
                new MedicationDispenseAdditionalInfoCell(item)
            ),
            new If(_ => infrequentProperties.Contains(MedicationDispense.InfrequentPropertiesPaths.Status),
                new TableCell([
                    new EnumIconTooltip("f:status", "http://terminology.hl7.org/CodeSystem/medicationdispense-status",
                        new DisplayLabel(LabelCodes.Status))
                ])
            ),
            new If(_ => infrequentProperties.Contains(MedicationDispense.InfrequentPropertiesPaths.Text),
                new NarrativeCell()
            )
        ], rowDetails, idSource: item);

        var result = await tree.Render(item, renderer, context);

        var isStatus = item.EvaluateCondition("f:status");
        if (!isStatus)
        {
            result.Errors.Add(ParseError.MissingValue(item.SelectSingleNode("f:status").GetFullPath()));
        }

        var isMedication = item.EvaluateCondition("f:medication");
        if (!isMedication)
        {
            result.Errors.Add(ParseError.MissingValue(item.SelectSingleNode("f:medication").GetFullPath()));
        }

        return result;
    }
}