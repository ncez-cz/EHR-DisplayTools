using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.MedicationResources.MedicationRequestSection;

public class MedicationRequest : ColumnResourceBase<MedicationRequest>, IResourceWidget
{
    public static string ResourceType => "MedicationRequest";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public static bool HasBorderedContainer(Widget widget) => true;

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<InfrequentProps>(navigator);

        var severity = GetSeverity(navigator);

        var widget = new Concat([
            new Card(title: new Row(
                [
                    new Row([
                        new Optional("f:doNotPerform[@value='true']", x =>
                        [
                            new TextContainer(TextStyle.Bold,
                            [
                                new Badge(
                                    new Concat([
                                        new Icon(SupportedIcons.TriangleExclamation),
                                        new Container([
                                            new If(_ => context.RenderMode == RenderMode.Documentation,
                                                new ConstantText(x.GetFullPath())
                                            ).Else(
                                                new LocalizedLabel("medication-request.doNotPerform.true")
                                            ),
                                        ], ContainerType.Span, "align-middle")
                                    ]), severity == Severity.Gray ? Severity.Error : null, optionalClass: "m-0"
                                ),
                            ])
                        ]),
                        new If(
                            nav =>
                                nav.EvaluateCondition(
                                    $"f:priority[@value != '{Priority.Routine.ToEnumString()}']")
                                && (!infrequentProperties.Contains(InfrequentProps.DoNotPerform) ||
                                    nav.EvaluateCondition("f:doNotPerform[@value!='true']")
                                ),
                            new Badge(
                                new Concat([
                                    new Icon(SupportedIcons.TriangleExclamation),
                                    new TextContainer(TextStyle.Bold | TextStyle.Uppercase, [
                                        new EnumLabel("f:priority",
                                            "http://hl7.org/fhir/ValueSet/request-priority"),
                                    ], optionalClass: "align-middle")
                                ]), severity == Severity.Gray ? Severity.Error : null, optionalClass: "m-0"
                            )
                        ),
                        new OpenTypeElement(null, "medication"),
                        new Container([
                            new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/medicationrequest-status",
                                new EhdsiDisplayLabel(LabelCodes.Status)),
                        ], optionalClass: "d-flex align-items-center"),
                    ], flexContainerClasses: "align-items-center gap-2", flexWrap: false),
                    new Row([
                        infrequentProperties.Optional(InfrequentProps.AuthoredOn,
                            new TextContainer(TextStyle.Muted, [
                                new LocalizedLabel("medication-request.authoredOn"),
                                new ConstantText(": "),
                                new ShowDateTime(),
                            ])
                        ),
                    ], flexContainerClasses: "align-items-center gap-1", flexWrap: false),
                    new NarrativeModal(),
                ], flexContainerClasses: "gap-2 align-items-center", flexWrap: false),
                body: new Concat([
                    new Row([
                        new MedicationRequestMedicationContainer("flex-grow-0 flex-shrink-1 flex-basis-auto"),
                        new If(_ => infrequentProperties.Contains(InfrequentProps.DispenseRequest),
                            new MedicationRequestDispenseContainer("flex-grow-0 flex-shrink-1 flex-basis-auto")
                        ),
                        infrequentProperties.Optional(InfrequentProps.Requester,
                            new AnyReferenceNamingWidget(
                                widgetModel: new ReferenceNamingWidgetModel
                                {
                                    Type = ReferenceNamingWidgetType.NameValuePair,
                                    Direction = FlexDirection.Column,
                                    LabelOverride = new LocalizedLabel("medication-request.requester"),
                                }
                            )
                        ),
                    ], flexContainerClasses: "column-gap-6 row-gap-1"),
                    new If(_ => infrequentProperties.Contains(InfrequentProps.DosageInstruction),
                        new DosageCard("f:dosageInstruction")
                    ),
                ]),
                severity: severity, footer: infrequentProperties.Contains(InfrequentProps.Encounter)
                    ? new HideableDetails(ContainerType.Div,
                        new ShowMultiReference("f:encounter",
                            (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                            x =>
                            [
                                new Collapser(
                                    [new LocalizedLabel("node-names.Encounter")],
                                    x.ToList(),
                                    isCollapsed: true),
                            ]
                        )
                    )
                    : null)
        ]);

