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

    public static bool HasBorderedContainer(Widget widget) => true;

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<AppointmentInfrequentProperties>(navigator);


        var headerInfo = new Container([
            new Container([
                new LocalizedLabel("appointment"),
                new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.AppointmentType),
                    new Container([
                        new ChangeContext("f:appointmentType", new CodeableConcept())
                    ], ContainerType.Span, "fw-bold black ms-4")
                ),
            ], ContainerType.Span),
            new EnumIconTooltip("f:status", "http://hl7.org/fhir/request-status",
                new EhdsiDisplayLabel(LabelCodes.Status))
        ], ContainerType.Div, "d-flex align-items-center gap-1");

        var nameValuePairClasses = new NameValuePair.NameValuePairClasses
        {
            OuterClass = "mw-50",
        };

        var info = new Row([
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.CancelationReason),
                new NameValuePair(
                    new LocalizedLabel("appointment.cancelationReason"),
                    new ChangeContext("f:cancelationReason", // Yes, there is a typo in documentation 
                        new CodeableConcept()), direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.ServiceCategory),
                new NameValuePair(
                    new LocalizedLabel("appointment.serviceCategory"),
                    new CommaSeparatedBuilder("f:serviceCategory", _ => [new CodeableConcept()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.ServiceType),
                new NameValuePair(
                    new LocalizedLabel("appointment.serviceType"),
                    new CommaSeparatedBuilder("f:serviceType", _ => [new CodeableConcept()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.Specialty),
                new NameValuePair(
                    new LocalizedLabel("appointment.specialty"),
                    new CommaSeparatedBuilder("f:specialty", _ => [new CodeableConcept()]),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.AppointmentType),
                new NameValuePair(
                    new LocalizedLabel("appointment.appointmentType"),
                    new ChangeContext("f:appointmentType", new CodeableConcept()), direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            new If(
                _ => infrequentProperties.ContainsAnyOf(AppointmentInfrequentProperties.Start,
                    AppointmentInfrequentProperties.End),
                new NameValuePair(
                    new LocalizedLabel("appointment.start"),
                    new ShowPeriod(), direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.MinutesDuration),
                new NameValuePair(
                    new LocalizedLabel("appointment.minutesDuration"),
                    new Text("f:minutesDuration/@value"), direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            //ignore slot
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.Priority),
                new NameValuePair(
                    new LocalizedLabel("appointment.priority"),
                    new Text("f:priority/@value"), direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.Created),
                new NameValuePair(
                    new LocalizedLabel("appointmnet.created"),
                    new ShowDateTime("f:created"), direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            new If(_ => infrequentProperties.Contains(AppointmentInfrequentProperties.RequestedPeriod),
                new NameValuePair(
                    new LocalizedLabel("appointment.requestedPeriod"),
                    new ListBuilder("f:requestedPeriod", FlexDirection.Column,
                        _ => [new Container([new ShowPeriod()], ContainerType.Span)], flexContainerClasses: "gap-0"),
                    direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            new If(
                _ => infrequentProperties.ContainsAnyOf(AppointmentInfrequentProperties.ReasonCode,
                    AppointmentInfrequentProperties.ReasonReference),
                new NameValuePair(
                    new LocalizedLabel("appointment.reason"),
                    new CommaSeparatedBuilder("f:reasonReference|f:reasonCode", (_, _, x) =>
                        {
                            return x.Node?.LocalName switch
                            {
                                "reasonReference" =>
                                [
                                    new AnyReferenceNamingWidget(),
                                ],
                                "reasonCode" =>
                                [
                                    new CodeableConcept(),
                                ],
                                _ => [],
                            };
                        }
                    ), direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            infrequentProperties.Optional(AppointmentInfrequentProperties.Description,
                new NameValuePair(
                    new LocalizedLabel("appointment.description"),
                    new Text("@value"), direction: 
                    FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            infrequentProperties.Condition(AppointmentInfrequentProperties.SupportingInformation,
                new NameValuePair(
                    new LocalizedLabel("appointment.supportingInformation"),
                    new CommaSeparatedBuilder("f:supportingInformation", _ => [new AnyReferenceNamingWidget()]),
                    direction: FlexDirection.Column, 
                    style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            infrequentProperties.Optional(AppointmentInfrequentProperties.Comment,
                new NameValuePair(
                    new LocalizedLabel("appointment.comment"),
                    new Text("@value"), 
                    direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            infrequentProperties.Optional(AppointmentInfrequentProperties.PatientInstruction,
                new NameValuePair(
                    new LocalizedLabel("appointment.patientInstruction"),
                    new Text("@value"), 
                    direction: FlexDirection.Column,
                    style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            infrequentProperties.Condition(AppointmentInfrequentProperties.BasedOn,
                new NameValuePair(
                    new LocalizedLabel("appointment.basedOn"),
                    new CommaSeparatedBuilder("f:basedOn", _ => [new AnyReferenceNamingWidget()]),
                    direction: FlexDirection.Column, 
                    style: NameValuePair.NameValuePairStyle.Primary,
                    optionalClasses: nameValuePairClasses
                )
            ),
            new ConcatBuilder("f:participant", _ =>
                {
                    var tree = new NameValuePair(
                        [
                                    new LocalizedLabel("appointment.participant.actor"),
                        ], [
                            new Optional("f:actor", new AnyReferenceNamingWidget(showInlineType: false)),
                        ], direction: FlexDirection.Column, style: NameValuePair.NameValuePairStyle.Primary,
                        optionalClasses: nameValuePairClasses
                    );

                    return [tree];
                }
            ),
        ], flexContainerClasses: "name-value-pair-wrapper row-gap-1");

        var complete =
            new Card(
                new Concat([
                    headerInfo,
                    new NarrativeModal(),
                ]),
                info,
                footer: infrequentProperties.Contains(AppointmentInfrequentProperties.Text)
                    ? new NarrativeCollapser()
                    : null
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