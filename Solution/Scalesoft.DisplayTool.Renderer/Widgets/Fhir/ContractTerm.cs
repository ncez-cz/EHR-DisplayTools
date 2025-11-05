using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ContractTerm : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var headerInfo = new Container([
            new ConstantText("Smluvní ustanovení"),
            new Optional("f:type|f:subType",
                new ConstantText(" ("),
                new CodeableConcept(),
                new ConstantText(")")
            ),
        ], ContainerType.Span);

        var globalInfrequentProperties =
            Widgets.InfrequentProperties.Evaluate<InfrequentProperties>([navigator]);

        var badge = new PlainBadge(new ConstantText("Základní informace"));

        var basicInfo = new Container([
            // ignore identifier
            new Optional("f:issued", new NameValuePair(
                new ConstantText("Datum vydání"),
                new ShowDateTime()
            )),
            new Optional("f:applies", new NameValuePair(
                new ConstantText("Platnost"),
                new ShowPeriod()
            )),
            new If(_ => globalInfrequentProperties.Contains(InfrequentProperties.Topic),
                new NameValuePair(
                    new ConstantText("Téma"),
                    new OpenTypeElement(null, "topic") // CodeableConcept | Reference(Any)
                )
            ),
            new Optional("f:type", new NameValuePair(
                new ConstantText("Typ/Forma"),
                new CodeableConcept()
            )),
            new Optional("f:subType", new NameValuePair(
                new ConstantText("Podtyp"),
                new CodeableConcept()
            )),
            new Optional("f:text", new NameValuePair(
                new ConstantText("Prohlášení"),
                new Text("@value")
            )),
            new Condition("f:securityLabel",
                new Container([
                    new TextContainer(TextStyle.Bold, new ConstantText("Označení bezpečnosti:")),
                    new ListBuilder("f:securityLabel", FlexDirection.Row, _ =>
                        [
                            new Card(null, new Container([
                                new Condition("f:number", new NameValuePair(
                                    new ConstantText("Číslo"),
                                    new CommaSeparatedBuilder("f:number", _ => new Text("@value"))
                                )),
                                new ChangeContext("f:classification", new NameValuePair(
                                    new ConstantText("Klasifikace"),
                                    new Coding()
                                )),
                                new Condition("f:category", new NameValuePair(
                                    new ConstantText("Kategorie"),
                                    new CommaSeparatedBuilder("f:category", _ => new Coding())
                                )),
                                new Condition("f:control", new NameValuePair(
                                    new ConstantText("Instrukce"),
                                    new CommaSeparatedBuilder("f:control", _ => new Coding())
                                )),
                            ]))
                        ]
                    ),
                ], optionalClass: "full-grid-row")),
            
        ], optionalClass: "name-value-pair-wrapper");

        var offerBadge = new PlainBadge(new ConstantText("Nabídka"));
        var offerInfo = new Container([
            //ignore identifier
            new Condition("f:party",
                new TextContainer(TextStyle.Bold, new ConstantText("Strany:")),
                new ListBuilder("f:party", FlexDirection.Row, _ =>
                [
                    new Card(new ConstantText("Strana"), new Concat([
                        new NameValuePair(
                            new ConstantText("Reference"),
                            new CommaSeparatedBuilder("f:reference", _ => new AnyReferenceNamingWidget())
                        ),
                        new NameValuePair(
                            new ConstantText("Role"),
                            new ChangeContext("f:role", new CodeableConcept())
                        )
                    ]))
                ])
            ),
            new Optional("f:topic", new NameValuePair(
                new ConstantText("Téma"),
                new AnyReferenceNamingWidget()
            )),
            new Optional("f:type", new NameValuePair(
                new ConstantText("Typ/Forma"),
                new CodeableConcept()
            )),
            new Optional("f:decision", new NameValuePair(
                new ConstantText("Rozhodnutí"),
                new CodeableConcept()
            )),
            new Condition("f:decisionMode", new NameValuePair(
                new ConstantText("Režim rozhodnutí"),
                new CommaSeparatedBuilder("f:decisionMode", _ => new CodeableConcept())
            )),
            new Condition("f:answer", new NameValuePair(
                new ConstantText("Odpověď"),
                new CommaSeparatedBuilder("f:answer",
                    _ => new OpenTypeElement(
                        null)) // boolean | Decimal | Integer | Date | DateTime | Time | String | Uri | Attachment | Coding | Quantity | Reference(Any)
            )),
            new Optional("f:text", new NameValuePair(
                new ConstantText("Text nabídky"),
                new Text("@value")
            )),
            //ignore linkId
            //ignore securityLabelNumber
        ], optionalClass: "name-value-pair-wrapper w-max-content");

        var assetInfo = new ListBuilder("f:asset", FlexDirection.Column, _ =>
        [
            new Collapser([
                new ConstantText("Aktivum"),
                new Optional("f:type|f:subType",
                    new ConstantText(" ("),
                    new CodeableConcept(),
                    new ConstantText(")")
                ),
            ], [], [
                new Container([
                    new Optional("f:scope", new NameValuePair(
                        new ConstantText("Rozsah"),
                        new CodeableConcept()
                    )),
                    new Condition("f:type", new NameValuePair(
                        new ConstantText("Typ"),
                        new CommaSeparatedBuilder("f:type", _ => new CodeableConcept())
                    )),
                    new Condition("f:subtype", new NameValuePair(
                        new ConstantText("Podtyp"),
                        new CommaSeparatedBuilder("f:subtype", _ => new CodeableConcept())
                    )),
                    new Condition("f:typeReference", new NameValuePair(
                        new ConstantText("Přidružené reference"),
                        new CommaSeparatedBuilder("f:typeReference", _ => new AnyReferenceNamingWidget())
                    )),
                    new Optional("f:relationship", new NameValuePair(
                        new ConstantText("Vztah"),
                        new Coding()
                    )),
                    new Condition("f:context",
                        new TextContainer(TextStyle.Bold, new ConstantText("Kontexty:")),
                        new ListBuilder("f:context", FlexDirection.Row, _ =>
                        [
                            new Card(new ConstantText("Kontext"),
                                new Concat([
                                    new Optional("f:reference",
                                        new NameValuePair(
                                            new ConstantText("Tvůrce, správce nebo vlastník"),
                                            new AnyReferenceNamingWidget()
                                        )
                                    ),
                                    new Condition("f:code",
                                        new NameValuePair(
                                            new ConstantText("Kód"),
                                            new CommaSeparatedBuilder("f:code", _ => new CodeableConcept())
                                        )
                                    ),
                                    new Optional("f:text", new NameValuePair(
                                        new ConstantText("Popis"),
                                        new Text("@value")
                                    ))
                                ])
                            )
                        ])
                    ),
                    new Optional("f:condition", new NameValuePair(
                        new ConstantText("Kvalita"),
                        new Text("@value")
                    )),
                    new Condition("f:periodType", new NameValuePair(
                        new ConstantText("Typ období"),
                        new CommaSeparatedBuilder("f:periodType", _ => new CodeableConcept())
                    )),
                    new Condition("f:period", new NameValuePair(
                        new ConstantText("Období"),
                        new CommaSeparatedBuilder("f:period", _ => new ShowPeriod())
                    )),
                    new Condition("f:usePeriod", new NameValuePair(
                        new ConstantText("Období použití"),
                        new CommaSeparatedBuilder("f:usePeriod", _ => new ShowPeriod())
                    )),
                    new Optional("f:text", new NameValuePair(
                        new ConstantText("Text aktiva, nebo otázky"),
                        new Text("@value")
                    )),
                    //ignore linkId
                    //ignore securityLabelNumber
                    new Condition("f:answer", new NameValuePair(
                        new ConstantText("Odpověď"),
                        new CommaSeparatedBuilder("f:answer",
                            _ => new OpenTypeElement(
                                null)) // boolean | Decimal | Integer | Date | DateTime | Time | String | Uri | Attachment | Coding | Quantity | Reference(Any)
                    )),
                    new Condition("f:valuedItem",
                        new TextContainer(TextStyle.Bold, new ConstantText("Hodnotné položky:")),
                        new ListBuilder("f:valuedItem", FlexDirection.Row, _ =>
                        {
                            var infrequentProperties =
                                Widgets.InfrequentProperties.Evaluate<InfrequentProperties>([navigator]);

                            return
                            [
                                new Card(new ConstantText("Položka"),
                                    new Concat([
                                        new If(_ => infrequentProperties.Contains(InfrequentProperties.Entity),
                                            new NameValuePair(
                                                new ConstantText("Entita"),
                                                new OpenTypeElement(null, "entity") // CodeableConcept | Reference(Any)
                                            )
                                        ),
                                        //ignore identifier
                                        new Optional("f:effectiveTime", new NameValuePair(
                                            new ConstantText("Čas efektivity"),
                                            new ShowDateTime()
                                        )),
                                        new Optional("f:quantity", new NameValuePair(
                                            new ConstantText("Množství"),
                                            new ShowQuantity()
                                        )),
                                        new Optional("f:unitPrice", new NameValuePair(
                                            new ConstantText("Jednotková cena"),
                                            new ShowMoney()
                                        )),
                                        new Optional("f:factor", new NameValuePair(
                                            new ConstantText("Faktor"),
                                            new ShowDecimal()
                                        )),
                                        new Optional("f:net", new NameValuePair(
                                            new ConstantText("Čistá hodnota"),
                                            new ShowMoney()
                                        )),
                                        new Optional("f:payment", new NameValuePair(
                                            new ConstantText("Platba"),
                                            new Text("@value")
                                        )),
                                        new Optional("f:paymentDate", new NameValuePair(
                                            new ConstantText("Datum platby"),
                                            new ShowDateTime()
                                        )),
                                        new Optional("f:responsible", new NameValuePair(
                                            new ConstantText("Zodpovědný"),
                                            new AnyReferenceNamingWidget()
                                        )),
                                        new Optional("f:recipient", new NameValuePair(
                                            new ConstantText("Příjemce"),
                                            new AnyReferenceNamingWidget()
                                        )),
                                        // ignore linkId
                                        // ignore securityLabelNumber
                                    ])
                                )
                            ];
                        })
                    )
                ], optionalClass: "name-value-pair-wrapper")
            ]),
        ]);

        var actionBadge = new PlainBadge(new ConstantText("Akce"));
        var actionInfo = new ListBuilder("f:action", FlexDirection.Row, (_, nav) =>
        {
            var infrequentProperties =
                Widgets.InfrequentProperties.Evaluate<InfrequentProperties>([nav]);

            Widget[] tree =
            [
                new Optional(
                    "f:doNotPerform", new NameValuePair(
                        new ConstantText("Zákaz"),
                        new ShowDoNotPerform()
                    )),
                new NameValuePair(
                    new ConstantText("Typ/Forma"),
                    new CodeableConcept()
                ),
                new Condition("f:subject",
                    new TextContainer(TextStyle.Bold, new ConstantText("Předmět:")),
                    new ListBuilder("f:subject", FlexDirection.Row, _ =>
                    [
                        new NameValuePair(
                            new ConstantText("Reference"),
                            new CommaSeparatedBuilder("f:reference", _ => new AnyReferenceNamingWidget())
                        ),
                        new Optional("f:role", new NameValuePair(
                            new ConstantText("Role"),
                            new CodeableConcept()
                        ))
                    ])
                ),
                new NameValuePair(
                    new ConstantText("Záměr"),
                    new ChangeContext("f:intent", new CodeableConcept())
                ),
                // ignore linkId
                new NameValuePair(
                    new ConstantText("Stav"),
                    new ChangeContext("f:status", new CodeableConcept())
                ),
                new Optional("f:context", new NameValuePair(
                    new ConstantText("Kontext"),
                    new AnyReferenceNamingWidget()
                )),
                // ignore contextLinkId
                new If(_ => infrequentProperties.Contains(InfrequentProperties.Occurrence),
                    new NameValuePair(
                        new ConstantText("Žádaný rozvrh/čas"),
                        new Chronometry("occurrence")
                    )
                ),
                new Condition("f:requester", new NameValuePair(
                    new ConstantText("Žadatelé"),
                    new CommaSeparatedBuilder("f:requester", _ => new AnyReferenceNamingWidget())
                )),
                // ignore requesterLinkId
                new Condition("f:performerType", new NameValuePair(
                    new ConstantText("Typ provádějícího"),
                    new CommaSeparatedBuilder("f:performerType", _ => new CodeableConcept())
                )),
                new Optional("f:performerRole", new NameValuePair(
                    new ConstantText("Role provádějícího"),
                    new CodeableConcept()
                )),
                new Optional("f:performer", new NameValuePair(
                    new ConstantText("Provádějící"),
                    new AnyReferenceNamingWidget()
                )),
                // ignore performerLinkId
                new Condition("f:reasonCode|f:reasonReference", new NameValuePair(
                    new ConstantText("Důvod (ne)potřebnosti"),
                    new Concat([
                        new CommaSeparatedBuilder("f:reasonCode", _ => new CodeableConcept()),
                        new CommaSeparatedBuilder("f:reasonReference", _ => new AnyReferenceNamingWidget())
                    ], ", ")
                )),
                new Condition("f:reason", new NameValuePair(
                    new ConstantText("Důvod"),
                    new CommaSeparatedBuilder("f:reason", _ => new Text("@value"))
                )),
                // ignore reasonLinkId
                new Condition("f:note", new NameValuePair(
                    new ConstantText("Poznámka"),
                    new ListBuilder("f:note", FlexDirection.Column, _ => [new ShowAnnotationCompact()])
                ))
                // ignore securityLabelNumber
            ];

            return tree;
        });

        var complete =
            new Container([
                new Collapser([headerInfo], [], [
                    new Condition("f:issued or f:applies or f:type or f:subType or f:text or f:securityLabel",
                        badge,
                        basicInfo,
                        new Condition("f:offer or f:asset or f:action or f:group",
                            new ThematicBreak()
                        )
                    ),
                    new ChangeContext("f:offer",
                        offerBadge,
                        offerInfo,
                        new If(_ => navigator.EvaluateCondition("f:asset or f:action or f:group"),
                            new ThematicBreak()
                        )
                    ),
                    new Condition("f:asset",
                        new PlainBadge(new ConstantText("Aktiva")),
                        assetInfo,
                        new Condition("f:action or f:group",
                            new ThematicBreak()
                        )
                    ),
                    new Condition("f:action",
                        actionBadge,
                        actionInfo,
                        new Condition("f:group",
                            new ThematicBreak()
                        )
                    ),
                    new Condition("f:group",
                        new PlainBadge(new ConstantText("Smluvní ustanovení")),
                        new ListBuilder("f:group", FlexDirection.Column, _ => [new ContractTerm()])
                    )
                ])
            ]);


        return complete.Render(navigator, renderer, context);
    }

    private enum InfrequentProperties
    {
        [OpenType("topic")] Topic,
        [OpenType("entity")] Entity,
        [OpenType("occurrence")] Occurrence,
    }
}