using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Procedures(bool skipIdPopulation = true)
    : AlternatingBackgroundColumnResourceBase<Procedures>, IResourceWidget
{
    public Procedures() : this(true)
    {
    }

    public static string ResourceType => "Procedure";

    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public static bool HasBorderedContainer(Widget widget) => false;

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        var summaryItems = new List<Widget>();
        if (item.EvaluateCondition("f:code"))
        {
            summaryItems.Add(new ChangeContext(item, "f:code", new CodeableConcept()));
        }

        if (summaryItems.Count == 0)
        {
            return null;
        }

        var result = summaryItems.Intersperse(new ConstantText(" ")).ToArray();

        return new ResourceSummaryModel
        {
            Value = new Container(result, ContainerType.Span),
        };
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<ProcedureInfrequentPropertiesPaths>(navigator);

        var nameValuePairs = new FlexList([
            infrequentProperties.Optional(ProcedureInfrequentPropertiesPaths.Category,
                new NameValuePair(
                    new LocalizedLabel("procedure.category"),
                    new CodeableConcept(),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(ProcedureInfrequentPropertiesPaths.Recorder,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("procedure.recorder"),
                        Direction = FlexDirection.Column,
                        Style = NameValuePair.NameValuePairStyle.Primary,
                    }
                )
            ),
            new If(_ => infrequentProperties.Contains(ProcedureInfrequentPropertiesPaths.Performer),
                new ConcatBuilder("f:performer",
                    _ =>
                    [
                        new NameValuePair(
                            [
                                new If(
                                        nav => nav.EvaluateCondition("f:function"),
                                        new ChangeContext(
                                            "f:function",
                                            new LocalizedLabel("procedure.performer"),
                                            new ConstantText(" ("),
                                            new CodeableConcept(),
                                            new ConstantText(")")
                                        )
                                    )
                                    .Else(new LocalizedLabel("procedure.performer")),
                            ],
                            [
                                new AnyReferenceNamingWidget()
                            ],
                            style: NameValuePair.NameValuePairStyle.Primary,
                            direction: FlexDirection.Column
                        )
                    ])
            ),
            infrequentProperties.Optional(ProcedureInfrequentPropertiesPaths.Location,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("procedure.location"),
                        Direction = FlexDirection.Column,
                        Style = NameValuePair.NameValuePairStyle.Primary,
                    }
                )
            ),
            new If(_ => infrequentProperties.Contains(ProcedureInfrequentPropertiesPaths.Performed),
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.ProcedureDate),
                    new OpenTypeElement(null, "performed"), // dateTime | Period | string | Age | Range
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(ProcedureInfrequentPropertiesPaths.BodySite,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.BodySite),
                    new CodeableConcept(),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            infrequentProperties.Optional(ProcedureInfrequentPropertiesPaths.BodySiteExtension,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new EhdsiDisplayLabel(LabelCodes.BodySite),
                        Direction = FlexDirection.Column,
                        Style = NameValuePair.NameValuePairStyle.Primary,
                    }
                )
            ),
            new If(_ => infrequentProperties.ContainsAnyOf(
                    ProcedureInfrequentPropertiesPaths.ReasonCode,
                    ProcedureInfrequentPropertiesPaths.ReasonReference
                ),
                new NameValuePair([new LocalizedLabel("procedure.reason")], [
                        new CommaSeparatedBuilder("f:reasonCode",
                            _ => [new CodeableConcept()]),
                        new Condition("f:reasonCode and f:reasonReference", new ConstantText(", ")),
                        new CommaSeparatedBuilder("f:reasonReference",
                            _ => [new AnyReferenceNamingWidget()])
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(ProcedureInfrequentPropertiesPaths.Outcome,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.Outcome),
                    new CodeableConcept(),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            new If(_ => infrequentProperties.Contains(ProcedureInfrequentPropertiesPaths.Complication),
                new NameValuePair(
                    new LocalizedLabel("procedure.complication"),
                    new CommaSeparatedBuilder("f:complication", _ => [new CodeableConcept()]),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            new If(_ => infrequentProperties.Contains(ProcedureInfrequentPropertiesPaths.ComplicationDetail),
                new NameValuePair(
                    new LocalizedLabel("procedure.complicationDetail"),
                    new ConcatBuilder("f:complicationDetail", _ => [new AnyReferenceNamingWidget()], new LineBreak()),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            new If(_ => infrequentProperties.ContainsAnyOf(
                    ProcedureInfrequentPropertiesPaths.UsedCode,
                    ProcedureInfrequentPropertiesPaths.UsedReference
                ),
                new NameValuePair(
                    [new LocalizedLabel("procedure.used")],
                    [
                        new CommaSeparatedBuilder("f:usedCode",
                            _ => [new CodeableConcept()]),
                        new Condition("f:usedCode and f:usedReference", new ConstantText(", ")),
                        new CommaSeparatedBuilder("f:usedReference",
                            _ => [new AnyReferenceNamingWidget()])
                    ],
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )),
            new If(_ => infrequentProperties.Contains(ProcedureInfrequentPropertiesPaths.FocalDevice),
                new ConcatBuilder("f:focalDevice", _ =>
                [
                    new NameValuePair(
                        [
                            new If(
                                    nav => nav.EvaluateCondition("f:action"),
                                    new ChangeContext(
                                        "f:action",
                                        new LocalizedLabel("procedure.focalDevice"),
                                        new ConstantText(" ("),
                                        new CodeableConcept(),
                                        new ConstantText(")")
                                    )
                                )
                                .Else(new LocalizedLabel("procedure.focalDevice")),
                        ],
                        [
                            new AnyReferenceNamingWidget("f:manipulated"),
                        ],
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    )
                ], new LineBreak())),
            infrequentProperties.Optional(ProcedureInfrequentPropertiesPaths.Encounter,
                new HideableDetails(
                    new AnyReferenceNamingWidget(
                        widgetModel: new ReferenceNamingWidgetModel
                        {
                            Type = ReferenceNamingWidgetType.NameValuePair,
                            LabelOverride = new LocalizedLabel("node-names.Encounter"),
                            Direction = FlexDirection.Column,
                            Style = NameValuePair.NameValuePairStyle.Primary,
                        }
                    )
                )),
        ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1");

        var resultWidget = new Concat([
            new Row([
                    new Container([
                        new TextContainer(TextStyle.Bold,
                            [new ChangeContext("f:code", new CodeableConcept())]),
                    ], optionalClass: "h5 m-0 blue-color"),
                    new EnumIconTooltip("f:status", "http://hl7.org/fhir/event-status",
                        new EhdsiDisplayLabel(LabelCodes.Status)),
                    new NarrativeModal(alignRight: false),
                ], flexContainerClasses: "gap-1 align-items-center",
                idSource: skipIdPopulation ? null : new IdentifierSource(navigator),
                flexWrap: false),
            new FlexList([
                nameValuePairs,
                ThematicBreak.SurroundedThematicBreak(
                    infrequentProperties, [
                        ProcedureInfrequentPropertiesPaths.Category,
                        ProcedureInfrequentPropertiesPaths.Recorder,
                        ProcedureInfrequentPropertiesPaths.Performer,
                        ProcedureInfrequentPropertiesPaths.Location,
                        ProcedureInfrequentPropertiesPaths.Performed,
                        ProcedureInfrequentPropertiesPaths.BodySite,
                        ProcedureInfrequentPropertiesPaths.BodySiteExtension,
                        ProcedureInfrequentPropertiesPaths.ReasonCode,
                        ProcedureInfrequentPropertiesPaths.ReasonReference,
                        ProcedureInfrequentPropertiesPaths.Outcome,
                        ProcedureInfrequentPropertiesPaths.Complication,
                        ProcedureInfrequentPropertiesPaths.ComplicationDetail,
                        ProcedureInfrequentPropertiesPaths.UsedCode,
                        ProcedureInfrequentPropertiesPaths.UsedReference,
                        ProcedureInfrequentPropertiesPaths.FocalDevice,
                        ProcedureInfrequentPropertiesPaths.Encounter,
                    ], [
                        ProcedureInfrequentPropertiesPaths.Note,
                        ProcedureInfrequentPropertiesPaths.Text,
                    ]
                ),
                new If(_ => infrequentProperties.Contains(ProcedureInfrequentPropertiesPaths.Note),
                    new NameValuePair(
                        [new LocalizedLabel("procedure.note")],
                        [
                            new ConcatBuilder("f:note", _ => [new ShowAnnotationCompact()], new LineBreak()),
                        ],
                        style: NameValuePair.NameValuePairStyle.Secondary,
                        direction: FlexDirection.Row
                    )),
                new Condition("f:text", new NarrativeCollapser()),
            ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1"),
        ]);

        return resultWidget.Render(navigator, renderer, context);
    }
}

public enum ProcedureInfrequentPropertiesPaths
{
    [OpenType("performed")] Performed, // 0..1 DateTime | Period | String | Age | Range When the procedure was performed
    ReasonCode, //0..*	CodeableConcept	Why the procedure was performed (code)
    ReasonReference, //	0..*	Reference(Condition (HDR) | Observation | Procedure (HDR) | DiagnosticReport | DocumentReference)	Why the procedure was performed (details)
    BodySite,

    [Extension("http://hl7.org/fhir/StructureDefinition/procedure-targetBodyStructure")]
    BodySiteExtension,
    [HiddenInSimpleMode] Encounter,

    Category,
    Recorder,
    Performer,
    Outcome,
    Complication,
    ComplicationDetail,
    Location,
    Note,
    UsedReference,
    UsedCode,
    FocalDevice,
    [NarrativeDisplayType] Text,
}