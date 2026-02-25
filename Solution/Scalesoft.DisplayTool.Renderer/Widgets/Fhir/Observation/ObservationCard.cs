using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;
using RenderMode = Scalesoft.DisplayTool.Renderer.Models.Enums.RenderMode;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;

public class ObservationCard(bool skipIdPopulation = false, bool hideObservationType = false) : Widget
{
    public const string PerformerFunctionExtensionUrl =
        "http://hl7.org/fhir/StructureDefinition/event-performerFunction";

    public const string ClinicallyRelevantTimeExtensionUrl =
        "https://hl7.cz/fhir/lab/StructureDefinition/cz-lab-clinically-relevant-time";

    public const string SupportingInfoExtensionUrl =
        "http://hl7.org/fhir/StructureDefinition/workflow-supportingInfo";

    public const string LabTestKitExtensionUrl =
        "http://hl7.eu/fhir/laboratory/StructureDefinition/observation-deviceLabTestKit";

    public const string CertifiedRefMaterialCodeableExtensionUrl =
        "http://hl7.eu/fhir/laboratory/StructureDefinition/observation-certifiedRefMaterialCodeable";

    public const string CertifiedRefMaterialIdentifierExtensionUrl =
        "http://hl7.eu/fhir/laboratory/StructureDefinition/observation-certifiedRefMaterialIdentifer";

    public const string TriggeredByExtensionUrl =
        "http://hl7.org/fhir/5.0/StructureDefinition/extension-Observation.triggeredBy";

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<ObservationInfrequentProperties>(navigator);

        var chartContainer = new StructuredDetails();

