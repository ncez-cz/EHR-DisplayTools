using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ServiceRequest(
    ISet<ServiceRequestProperties> displayableProperties,
    bool skipIdPopulation = false,
    NameValuePair.NameValuePairStyle nameValuePairStyle = NameValuePair.NameValuePairStyle.Primary
)
    : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<ServiceRequestProperties>([navigator]);

        var resultWidget = new Concat([
            new If(
                _ => displayableProperties.ContainsAny(ServiceRequestProperties.Code, ServiceRequestProperties.Status,
                    ServiceRequestProperties.Text, ServiceRequestProperties.Occurrence), new Row([
                        new If(
                            _ => displayableProperties.ContainsAny(ServiceRequestProperties.Code,
                                ServiceRequestProperties.Status, ServiceRequestProperties.Occurrence), new Container([
                                new If(
                                    _ => displayableProperties.Contains(ServiceRequestProperties.Code),
                                    new If(_ => infrequentProperties.Contains(ServiceRequestProperties.Code),
                                            new TextContainer(TextStyle.Bold,
                                                [new ChangeContext("f:code", new CodeableConcept())]))
                                        .Else(new ConstantText("Žádost o službu"))
                                ),
                                new If(
                                    _ => displayableProperties.Contains(ServiceRequestProperties.Occurrence) &&
                                         infrequentProperties.Contains(ServiceRequestProperties.Occurrence),
                                    new If(
                                        _ => displayableProperties.Contains(ServiceRequestProperties.Code) &&
                                             infrequentProperties.Contains(ServiceRequestProperties.Occurrence),
                                        new ConstantText(" ")),
                                    new TextContainer(TextStyle.Light,
                                        [new Chronometry("occurrence")])
                                ),
                                new If(_ => displayableProperties.Contains(ServiceRequestProperties.Status),
                                    new HideableDetails(new EnumIconTooltip("f:status",
                                        "http://hl7.org/fhir/ValueSet/observation-status",
                                        new DisplayLabel(LabelCodes.Status)))),
                            ], optionalClass: "h5 m-0 blue-color")),
                        new If(_ => displayableProperties.Contains(ServiceRequestProperties.Text),
                            new NarrativeModal(alignRight: false)),
                    ], flexContainerClasses: "gap-1 align-items-center",
                    idSource: skipIdPopulation ? null : new IdentifierSource(navigator))),
            new FlexList([
                new FlexList([
                    new If(_ => displayableProperties.Contains(ServiceRequestProperties.Intent), new HideableDetails(
                        new NameValuePair(
                            new ConstantText("Záměr"),
                            new EnumLabel("f:intent", "http://hl7.org/fhir/ValueSet/request-intent"),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.Category) &&
                             infrequentProperties.Contains(ServiceRequestProperties.Category),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Kategorie"),
                            new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()]),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.Priority) &&
                             infrequentProperties.Contains(ServiceRequestProperties.Priority),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Priorita"),
                            new EnumLabel("f:priority", "http://hl7.org/fhir/ValueSet/request-priority"),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.BodySite) &&
                             infrequentProperties.Contains(ServiceRequestProperties.BodySite),
                        new HideableDetails(new NameValuePair(
                            new DisplayLabel(LabelCodes.BodySite),
                            new CommaSeparatedBuilder("f:bodySite", _ => new CodeableConcept()),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.AuthoredOn) &&
                             infrequentProperties.Contains(ServiceRequestProperties.AuthoredOn),
                        new NameValuePair(
                            new ConstantText("Datum a čas žádosti"),
                            new ShowDateTime("f:authoredOn"),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        )
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.DoNotPerform) &&
                             infrequentProperties.Contains(ServiceRequestProperties.DoNotPerform) &&
                             navigator.EvaluateCondition("f:doNotPerform[@value='true']"),
                        new NameValuePair(
                            new ConstantText("Zákaz"),
                            new ShowDoNotPerform(),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        )
                    ),
                    new If(_ => displayableProperties.Contains(ServiceRequestProperties.Subject), new HideableDetails(
                        new NameValuePair(
                            new ConstantText("Předmět"),
                            new AnyReferenceNamingWidget("f:subject"),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))),
                    //additional
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.Replaces) &&
                             infrequentProperties.Contains(ServiceRequestProperties.Replaces),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Nahrazuje"),
                            new CommaSeparatedBuilder("f:replaces",
                                _ => [new AnyReferenceNamingWidget(showOptionalDetails: false)]),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.BasedOn) &&
                             infrequentProperties.Contains(ServiceRequestProperties.BasedOn),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Založeno na"),
                            new CommaSeparatedBuilder("f:basedOn",
                                _ => [new AnyReferenceNamingWidget(showOptionalDetails: false)]),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.OrderDetail) &&
                             infrequentProperties.Contains(ServiceRequestProperties.OrderDetail),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Podrobnosti objednávky"),
                            new CommaSeparatedBuilder("f:orderDetail", _ => [new CodeableConcept()]),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.Quantity) &&
                             infrequentProperties.Contains(ServiceRequestProperties.Quantity),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Množství"),
                            new OpenTypeElement(null, "quantity"), // Quantity | Ratio | Range
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.AsNeeded) &&
                             infrequentProperties.Contains(ServiceRequestProperties.AsNeeded),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Dle potřeby"),
                            new OpenTypeElement(null, "asNeeded"), // 	boolean | CodeableConcept
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.Insurance) &&
                             infrequentProperties.Contains(ServiceRequestProperties.Insurance),
                        new HideableDetails(
                            new CommaSeparatedBuilder("f:insurance",
                                _ =>
                                [
                                    new AnyReferenceNamingWidget(
                                        showOptionalDetails: false,
                                        widgetModel: new ReferenceNamingWidgetModel
                                        {
                                            Type = ReferenceNamingWidgetType.NameValuePair,
                                            Direction = FlexDirection.Column,
                                            Style = NameValuePair.NameValuePairStyle.Primary,
                                            LabelOverride = new ConstantText("Úhrada"),
                                        }
                                    ),
                                ]
                            )
                        )
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.SupportingInfo) &&
                             infrequentProperties.Contains(ServiceRequestProperties.SupportingInfo),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Podpůrné informace"),
                            new CommaSeparatedBuilder("f:supportingInfo",
                                _ => [new AnyReferenceNamingWidget(showOptionalDetails: false)]),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.Specimen) &&
                             infrequentProperties.Contains(ServiceRequestProperties.Specimen),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Vzorky"),
                            new CommaSeparatedBuilder("f:specimen",
                                _ => [new AnyReferenceNamingWidget(showOptionalDetails: false)]),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.PatientInstruction) &&
                             infrequentProperties.Contains(ServiceRequestProperties.PatientInstruction),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Pokyny pro pacienta"),
                            new Text("f:patientInstruction/@value"),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.RelevantHistory) &&
                             infrequentProperties.Contains(ServiceRequestProperties.RelevantHistory),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Relevantní historie"),
                            new CommaSeparatedBuilder("f:relevantHistory",
                                _ => [new AnyReferenceNamingWidget(showOptionalDetails: false)]),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    // service
                    new If(
                        _ => (displayableProperties.Contains(ServiceRequestProperties.ReasonCode) &&
                              infrequentProperties.Contains(
                                  ServiceRequestProperties
                                      .ReasonCode // reasonCode in not specified as SHALL display by obligations
                              )) || (
                            displayableProperties.Contains(ServiceRequestProperties.ReasonReference) &&
                            infrequentProperties.Contains(ServiceRequestProperties.ReasonReference)),
                        new NameValuePair(
                            new ConstantText("Důvod žádosti"),
                            new CommaSeparatedBuilder("f:reasonCode|f:reasonReference",
                                (_, _, x) =>
                                {
                                    return x.Node?.Name switch
                                    {
                                        "reasonCode" =>
                                        [
                                            new If(
                                                _ => displayableProperties.Contains(ServiceRequestProperties
                                                    .ReasonCode),
                                                new CodeableConcept())
                                        ],
                                        "reasonReference" =>
                                        [
                                            new If(
                                                _ => displayableProperties.Contains(
                                                    ServiceRequestProperties.ReasonReference),
                                                new AnyReferenceNamingWidget())
                                        ],
                                        _ => throw new InvalidOperationException()
                                    };
                                }
                            ),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        )
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.Requester) &&
                             infrequentProperties.Contains(ServiceRequestProperties.Requester),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Žadatel"),
                            new AnyReferenceNamingWidget("f:requester"),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.PerformerType) &&
                             infrequentProperties.Contains(ServiceRequestProperties.PerformerType),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Typ požadovaného zpracovatele"),
                            new ChangeContext("f:performerType", new CodeableConcept()),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => displayableProperties.Contains(ServiceRequestProperties.Performer) &&
                             infrequentProperties.Contains(ServiceRequestProperties.Performer),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Požadovaný zpracovatel"),
                            new ListBuilder("f:performer", FlexDirection.Column,
                                _ => [new AnyReferenceNamingWidget(showOptionalDetails: false)]),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    new If(
                        _ => (displayableProperties.Contains(ServiceRequestProperties.LocationCode) &&
                              infrequentProperties.Contains(
                                  ServiceRequestProperties.LocationCode)) || (
                            displayableProperties.Contains(
                                ServiceRequestProperties.LocationReference) &&
                            infrequentProperties.Contains(ServiceRequestProperties.LocationReference)),
                        new HideableDetails(new NameValuePair(
                            new ConstantText("Místo"),
                            new CommaSeparatedBuilder("f:locationCode|f:locationReference",
                                (_, _, x) =>
                                {
                                    return x.Node?.Name switch
                                    {
                                        "locationCode" =>
                                        [
                                            new If(
                                                _ => displayableProperties.Contains(ServiceRequestProperties
                                                    .LocationCode),
                                                new CodeableConcept())
                                        ],
                                        "locationReference" =>
                                        [
                                            new If(
                                                _ => displayableProperties.Contains(ServiceRequestProperties
                                                    .LocationReference), new AnyReferenceNamingWidget())
                                        ],
                                        _ => throw new InvalidOperationException()
                                    };
                                }
                            ),
                            style: nameValuePairStyle,
                            direction: FlexDirection.Column
                        ))
                    ),
                    //note
                    new If(_ => infrequentProperties.Contains(ServiceRequestProperties.Note),
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Poznámky"),
                                new ListBuilder("f:note",
                                    FlexDirection.Column, _ => [new ShowAnnotationCompact()],
                                    flexContainerClasses: "gap-0"),
                                style: nameValuePairStyle,
                                direction: FlexDirection.Column
                            )
                        )
                    ),
                ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1"),
                new If(
                    _ => displayableProperties.Contains(ServiceRequestProperties.Encounter) &&
                         infrequentProperties.Contains(ServiceRequestProperties.Encounter),
                    new HideableDetails(new ShowMultiReference("f:encounter",
                        (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                        x =>
                        [
                            new Collapser([new ConstantText(Labels.Encounter)], [], x.ToList(),
                                isCollapsed: true)
                        ]
                    ))
                ),
                new If(
                    _ => displayableProperties.Contains(ServiceRequestProperties.Text) &&
                         infrequentProperties.Contains(ServiceRequestProperties.Text),
                    new NarrativeCollapser()
                ),
            ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1"),
        ]);

        return await resultWidget.Render(navigator, renderer, context);
    }
}

public enum ServiceRequestProperties
{
    Id,
    Meta,
    ImplicitRules,
    Language,
    Text,

    [Extension("http://hl7.org/fhir/StructureDefinition/bodySite")]
    BodySiteExtension,
    Identifier,
    InstantiatesCanonical,
    InstantiatesUri,
    BasedOn,
    Replaces,
    Requisition,
    Status,
    Intent,
    Category,
    Priority,
    DoNotPerform,
    Code,
    [OpenType("quantity")] Quantity,
    Subject,
    Encounter,
    [OpenType("occurrence")] Occurrence,
    [OpenType("asNeeded")] AsNeeded,
    AuthoredOn,
    Requester,
    PerformerType,
    Performer,
    LocationCode,
    LocationReference,
    ReasonCode,
    ReasonReference,
    Insurance,
    SupportingInfo,
    Specimen,
    BodySite,
    Note,
    OrderDetail,
    PatientInstruction,
    RelevantHistory,
}