        return await widget.Render(navigator, renderer, context);
    }

    private enum InfrequentProps
    {
        DispenseRequest,
        DosageInstruction,
        Encounter,
        DoNotPerform,
        AuthoredOn,
        Requester,
    }

    private static Severity GetSeverity(XmlDocumentNavigator navigator)
    {
        if (navigator.EvaluateCondition("f:doNotPerform[@value='true']"))
        {
            return Severity.Gray;
        }

        var priorityRaw = navigator
            .SelectSingleNode("f:priority/@value")
            .Node?.Value
            .ToEnum<Priority>();

        return priorityRaw switch
        {
            Priority.Urgent => Severity.Warning,
            Priority.Asap => Severity.Secondary,
            Priority.Stat => Severity.Error,
            _ => Severity.Primary,
        };
    }
}

public class MedicationRequestMedicationContainer(string? optionalClass = null) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<InfrequentProps>(navigator);

        var widget = new Row([
            new If(_ => infrequentProperties.Contains(InfrequentProps.Reason),
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.MedicationReason),
                    new Concat([
                        new ConcatBuilder("f:reasonCode", _ =>
                            [
                                new CodeableConcept(),
                            ]
                        ),
                        new ConcatBuilder("f:reasonReference", _ =>
                            [
                                new AnyReferenceNamingWidget(),
                            ]
                        ),
                    ]), direction: FlexDirection.Column
                )
            ),
            new If(_ => infrequentProperties.Contains(InfrequentProps.Substitution),
                new NameValuePair(
                    new LocalizedLabel("medication-request.substitution"),
                    new Choose([
                        new When("f:substitution/f:allowedBoolean",
                            new ShowBoolean(
                                new LocalizedLabel("medication-request.substitution.allowed.false"),
                                new LocalizedLabel("medication-request.substitution.allowed.true"),
                                "f:substitution/f:allowedBoolean"
                            )
                        ),
                        new When(
                            "f:substitution/f:allowedCodeableConcept",
                            new ChangeContext("f:substitution/f:allowedCodeableConcept",
                                new CodeableConcept()
                            )
                        ),
                    ]), direction: FlexDirection.Column
                )
            ),
        ], flexContainerClasses: optionalClass + " column-gap-6 row-gap-1");

        return widget.Render(navigator, renderer, context);
    }

    private enum InfrequentProps
    {
        Substitution,
        [OpenType("reason")] Reason,
    }
}

public class MedicationRequestDispenseContainer(string? optionalClass = null) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<InfrequentProps>(navigator.SelectSingleNode("f:dispenseRequest"));

        var widget = new Row([
                new If(
                    _ => infrequentProperties.ContainsAnyOf(InfrequentProps.Quantity, InfrequentProps.ValidityPeriod),
                    new NameValuePair(
                        new EhdsiDisplayLabel(LabelCodes.Dispensation),
                        new ChangeContext("f:dispenseRequest",
                            new TextContainer(TextStyle.Regular, [
                                new ShowQuantity("f:quantity"),
                                new If(
                                    _ => infrequentProperties.ContainsAllOf(InfrequentProps.Quantity,
                                        InfrequentProps.ValidityPeriod),
                                    new ConstantText(" ")
                                ),
                                new ShowPeriod("f:validityPeriod"),
                            ])
                        ), direction: FlexDirection.Column
                    )
                ),
            ], flexContainerClasses: optionalClass + " column-gap-6 row-gap-1"
        );

        return widget.Render(navigator, renderer, context);
    }

    private enum InfrequentProps
    {
        Quantity,
        ValidityPeriod,
    }
}