        var nameValuePairs = new FlexList([
            infrequentProperties.Condition(ObservationInfrequentProperties.Value,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.Result),
                    new OpenTypeElement(chartContainer, useChartInDetailsPlaceholder: false),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            new If(_ => !hideObservationType,
                infrequentProperties.Condition(ObservationInfrequentProperties.Category,
                    // prefer decision of MZČR (hide category) to observation obligation at https://build.fhir.org/ig/HL7-cz/img/StructureDefinition-cz-observationResult-obl-img.html
                    new NameValuePair(
                        new LocalizedLabel("observation.category"),
                        new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()]),
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    )
                )
            ),
            infrequentProperties.Optional(ObservationInfrequentProperties.Subject,
                new AnyReferenceNamingWidget(
                    showOptionalDetails: false,
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        Direction = FlexDirection.Column,
                        Style = NameValuePair.NameValuePairStyle.Primary,
                        LabelOverride = new LocalizedLabel("observation.subject"),
                    }
                )
            ),
            infrequentProperties.Condition(ObservationInfrequentProperties.PartOf,
                new HideableDetails(
                    new NameValuePair(
                        new LocalizedLabel("observation.partOf"),
                        new ConcatBuilder("f:partOf", _ => [new AnyReferenceNamingWidget()], new LineBreak()),
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    )
                )
            ),
            infrequentProperties.Condition(ObservationInfrequentProperties.Effective,
                new NameValuePair(
                    [
                        new LocalizedLabel("observation.effective"),
                        new OpenTypeChangeContext("effective",
                            new Optional($"f:extension[@url='{ClinicallyRelevantTimeExtensionUrl}']",
                                new ConstantText(" ("),
                                new ChangeContext("f:valueCoding", new Coding()),
                                new ConstantText(")")
                            )
                        ),
                    ],
                    [new Chronometry("effective")],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Condition(ObservationInfrequentProperties.Focus,
                new HideableDetails(
                    new NameValuePair(
                        new LocalizedLabel("observation.focus"),
                        new ConcatBuilder("f:focus", _ => [new AnyReferenceNamingWidget()], new LineBreak()),
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    )
                )
            ),
            infrequentProperties.Condition(ObservationInfrequentProperties.BasedOn,
                new HideableDetails(
                    new NameValuePair(
                        new LocalizedLabel("observation.basedOn"),
                        new ConcatBuilder("f:basedOn", _ => [new AnyReferenceNamingWidget()], new LineBreak()),
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    )
                )
            ),
            infrequentProperties.Condition(ObservationInfrequentProperties.SupportingInfo,
                new HideableDetails(
                    new NameValuePair(
                        new LocalizedLabel("observation.supportingInfo"),
                        new ConcatBuilder($"f:extension[@url='{SupportingInfoExtensionUrl}']/f:valueReference",
                            _ => [new AnyReferenceNamingWidget()], new LineBreak()
                        ),
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    )
                )
            ),
            infrequentProperties.Optional(ObservationInfrequentProperties.Issued,
                new HideableDetails(
                    new NameValuePair(
                        new LocalizedLabel("observation.issued"),
                        new ShowDateTime(),
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    )
                )
            ),
            infrequentProperties.Condition(ObservationInfrequentProperties.DerivedFrom,
                new NameValuePair(
                    new LocalizedLabel("observation.derivedFrom"),
                    new ConcatBuilder("f:derivedFrom", _ => [new AnyReferenceNamingWidget()], new LineBreak()),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(ObservationInfrequentProperties.Method,
                new NameValuePair(
                    new LocalizedLabel("observation.method"),
                    new CodeableConcept(),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Condition(ObservationInfrequentProperties.Performer,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.Performer),
                    new ConcatBuilder("f:performer", _ =>
                    [
                        new Container([
                            new AnyReferenceNamingWidget(),
                            new Condition($"f:extension[@url='{PerformerFunctionExtensionUrl}']",
                                new ConstantText(" - "),
                                new TextContainer(TextStyle.Italic,
                                    new CommaSeparatedBuilder(
                                        $"f:extension[@url='{PerformerFunctionExtensionUrl}']/f:valueCodeableConcept",
                                        _ => new CodeableConcept()))
                            ),
                        ], ContainerType.Span),
                    ], new LineBreak()),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Condition(ObservationInfrequentProperties.Interpretation,
                new NameValuePair(
                    new LocalizedLabel("observation.interpretation"),
                    new CommaSeparatedBuilder("f:interpretation", _ => [new CodeableConcept()]),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(ObservationInfrequentProperties.BodySite,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.BodySite),
                    new CodeableConcept(),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(ObservationInfrequentProperties.Specimen,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        Direction = FlexDirection.Column,
                        Style = NameValuePair.NameValuePairStyle.Primary,
                        LabelOverride = new LocalizedLabel("observation.specimen"),
                    }
                )
            ),
            new If(_ => infrequentProperties.ContainsAnyOf(
                    ObservationInfrequentProperties.CertifiedRefMaterialCodeable,
                    ObservationInfrequentProperties.CertifiedRefMaterialIdentifier
                ),
                new HideableDetails(new NameValuePair(
                    [
                        new LocalizedLabel("observation.certifiedRefMaterial"),
                    ],
                    [
                        new CommaSeparatedBuilder(
                            $"f:extension[@url='{CertifiedRefMaterialCodeableExtensionUrl}']/f:valueCodeableConcept",
                            _ => [new CodeableConcept()]),
                        new If(
                            _ => infrequentProperties.ContainsAllOf(
                                ObservationInfrequentProperties.CertifiedRefMaterialIdentifier,
                                ObservationInfrequentProperties.CertifiedRefMaterialCodeable), new ConstantText(", ")),
                        new CommaSeparatedBuilder(
                            $"f:extension[@url='{CertifiedRefMaterialIdentifierExtensionUrl}']/f:valueIdentifier",
                            _ => [new ShowIdentifier()]),
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                ))
            ),
            new If(_ =>
                    infrequentProperties.Contains(ObservationInfrequentProperties.LabTestKitExtension) ||
                    infrequentProperties.Contains(ObservationInfrequentProperties.Device),
                new NameValuePair([new LocalizedLabel("observation.device")], [
                        new ListBuilder(
                            $"f:extension[@url='{LabTestKitExtensionUrl}']/f:valueReference|f:device",
                            FlexDirection.Column, _ =>
                            [
                                new AnyReferenceNamingWidget()
                            ],
                            separator: new LineBreak()
                        )
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column)
            ),
            infrequentProperties.Optional(
                new HideableDetails(new LineBreak()),
                ObservationInfrequentProperties.TriggeredByExtension,
                new HideableDetails(new NameValuePair([new LocalizedLabel("observation.triggeredBy")], [
                    new Condition(
                        "f:extension[@url='observation']",
                        new NameValuePair(
                            new LocalizedLabel("observation"),
                            new CommaSeparatedBuilder(
                                "f:extension[@url='observation']/f:valueReference",
                                _ => new AnyReferenceNamingWidget()
                            ),
                            style: NameValuePair.NameValuePairStyle.Secondary
                        )
                    ),
                    new Condition(
                        "f:extension[@url='type']",
                        new NameValuePair(
                            new LocalizedLabel("observation.triggeredBy.type"),
                            new CommaSeparatedBuilder(
                                "f:extension[@url='type']/f:valueCode",
                                _ => new EnumLabel(".", "http://hl7.org/fhir/ValueSet/observation-triggeredbytype")
                            ),
                            style: NameValuePair.NameValuePairStyle.Secondary
                        )
                    ),
                    new Condition(
                        "f:extension[@url='reason']",
                        new NameValuePair(
                            new LocalizedLabel("observation.triggeredBy.reason"),
                            new CommaSeparatedBuilder(
                                "f:extension[@url='reason']/f:valueString",
                                _ => new Text("@value")
                            ),
                            style: NameValuePair.NameValuePairStyle.Secondary
                        )
                    )
                ], style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Column))
            ),
            infrequentProperties.Condition(ObservationInfrequentProperties.Note,
                new NameValuePair(
                    new LocalizedLabel("observation.note"),
                    new ConcatBuilder("f:note", _ => [new ShowAnnotationCompact()], new LineBreak()),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Condition(ObservationInfrequentProperties.ReferenceRange,
                new ReferenceRanges()
            ),
            infrequentProperties.Optional(ObservationInfrequentProperties.Encounter,
                new HideableDetails(new AnyReferenceNamingWidget())
            )
        ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1");

        var subcomponents = navigator.SelectAllNodes("f:component");
        var resultWidget = new Concat([
            new Row([
                    new Container([
                        new TextContainer(TextStyle.Bold,
                            [new ChangeContext("f:code", new CodeableConcept())]),
                        new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/observation-status",
                            new EhdsiDisplayLabel(LabelCodes.Status)),
                    ], optionalClass: "h5 m-0 blue-color"),
                    new NarrativeModal(alignRight: false),
                ], flexContainerClasses: "gap-1 align-items-center", flexWrap: false,
                idSource: skipIdPopulation ? null : new IdentifierSource(navigator)),
            new FlexList([
                nameValuePairs,
                new Condition("f:component", new Card(new LocalizedLabel("observation.component"),
                    new AlternatingBackgroundColumn([
                        ..subcomponents.Select(nav =>
                        {
                            var componentInfrequentProperties =
                                InfrequentProperties.Evaluate<ObservationInfrequentProperties>(nav);

                            var chartContainerComponent = new StructuredDetails();

                            var widget = new Concat([
                                new Row([
                                    new Container([
                                        new TextContainer(TextStyle.Bold,
                                            [new ChangeContext("f:code", new CodeableConcept())]),
                                    ], optionalClass: "d-flex align-items-center h5 m-0 blue-color"),
                                ], flexContainerClasses: "gap-1 align-items-center"),
                                new FlexList([
                                    new If(
                                        _ => componentInfrequentProperties.Contains(ObservationInfrequentProperties
                                            .Value),
                                        new NameValuePair(
                                            new EhdsiDisplayLabel(LabelCodes.Result),
                                            new OpenTypeElement(chartContainerComponent,
                                                useChartInDetailsPlaceholder: false),
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    ),
                                    new If(
                                        _ => componentInfrequentProperties.Contains(ObservationInfrequentProperties
                                            .Interpretation),
                                        new NameValuePair(
                                            new LocalizedLabel("observation.component.interpretation"),
                                            new CommaSeparatedBuilder("f:interpretation", _ => [new CodeableConcept()]),
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    ),
                                    new If(
                                        _ => componentInfrequentProperties.Contains(ObservationInfrequentProperties
                                            .ReferenceRange),
                                        new ReferenceRanges()
                                    ),
                                    ..chartContainerComponent.Build()
                                ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1")
                            ]);

                            return new ChangeContext(nav, widget);
                        })
                    ]))),
                new If(_ => infrequentProperties.Contains(ObservationInfrequentProperties.HasMember),
                    new Card(new LocalizedLabel("observation.hasMember"),
                        new ShowMultiReference("f:hasMember", displayResourceType: false))),
                ..chartContainer.Build(),
                new Condition("f:text", new NarrativeCollapser()),
            ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1"),
        ]);

        return await resultWidget.Render(navigator, renderer, context);
    }

    private class ReferenceRanges : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var referenceRange =
                new ListBuilder("f:referenceRange", FlexDirection.Row, _ =>
                [
                    new NameValuePair([new LocalizedLabel("observation.referenceRange")], [
                        new Container([
                                new Condition("f:low or f:high",
                                    new NameValuePair(
                                        new LocalizedLabel("observation.referenceRange.range"),
                                        new ShowRange(),
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        size: NameValuePair.NameValuePairSize.Small
                                    )
                                ),
                                new Optional("f:type",
                                    new NameValuePair(
                                        new LocalizedLabel("observation.referenceRange.type"),
                                        new CodeableConcept(),
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        size: NameValuePair.NameValuePairSize.Small
                                    )
                                ),
                                new Condition("f:appliesTo",
                                    new NameValuePair(
                                        new LocalizedLabel("observation.referenceRange.appliesTo"),
                                        new CommaSeparatedBuilder("f:appliesTo", _ => [new CodeableConcept()]),
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        size: NameValuePair.NameValuePairSize.Small
                                    )
                                ),
                                new Optional("f:age",
                                    new NameValuePair(
                                        new LocalizedLabel("observation.referenceRange.age"),
                                        new ShowRange(),
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        size: NameValuePair.NameValuePairSize.Small
                                    )
                                ),
                                new Optional("f:text",
                                    new NameValuePair(
                                        new LocalizedLabel("observation.referenceRange.text"),
                                        new Text("@value"),
                                        style: NameValuePair.NameValuePairStyle.Secondary,
                                        size: NameValuePair.NameValuePairSize.Small
                                    )
                                ),
                            ],
                            optionalClass:
                            $"name-value-pair-wrapper column-gap-4 {(context.RenderMode != RenderMode.Documentation ? "two-col-grid" : string.Empty)}"),
                    ], style: NameValuePair.NameValuePairStyle.Primary, direction: FlexDirection.Column),
                ], flexContainerClasses: "column-gap-6");

            return referenceRange.Render(navigator, renderer,
                context);
        }
    }
}

public enum ObservationInfrequentProperties
{
    [OpenType("effective")] Effective,
    [OpenType("value")] Value,
    Performer,
    Subject,
    Category,
    Method,
    BasedOn,
    PartOf,
    Interpretation,
    Note,
    BodySite,
    Specimen,
    ReferenceRange,
    HasMember,
    DerivedFrom,
    Component,
    Text,
    Encounter,


    [Extension(ObservationCard.LabTestKitExtensionUrl)]
    LabTestKitExtension,
    Device,

    [Extension(ObservationCard.CertifiedRefMaterialCodeableExtensionUrl)]
    CertifiedRefMaterialCodeable,

    [Extension(ObservationCard.CertifiedRefMaterialIdentifierExtensionUrl)]
    CertifiedRefMaterialIdentifier,

    [Extension(ObservationCard.SupportingInfoExtensionUrl)]
    SupportingInfo,

    Focus,
    Issued,

    [Extension(ObservationCard.TriggeredByExtensionUrl)]
    TriggeredByExtension,
}