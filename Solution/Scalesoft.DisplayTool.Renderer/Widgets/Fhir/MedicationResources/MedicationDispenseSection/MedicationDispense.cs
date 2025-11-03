using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicationResources.MedicationDispenseSection;

public class MedicationDispense(List<XmlDocumentNavigator> items) : Widget, IResourceWidget
{
    public static string ResourceType => "MedicationDispense";
    [UsedImplicitly]
    public static bool RequiresExternalTitle => true;

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new MedicationDispense(items)];
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<InfrequentPropertiesPaths>(items);

        var dosageInstructionInfrequentProperties =
            new InfrequentPropertiesData<Dosage.InfrequentPropertiesPaths>();
        List<XmlDocumentNavigator> dosageInstructionNodes = [];

        foreach (var item in items)
        {
            dosageInstructionNodes.AddRange(item.SelectAllNodes("f:dosageInstruction").ToList());
        }

        if (dosageInstructionNodes.Count != 0)
        {
            dosageInstructionInfrequentProperties =
                InfrequentProperties.Evaluate<Dosage.InfrequentPropertiesPaths>(
                    dosageInstructionNodes);
        }

        var table = new Table(
            [
                new TableHead([
                    new TableRow([
                        new If(_ => infrequentProperties.HasAnyOfGroup("MedicationCell"),
                            new TableCell([new DisplayLabel(LabelCodes.MedicalProduct)], TableCellType.Header)
                        ),
                        new If(_ => dosageInstructionInfrequentProperties.Count != 0,
                            new TableCell([new ConstantText("Informace o dávkování")], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.HasAnyOfGroup("ActorCell"),
                            new TableCell([new ConstantText("Zainteresované strany")], TableCellType.Header)
                        ),
                        new If(
                            _ => infrequentProperties.HasAnyOfGroup("AdditionalInfoCell"),
                            new TableCell([new ConstantText("Doplňujíci informace")], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(InfrequentPropertiesPaths.Status),
                            new TableCell([new DisplayLabel(LabelCodes.Status)], TableCellType.Header)
                        ),
                        new If(_ => infrequentProperties.Contains(InfrequentPropertiesPaths.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        )
                    ])
                ]),
                ..items.Select(x => new TableBody([
                    new MedicationDispenseRowBuilder(x, infrequentProperties, dosageInstructionInfrequentProperties)
                ])),
            ],
            true
        );

        return table.Render(navigator, renderer, context);
    }

    public enum InfrequentPropertiesPaths
    {
        [Group("MedicationCell")] Quantity,
        [Group("MedicationCell")] DaysSupply,
        [Group("MedicationCell")] WhenPrepared,
        [Group("MedicationCell")] WhenHandedOver,
        [Group("MedicationCell")] MedicationCodeableConcept,
        [Group("MedicationCell")] MedicationReference,

        [Group("ActorCell")] Performer,
        [Group("ActorCell")] AuthorizingPrescription,

        [Group("AdditionalInfoCell")] Id,
        [Group("AdditionalInfoCell")] Identifier,
        [Group("AdditionalInfoCell")] Category,
        [Group("AdditionalInfoCell")] Type,
        [Group("AdditionalInfoCell")] PartOf,
        [Group("AdditionalInfoCell")] Note,
        [Group("AdditionalInfoCell")] StatusReasonCodeableConcept,
        [Group("AdditionalInfoCell")] StatusReasonReference,

        [EnumValueSet("http://terminology.hl7.org/CodeSystem/medicationdispense-status")]
        Status,

        Text
    }
}