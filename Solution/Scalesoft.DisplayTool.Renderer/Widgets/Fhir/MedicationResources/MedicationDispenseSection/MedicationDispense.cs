using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicationResources.MedicationDispenseSection;

public class MedicationDispense(bool skipIdPopulation = true)
    : AlternatingBackgroundColumnResourceBase<MedicationDispense>, IResourceWidget
{
    public MedicationDispense() : this(true)
    {
    }

    public static string ResourceType => "MedicationDispense";

    public static bool HasBorderedContainer(Widget resourceWidget) => false;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<MedicationDispenseInfrequentProperties>(navigator);

        var nameValuePairs = new FlexList([
            infrequentProperties.Optional(MedicationDispenseInfrequentProperties.Quantity,
                new NameValuePair([new LocalizedLabel("medication-dispense.quantity")],
                    [
                        new ShowQuantity(),
                    ], direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary)),
            infrequentProperties.Optional(MedicationDispenseInfrequentProperties.DaysSupply,
                new NameValuePair([new LocalizedLabel("medication-dispense.daysSupply")],
                    [
                        new ShowQuantity(),
                    ], direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary)),
            infrequentProperties.Optional(MedicationDispenseInfrequentProperties.WhenPrepared,
                new NameValuePair([new LocalizedLabel("medication-dispense.whenPrepared")],
                    [
                        new ShowDateTime(),
                    ], direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary)),
            infrequentProperties.Optional(MedicationDispenseInfrequentProperties.WhenHandedOver,
                new NameValuePair([new LocalizedLabel("medication-dispense.whenHandedOver")],
                    [
                        new ShowDateTime(),
                    ], direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary)),
            infrequentProperties.Condition(MedicationDispenseInfrequentProperties.Performer,
                new NameValuePair([new LocalizedLabel("medication-dispense.performer")],
                    [
                        new CommaSeparatedBuilder("f:performer",
                            _ =>
                            [
                                new AnyReferenceNamingWidget("f:actor")
                            ])
                    ], direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary)),
            infrequentProperties.Condition(MedicationDispenseInfrequentProperties.AuthorizingPrescription,
                new NameValuePair(
                    [new LocalizedLabel("medication-dispense.authorizingPrescription")],
                    [
                        new CommaSeparatedBuilder("f:authorizingPrescription", _ => [new AnyReferenceNamingWidget()])
                    ], direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary)),
            infrequentProperties.Condition(MedicationDispenseInfrequentProperties.StatusReason, new NameValuePair(
                [new LocalizedLabel("medication-dispense.statusReason")],
                [
                    new OpenTypeElement(null, "statusReason"),
                ], direction: FlexDirection.Column,
                style: NameValuePair.NameValuePairStyle.Primary)),
            infrequentProperties.Optional(MedicationDispenseInfrequentProperties.Category,
                new NameValuePair([new LocalizedLabel("medication-dispense.category")],
                    [
                        new CodeableConcept(),
                    ], direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary)),
            infrequentProperties.Optional(MedicationDispenseInfrequentProperties.Type,
                new NameValuePair([new LocalizedLabel("medication-dispense.type")],
                    [
                        new CodeableConcept(),
                    ], direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary)),
            infrequentProperties.Condition(MedicationDispenseInfrequentProperties.PartOf,
                new NameValuePair([new LocalizedLabel("medication-dispense.partOf")],
                    [
                        new CommaSeparatedBuilder("f:partOf", _ => [new AnyReferenceNamingWidget()])
                    ], direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary)),
            infrequentProperties.Condition(MedicationDispenseInfrequentProperties.DetectedIssue,
                new NameValuePair(
                    new LocalizedLabel("medication-dispense.detectedIssue"),
                    new CommaSeparatedBuilder("f:detectedIssue",
                        _ => [new AnyReferenceNamingWidget()]),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column)
            ),
            infrequentProperties.Condition(MedicationDispenseInfrequentProperties.Context,
                new NameValuePair(
                    new LocalizedLabel("medication-dispense.context"),
                    new ChangeContext("f:context", new AnyReferenceNamingWidget()),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column)
            ),
        ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1");

        var resultWidget = new Concat([
            new Row([
                    new Container([
                        new If(
                                _ => infrequentProperties.Contains(
                                    MedicationDispenseInfrequentProperties.Medication),
                                new OpenTypeElement(null, "medication"))
                            .Else(new LocalizedLabel("node-names.MedicationDispense")),
                    ], optionalClass: "h5 m-0 blue-color fw-bold"),
                    new EnumIconTooltip("f:status", "http://terminology.hl7.org/CodeSystem/medicationdispense-status",
                        new EhdsiDisplayLabel(LabelCodes.Status)),
                    new NarrativeModal(alignRight: false),
                ], flexContainerClasses: "gap-1 align-items-center",
                idSource: skipIdPopulation ? null : new IdentifierSource(navigator),
                flexWrap: false),
            new FlexList([
                nameValuePairs,
                infrequentProperties.Condition(MedicationDispenseInfrequentProperties.DosageInstruction,
                    new DosageCard("f:dosageInstruction")),
                ThematicBreak.SurroundedThematicBreak(
                    infrequentProperties, [
                        MedicationDispenseInfrequentProperties.Quantity,
                        MedicationDispenseInfrequentProperties.DaysSupply,
                        MedicationDispenseInfrequentProperties.WhenPrepared,
                        MedicationDispenseInfrequentProperties.WhenHandedOver,
                        MedicationDispenseInfrequentProperties.Performer,
                        MedicationDispenseInfrequentProperties.AuthorizingPrescription,
                        MedicationDispenseInfrequentProperties.StatusReason,
                        MedicationDispenseInfrequentProperties.Category,
                        MedicationDispenseInfrequentProperties.Type,
                        MedicationDispenseInfrequentProperties.PartOf,
                        MedicationDispenseInfrequentProperties.DetectedIssue,
                        MedicationDispenseInfrequentProperties.Context,
                        MedicationDispenseInfrequentProperties.DosageInstruction,
                    ], [
                        MedicationDispenseInfrequentProperties.Note,
                        MedicationDispenseInfrequentProperties.Text,
                    ]
                ),
                new If(_ => infrequentProperties.Contains(MedicationDispenseInfrequentProperties.Note),
                    new NameValuePair(
                        [new LocalizedLabel("medication-dispense.note")],
                        [
                            new ConcatBuilder("f:note", _ => [new ShowAnnotationCompact()],
                                new LineBreak()),
                        ],
                        style: NameValuePair.NameValuePairStyle.Secondary,
                        direction: FlexDirection.Row
                    )),
                new Condition("f:text", new NarrativeCollapser()),
            ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1"),
        ]);

        return resultWidget.Render(navigator, renderer, context);
    }

    public enum MedicationDispenseInfrequentProperties
    {
        Quantity,
        DaysSupply,
        WhenPrepared,
        WhenHandedOver,
        [OpenType("medication")] Medication,

        Performer,
        AuthorizingPrescription,

        Id,
        Identifier,
        Category,
        Type,
        PartOf,
        Note,
        [OpenType("statusReason")] StatusReason,

        [EnumValueSet("http://terminology.hl7.org/CodeSystem/medicationdispense-status")]
        Status,

        [NarrativeDisplayType] Text,

        DosageInstruction,
        Context,
        DetectedIssue,
    }
}