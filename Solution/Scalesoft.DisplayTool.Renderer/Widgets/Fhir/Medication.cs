using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Medication(bool skipIdPopulation = true)
    : AlternatingBackgroundColumnResourceBase<Medication>, IResourceWidget
{
    public Medication() : this(true)
    {
    }

    public static string ResourceType => "Medication";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public static bool HasBorderedContainer(Widget resourceWidget) => false;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<MedicationInfrequentProperties>(navigator);

        // implicitRules just contains a URL to a set of rules, and has little value to the end user 
        // ignore language
        // ignore extension
        // ignore identifier

        var nameValuePairs = new FlexList([
            infrequentProperties.Optional(MedicationInfrequentProperties.Manufacturer,
                new AnyReferenceNamingWidget(widgetModel: new ReferenceNamingWidgetModel
                {
                    LabelOverride = new LocalizedLabel("medication.manufacturer"),
                    Style = NameValuePair.NameValuePairStyle.Primary,
                    Direction = FlexDirection.Column,
                    Type = ReferenceNamingWidgetType.NameValuePair,
                })),
            infrequentProperties.Optional(MedicationInfrequentProperties.Form,
                new NameValuePair([new LocalizedLabel("medication.form")], [
                        new CodeableConcept()
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            infrequentProperties.Optional(MedicationInfrequentProperties.Amount,
                new NameValuePair([new LocalizedLabel("medication.amount")], [
                        new ShowRatio()
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            infrequentProperties.Condition(MedicationInfrequentProperties.Ingredient,
                new ConcatBuilder("f:ingredient", _ =>
                [
                    new NameValuePair(
                        [new LocalizedLabel("medication.ingredient")],
                        [
                            new Condition("f:itemCodeableConcept or f:itemReference", new NameValuePair([
                                    new LocalizedLabel("medication.ingredient.item")
                                ], [
                                    new CommaSeparatedBuilder("f:itemCodeableConcept",
                                        _ => [new CodeableConcept()]),
                                    new Condition("f:itemCodeableConcept and f:itemReference",
                                        new ConstantText(", ")),
                                    new CommaSeparatedBuilder("f:itemReference",
                                        _ => [new AnyReferenceNamingWidget()])
                                ], direction: FlexDirection.Row,
                                style: NameValuePair.NameValuePairStyle.Secondary)),
                            new Optional("f:strength",
                                new NameValuePair([new LocalizedLabel("medication.ingredient.strength")],
                                    [new ShowRatio()], direction: FlexDirection.Row,
                                    style: NameValuePair.NameValuePairStyle.Secondary)),
                        ],
                        direction: FlexDirection.Column,
                        style: NameValuePair.NameValuePairStyle.Primary
                    ),
                ])),
            infrequentProperties.Optional(MedicationInfrequentProperties.BatchLotNumber,
                new NameValuePair([new LocalizedLabel("medication.batch.lotNumber")], [
                        new Text("@value"),
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            infrequentProperties.Optional(MedicationInfrequentProperties.BatchExpirationDate,
                new NameValuePair([new LocalizedLabel("medication.batch.expirationDate")], [
                        new ShowDateTime(),
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
        ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1");

        var resultWidget = new Concat([
            new Row([
                    new Heading([
                        new Container([
                            new TextContainer(TextStyle.Bold,
                            [
                                new If(nav => nav.EvaluateCondition("f:code"),
                                        new ChangeContext("f:code", new CodeableConcept()))
                                    .Else(new LocalizedLabel("medication")),
                            ]),
                        ], optionalClass: "blue-color d-flex align-items-center"),
                    ], HeadingSize.H5, customClass: "m-0"),
                    new EnumIconTooltip("f:status", "http://hl7.org/fhir/CodeSystem/medication-status",
                        new EhdsiDisplayLabel(LabelCodes.Status)),
                    new NarrativeModal(alignRight: false),
                ], flexContainerClasses: "gap-1 align-items-center",
                idSource: skipIdPopulation ? null : new IdentifierSource(navigator)),
            new FlexList([
                nameValuePairs,
                ThematicBreak.SurroundedThematicBreak(infrequentProperties, [
                    MedicationInfrequentProperties.Manufacturer,
                    MedicationInfrequentProperties.Form,
                    MedicationInfrequentProperties.Amount,
                    MedicationInfrequentProperties.Ingredient,
                    MedicationInfrequentProperties.BatchLotNumber,
                    MedicationInfrequentProperties.BatchExpirationDate,
                ], [
                    MedicationInfrequentProperties.Text,
                ]),
                new Condition("f:text", new NarrativeCollapser()),
            ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1")
        ]);

        return resultWidget.Render(navigator, renderer, context);
    }

    private enum MedicationInfrequentProperties
    {
        Code,
        [HiddenInSimpleMode] Manufacturer,
        [NarrativeDisplayType] Text,
        Form,
        [HiddenInSimpleMode] Amount,
        Ingredient,
        Batch,
        [PropertyPath("f:batch/f:lotNumber")] BatchLotNumber,

        [PropertyPath("f:batch/f:expirationDate")]
        BatchExpirationDate,
    }
}