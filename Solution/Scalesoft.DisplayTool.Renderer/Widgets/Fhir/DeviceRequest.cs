using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class DeviceRequest : ColumnResourceBase<DeviceRequest>, IResourceWidget
{
    public static string ResourceType => "DeviceRequest";
    
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<DeviceRequestInfrequentProperties>([navigator]);

        var headerInfo = new Container([
            new Container([
                new ConstantText("Žádost o přístroj"),
                new If(_ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.Code),
                    new ConstantText(" ("),
                    new OpenTypeElement(null, "code"), // 	Reference(Device) | CodeableConcept
                    new ConstantText(")")
                ),
            ], ContainerType.Span),
            new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/request-status",
                new ConstantText("Stav žádosti"))
        ], ContainerType.Div, "d-flex align-items-center gap-1");


        var badge = new PlainBadge(new ConstantText("Základní informace"));
        var basicInfo = new Container([
            new If(_ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.Priority),
                new NameValuePair(
                    new ConstantText("Priorita"),
                    new EnumLabel("f:priority", "http://hl7.org/fhir/ValueSet/request-priority")
                )
            ),
            new If(_ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.Intent),
                new NameValuePair(
                    new ConstantText("Záměr"),
                    new EnumLabel("f:intent", "http://hl7.org/fhir/ValueSet/request-intent")
                )
            ),
            new If(_ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.Reason),
                new NameValuePair(
                    new ConstantText("Důvody žádosti"),
                    new Concat([
                        new Optional("f:reasonReference",
                            new ListBuilder(".", FlexDirection.Column, _ => [new AnyReferenceNamingWidget()])
                        ),
                        new Optional("f:reasonCode",
                            new ListBuilder(".", FlexDirection.Column, _ => [new CodeableConcept()])
                        )
                    ])
                )
            ),
            new If(_ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.Occurrence),
                new NameValuePair(
                    new ConstantText("Žádaný rozvrh/čas"),
                    new Chronometry("occurrence")
                )
            ),
            new If(_ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.AuthoredOn),
                new NameValuePair(
                    new ConstantText("Datum a čas žádosti"),
                    new ShowDateTime("f:authoredOn")
                )
            ),
            new If(
                _ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.DoNotPerform) &&
                     navigator.EvaluateCondition("f:doNotPerform[@value='true']"),
                new NameValuePair(
                    new ConstantText("Zákaz"),
                    new ShowDoNotPerform()
                )
            ),
        ]);

        var deviceBadge = new Concat([
            new PlainBadge(new ConstantText("Zařízení")),
            new EnumIconTooltip("f:status",
                "http://hl7.org/fhir/ValueSet/device-status-reason",
                new ConstantText("Stav zařízení")
            )
        ]);

        var deviceInfo = new Container([
            new If(_ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.Code),
                new Choose([
                    new When("f:codeReference",
                        ShowSingleReference.WithDefaultDisplayHandler(
                            x => [new Container(new DeviceTextInfo(), idSource: x)],
                            "f:codeReference")),
                    new When("f:codeCodeableConcept",
                        new ChangeContext("f:codeCodeableConcept", new NameValuePair(
                            new DisplayLabel(LabelCodes.DeviceName),
                            new CodeableConcept()
                        )))
                ])
            )
        ]);

        var actorBadge = new PlainBadge(new ConstantText("Činitelé"));
        var actorInfo = new Container([
            new If(_ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.Requester),
                new NameValuePair(
                    new ConstantText("Žadatel"),
                    new AnyReferenceNamingWidget("f:requester")
                )
            ),
            new If(_ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.Performer),
                new NameValuePair(
                    new ConstantText("Vyřizovatel"),
                    new AnyReferenceNamingWidget("f:performer")
                )
            ),
        ]);

        var complete =
            new Collapser([headerInfo], [], [
                    badge,
                    basicInfo,
                    new ThematicBreak(),
                    deviceBadge,
                    deviceInfo,
                    new If(
                        _ => infrequentProperties.ContainsAnyOf(DeviceRequestInfrequentProperties.Requester,
                            DeviceRequestInfrequentProperties.Performer),
                        new ThematicBreak(),
                        actorBadge,
                        actorInfo
                    )
                ], footer: infrequentProperties.ContainsAnyOf(DeviceRequestInfrequentProperties.Encounter,
                    DeviceRequestInfrequentProperties.Text)
                    ?
                    [
                        new If(_ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.Encounter),
                            new ShowMultiReference("f:encounter",
                                (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                                x =>
                                [
                                    new Collapser([new ConstantText(Labels.Encounter)], [], x.ToList(),
                                        isCollapsed: true)
                                ]
                            )
                        ),
                        new If(_ => infrequentProperties.Contains(DeviceRequestInfrequentProperties.Text),
                            new NarrativeCollapser()
                        ),
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()]
            );


        return await complete.Render(navigator, renderer, context);
    }
}

public enum DeviceRequestInfrequentProperties
{
    Status,
    Requester,
    Performer,
    Priority,
    AuthoredOn,
    Intent,
    [OpenType("occurrence")] Occurrence,
    [OpenType("code")] Code,
    [OpenType("reason")] Reason,
    DoNotPerform,
    Text,
    Encounter
}