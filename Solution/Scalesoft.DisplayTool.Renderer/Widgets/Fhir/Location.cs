using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Location : SequentialResourceBase<Location>, IResourceWidget
{
    public static string ResourceType => "Location";

    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer, RenderContext context)
    {
        var headerInfo = new Container([
            new ConstantText("Lokace"),
            new Optional("f:name",
                new ConstantText(" ("),
                new Text("@value"),
                new ConstantText(")")
            ),
        ], ContainerType.Span);

        var badge = new PlainBadge(new ConstantText("Základní informace"));

        var basicInfo = new Container([
            new Optional("f:status", new NameValuePair(
                new DisplayLabel(LabelCodes.Status),
                new EnumLabel(".", "http://hl7.org/fhir/ValueSet/location-status")
            )),
            new Optional("f:operationalStatus", new NameValuePair(
                new ConstantText("Operační stav"),
                new Coding()
            )),
            new Optional("f:name", new NameValuePair(
                new DisplayLabel(LabelCodes.Name),
                new Text("@value")
            )),
            new Condition("f:alias", new NameValuePair(
                new ConstantText("Alternativní názvy"),
                new CommaSeparatedBuilder("f:alias", _ => [new Text("@value")])
            )),
            new Optional("f:description", new NameValuePair(
                new DisplayLabel(LabelCodes.Description),
                new Text("@value")
            )),
        ]);

        var locationBadge = new PlainBadge(new ConstantText("Informace o umístění"));
        var locationInfo = new Container([
            new Condition("f:type", new NameValuePair(
                new ConstantText("Typ"),
                new CommaSeparatedBuilder("f:type", _ => [new CodeableConcept()])
            )),
            new Condition("f:telecom", new NameValuePair(
                new DisplayLabel(LabelCodes.Telecom),
                new ShowContactPoint()
            )),
            new Optional("f:address",
                new Address()
            ),
            new Optional("f:physicalType", new NameValuePair(
                new ConstantText("Typ fyzického umístění"),
                new CodeableConcept()
            )),
            new Optional("f:position",
                new NameValuePair(
                    [new ConstantText("Geografická poloha")],
                    [
                        new Optional("f:longitude",
                            new NameValuePair(
                                new ConstantText("Zeměpisná délka"),
                                new Concat([new Text("@value"), new ConstantText("°")], string.Empty)
                            )
                        ),
                        new Optional("f:latitude",
                            new NameValuePair(
                                new ConstantText("Zeměpisná šířka"),
                                new Concat([new Text("@value"), new ConstantText("°")], string.Empty)
                            )
                        ),
                        new Optional("f:altitude",
                            new NameValuePair(
                                new ConstantText("Nadmořská výška"),
                                new Concat([new Text("@value"), new ConstantText("m")])
                            )
                        )
                    ]
                )
            ),
        ]);

        var operationBadge = new PlainBadge(new ConstantText("Provozní informace"));
        var operationInfo = new Container([
            new Optional("f:managingOrganization",
                new NameValuePair(
                    new ConstantText("Spravující organizace"),
                    new AnyReferenceNamingWidget()
                )
            ),
            new Optional("f:partOf",
                new NameValuePair(
                    new ConstantText("Součástí"),
                    new AnyReferenceNamingWidget()
                )
            ),
            new Condition("f:hoursOfOperation",
                new ConcatBuilder("f:hoursOfOperation", _ =>
                    [
                        new NameValuePair(
                            new ConstantText("Dny v týdnu"),
                            new CommaSeparatedBuilder("f:daysOfWeek",
                                _ => [new EnumLabel("@value", "http://hl7.org/fhir/ValueSet/days-of-week")])
                        ),
                        new NameValuePair(
                            [new ConstantText("Časové úseky")],
                            [
                                new Optional("f:allDay",
                                    new ShowBoolean(new NullWidget(),
                                        new TextContainer(TextStyle.Bold, [new ConstantText("Celý den")]))),
                                new Concat(
                                [
                                    new Optional("f:openingTime",
                                        new Text("@value")),
                                    new Condition("f:openingTime and f:closingTime",
                                        new ConstantText(" - ")
                                    ),
                                    new Optional("f:closingTime",
                                        new Text("@value")
                                    )
                                ])
                            ]
                        )
                    ]
                )
            ),
            new Optional("f:availabilityExceptions",
                new NameValuePair(
                    new ConstantText("Výjimky dostupnosti"),
                    new Text("@value")
                )
            ),
        ]);


        var complete =
            new Collapser([headerInfo], [], [
                    new Condition("f:name or f:alias or f:description or f:status or f:operationalStatus",
                        badge,
                        basicInfo
                    ),
                    new Condition("f:type or f:telecom or f:address or f:physicalType or f:position",
                        new ThematicBreak(),
                        locationBadge,
                        locationInfo
                    ),
                    new Condition(
                        "f:managingOrganization or f:partOf or f:hoursOfOperation or f:availabilityExceptions",
                        new ThematicBreak(),
                        operationBadge,
                        operationInfo
                    )
                ], footer: navigator.EvaluateCondition("f:text")
                    ?
                    [
                        new NarrativeCollapser()
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()]
            );


        return complete.Render(navigator, renderer, context);
    }
}