using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Appointment : ColumnResourceBase<Appointment>, IResourceWidget
{
    public static string ResourceType => "Appointment";

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var participantsInfo =
            new ListBuilder("f:participant", FlexDirection.Row, _ =>
                {
                    var tree =
                        new Card(
                            new Container([
                                new Container([
                                    new ConstantText("Činitel"),
                                    new Optional("f:actor",
                                        new ConstantText(" ("),
                                        new AnyReferenceNamingWidget(),
                                        new ConstantText(")")
                                    ),
                                ], ContainerType.Span),
                                new EnumIconTooltip("f:status",
                                    "https://hl7.org/fhir/R4/valueset-participationstatus.html",
                                    new ConstantText("Stav účasti")
                                )
                            ], ContainerType.Div, "d-flex align-items-center gap-1"),
                            new Container([
                                new Optional("f:actor", new NameValuePair(
                                    new ConstantText("Činitel"),
                                    new AnyReferenceNamingWidget()
                                )),
                                new Condition("f:type", new NameValuePair(
                                    new ConstantText("Typ"),
                                    new CommaSeparatedBuilder("f:type", _ => [new CodeableConcept()])
                                )),
                                new Optional("f:period", new NameValuePair(
                                    new ConstantText("Doba účasti"),
                                    new ShowPeriod()
                                ))
                            ])
                        );

                    return [tree];
                }, flexContainerClasses: "gap-2"
            );

        var infrequentProperties =
            InfrequentProperties.Evaluate<AppointmentInfrequentProperties>([navigator]);


        var headerInfo = new Container([
            new Container([
                new ConstantText("Schůzka"),
                new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.AppointmentType),
                    new ConstantText(" ("),
                    new ChangeContext("f:appointmentType", new CodeableConcept()),
                    new ConstantText(")")
                ),
            ], ContainerType.Span),
            new EnumIconTooltip("f:status", "http://hl7.org/fhir/request-status", new DisplayLabel(LabelCodes.Status))
        ], ContainerType.Div, "d-flex align-items-center gap-1");

        var badge = new PlainBadge(new ConstantText("Základní informace"));
        var basicInfo = new Container([
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.CancelationReason),
                new NameValuePair(
                    new ConstantText("Důvod zrušení"),
                    new ChangeContext("f:cancelationReason", new CodeableConcept())
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.ServiceCategory),
                new NameValuePair(
                    new ConstantText("Kategorie služby"),
                    new CommaSeparatedBuilder("f:serviceCategory", _ => [new CodeableConcept()])
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.ServiceType),
                new NameValuePair(
                    new ConstantText("Typ služby"),
                    new CommaSeparatedBuilder("f:serviceType", _ => [new CodeableConcept()])
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.Specialty),
                new NameValuePair(
                    new ConstantText("Specializace"),
                    new CommaSeparatedBuilder("f:specialty", _ => [new CodeableConcept()])
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.AppointmentType),
                new NameValuePair(
                    new ConstantText("Typ schůzky"),
                    new ChangeContext("f:appointmentType", new CodeableConcept())
                )
            ),
        ]);

        var timingBadge = new PlainBadge(new ConstantText("Časové údaje"));
        var timingInfo = new Container([
            new If(
                _ => infrequentProperties.ContainsAnyOf(AppointmentInfrequentProperties.Start,
                    AppointmentInfrequentProperties.End),
                new NameValuePair(
                    new ConstantText("Čas"),
                    new ShowPeriod()
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.MinutesDuration),
                new NameValuePair(
                    new ConstantText("Délka schůzky"),
                    new Text("f:minutesDuration/@value")
                )
            ),
            //ignore slot
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.Priority),
                new NameValuePair(
                    new ConstantText("Priorita"),
                    new Text("f:priority/@value")
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.Created),
                new NameValuePair(
                    new ConstantText("Vytvořeno"),
                    new ShowDateTime("f:created")
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.RequestedPeriod),
                new NameValuePair(
                    new ConstantText("Požadované období"),
                    new ListBuilder("f:requestedPeriod", FlexDirection.Column,
                        _ => [new Container([new ShowPeriod()], ContainerType.Span)], flexContainerClasses: "gap-0")
                )
            ),
        ]);

        var additionalDetailsBadge = new PlainBadge(new ConstantText("Další detaily"));
        var additionalDetails = new Container([
            new If(
                _ => infrequentProperties.ContainsAnyOf(AppointmentInfrequentProperties.ReasonCode,
                    AppointmentInfrequentProperties.ReasonReference),
                new NameValuePair(
                    new ConstantText("Důvod schůzky"),
                    new Condition("f:reasonReference or f:reasonCode",
                        new CommaSeparatedBuilder("f:reasonReference", _ => [new AnyReferenceNamingWidget()]),
                        new Condition("f:reasonRefernce and f:reasonCode",
                            new ConstantText(", ")
                        ),
                        new CommaSeparatedBuilder("f:reasonCode", _ => [new CodeableConcept()])
                    )
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.Description),
                new NameValuePair(
                    new ConstantText("Popis"),
                    new Text("f:description/@value")
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.SupportingInformation),
                new NameValuePair(
                    new ConstantText("Podpůrné informace"),
                    new CommaSeparatedBuilder("f:supportingInformation", _ => [new AnyReferenceNamingWidget()])
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.Comment),
                new NameValuePair(
                    new ConstantText("Komentář"),
                    new Text("f:comment/@value")
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.PatientInstruction),
                new NameValuePair(
                    new ConstantText("Pokyny pro pacienta"),
                    new Text("f:patientInstruction/@value")
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.BasedOn),
                new NameValuePair(
                    new ConstantText("Založeno na"),
                    new CommaSeparatedBuilder("f:basedOn", _ => [new AnyReferenceNamingWidget()])
                )
            ),
        ]);

        var participantsBadge = new PlainBadge(new ConstantText("Činitelé"));

        var complete =
            new Collapser([headerInfo], [], [
                    badge,
                    basicInfo,
                    new If(
                        _ => infrequentProperties.ContainsAnyOf(AppointmentInfrequentProperties.Start,
                            AppointmentInfrequentProperties.End, AppointmentInfrequentProperties.MinutesDuration,
                            AppointmentInfrequentProperties.Priority, AppointmentInfrequentProperties.Created,
                            AppointmentInfrequentProperties.RequestedPeriod),
                        new ThematicBreak(),
                        timingBadge,
                        timingInfo
                    ),
                    new If(_ => infrequentProperties.ContainsAnyOf(AppointmentInfrequentProperties.ReasonCode,
                            AppointmentInfrequentProperties.ReasonReference,
                            AppointmentInfrequentProperties.Description,
                            AppointmentInfrequentProperties.SupportingInformation,
                            AppointmentInfrequentProperties.Comment,
                            AppointmentInfrequentProperties.PatientInstruction,
                            AppointmentInfrequentProperties.BasedOn),
                        new ThematicBreak(),
                        additionalDetailsBadge,
                        additionalDetails
                    ),
                    new ThematicBreak(),
                    participantsBadge,
                    participantsInfo
                ], footer: infrequentProperties.Contains(AppointmentInfrequentProperties.Text)
                    ?
                    [
                        new NarrativeCollapser()
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()]
            );


        return await complete.Render(navigator, renderer, context);
    }
}

public enum AppointmentInfrequentProperties
{
    ServiceType,
    CancelationReason,
    ServiceCategory,
    Specialty,
    AppointmentType,
    ReasonCode,
    ReasonReference,
    Priority,
    Description,
    SupportingInformation,
    Start,
    End,
    MinutesDuration,
    Created,
    Comment,
    PatientInstruction,
    BasedOn,
    RequestedPeriod,
    Text,
}