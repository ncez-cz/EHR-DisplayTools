using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class HealthcareService : ColumnResourceBase<HealthcareService>, IResourceWidget
{
    public static string ResourceType => "HealthcareService";

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var headerInfo = new Container([
            new ConstantText("Zdravotnická služba"),
            new Optional("f:name",
                new ConstantText(" ("),
                new Text("@value"),
                new ConstantText(")")
            ),
        ], ContainerType.Span);

        var badge = new PlainBadge(new ConstantText("Základní informace"));

        var basicInfo = new Container([
            new Optional("f:active", new TextContainer(TextStyle.Bold,
                new ShowBoolean("Neaktivní", "Aktivní")
            )),
            new Optional("f:name", new NameValuePair(
                new DisplayLabel(LabelCodes.Name),
                new Text("@value")
            )),
            new Optional("f:comment",
                new NameValuePair(
                    new DisplayLabel(LabelCodes.Description),
                    new Text("@value")
                )
            ),
            new Optional("f:providedBy", new NameValuePair(
                new ConstantText("Poskytovatel"),
                new AnyReferenceNamingWidget()
            )),
            new Condition("f:category", new NameValuePair(
                new ConstantText("Kategorie"),
                new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()])
            )),
            new Condition("f:type", new NameValuePair(
                new ConstantText("Typ"),
                new CommaSeparatedBuilder("f:type", _ => [new CodeableConcept()])
            )),
            new Condition("f:specialty", new NameValuePair(
                new ConstantText("Specializace"),
                new CommaSeparatedBuilder("f:specialty", _ => [new CodeableConcept()])
            )),
            new Condition("f:location", new NameValuePair(
                new ConstantText("Umístění"),
                new CommaSeparatedBuilder("f:location", _ => [new AnyReferenceNamingWidget()])
            )),
        ]);

        var detailBadge = new PlainBadge(new ConstantText("Detailní informace"));
        var detailInfo = new Container([
            new Optional("f:extraDetails",
                new NameValuePair(
                    new ConstantText("Další podrobnosti"),
                    new Markdown("@value")
                )
            ),
            new Optional("f:photo",
                new NameValuePair(
                    new ConstantText("Fotografie"),
                    new Attachment()
                )
            ),
            new Condition("f:telecom", new NameValuePair(
                new DisplayLabel(LabelCodes.Telecom),
                new ShowContactPoint()
            )),
            new Condition("f:coverageArea", new NameValuePair(
                new ConstantText("Pokrytí oblastí"),
                new CommaSeparatedBuilder("f:coverageArea", _ => [new AnyReferenceNamingWidget()])
            )),
            new Condition("f:serviceProvisionCode", new NameValuePair(
                new ConstantText("Podmínky poskytování služeb"),
                new CommaSeparatedBuilder("f:serviceProvisionCode", _ => [new CodeableConcept()])
            )),
            new Condition("f:eligibility",
                new TextContainer(TextStyle.Bold, new ConstantText("Podmínky způsobilosti:")),
                new ItemListBuilder("f:eligibility", ItemListType.Unordered, _ =>
                [
                    new Concat([
                        new Optional("f:code",
                            new NameValuePair(
                                new ConstantText("Kód"),
                                new CodeableConcept()
                            )
                        ),
                        new Optional("f:comment",
                            new NameValuePair(
                                new ConstantText("Komentář"),
                                new Markdown("@value")
                            )
                        )
                    ])
                ])
            ),
            new Condition("f:program", new NameValuePair(
                new ConstantText("Programy"),
                new CommaSeparatedBuilder("f:program", _ => [new CodeableConcept()])
            )),
            new Condition("f:characteristic", new NameValuePair(
                new ConstantText("Charakteristiky"),
                new CommaSeparatedBuilder("f:characteristic", _ => [new CodeableConcept()])
            )),
            new Condition("f:referralMethod", new NameValuePair(
                new ConstantText("Způsoby doporučení"),
                new CommaSeparatedBuilder("f:referralMethod", _ => [new CodeableConcept()])
            )),
            //ignore endpoint
        ]);

        var operationBadge = new PlainBadge(new ConstantText("Provozní informace"));
        var operationInfo = new Container([
            new Optional("f:appointmentRequired",
                new TextContainer(TextStyle.Bold, [
                        new ShowBoolean("Nevyžaduje rezervaci", "Vyžaduje rezervaci"),
                    ]
                ),
                new LineBreak()
            ),
            new Condition("f:availableTime",
                new TextContainer(TextStyle.Bold, new ConstantText("Dostupné časy:")),
                new Row([
                    new HealthcareServiceAvailableTime()
                ])
            ),
            new Condition("f:notAvailable",
                new TextContainer(TextStyle.Bold, new ConstantText("Nedostupné časy:")),
                new Row([
                    new ListBuilder("f:notAvailable[f:during]", FlexDirection.Row, _ =>
                        [
                            new Condition("f:during",
                                new Card(new ShowPeriod("f:during"),
                                    new Markdown("f:description/@value"), "time-card")
                            ),
                        ]
                    ),
                    new Condition("not(f:during)",
                        new Card(new ConstantText("Bez doby"), new ItemListBuilder("f:notAvailable[not(f:during)]",
                            ItemListType.Unordered, _ => [new Markdown("f:description/@value")]
                        ), "time-card")
                    )
                ])
            ),
            new Optional("f:availabilityExceptions",
                new NameValuePair(
                    new ConstantText("Výjimky dostupnosti"),
                    new Text("@value")
                )
            ),
        ]);


        var complete =
            new Container([
                new Collapser([headerInfo], [], [
                        new Condition(
                            "f:active or f:name or f:comment or f:providedBy or f:category or f:type or f:specialty or f:location",
                            badge,
                            basicInfo,
                            new Condition(
                                "f:extraDetails or f:photo or f:telecom or f:coverageArea or f:serviceProvisionCode or f:eligibility or " +
                                "f:program or f:characteristic or f:referralMethod or f:appointmentRequired or f:availableTime or f:notAvailable or f:availabilityExceptions",
                                new ThematicBreak()
                            )
                        ),
                        new Condition(
                            "f:extraDetails or f:photo or f:telecom or f:coverageArea or f:serviceProvisionCode or f:eligibility or f:program or " +
                            "f:characteristic or f:referralMethod",
                            detailBadge,
                            detailInfo,
                            new Condition(
                                "f:appointmentRequired or f:availableTime or f:notAvailable or f:availabilityExceptions",
                                new ThematicBreak()
                            )
                        ),
                        new Condition(
                            "f:appointmentRequired or f:availableTime or f:notAvailable or f:availabilityExceptions",
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
                )
            ]);


        return complete.Render(navigator, renderer, context);
    }
}