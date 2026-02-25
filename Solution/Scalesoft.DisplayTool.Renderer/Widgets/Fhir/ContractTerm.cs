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
            new LocalizedLabel("contract.term"),
            new Optional("f:type|f:subType",
                new ConstantText(" ("),
                new CodeableConcept(),
                new ConstantText(")")
            ),
        ], ContainerType.Span);

        var globalInfrequentProperties = InfrequentProperties.Evaluate<ContractTermInfrequentProperties>(navigator);

        var badge = new PlainBadge(new LocalizedLabel("general.basic-information"));

        var basicInfo = new Container([
            // ignore identifier
            globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Issued,
                new NameValuePair(
                    new LocalizedLabel("contract.term.issued"),
                    new ShowDateTime()
                )
            ),
            globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Applies,
                new NameValuePair(
                    new LocalizedLabel("contract.term.applies"),
                    new ShowPeriod()
                )
            ),
            globalInfrequentProperties.Condition(ContractTermInfrequentProperties.Topic,
                new NameValuePair(
                    new LocalizedLabel("contract.term.topic"),
                    new OpenTypeElement(null, "topic") // CodeableConcept | Reference(Any)
                )
            ),
            globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Type,
                new NameValuePair(
                    new LocalizedLabel("contract.term.type"),
                    new CodeableConcept()
                )
            ),
            globalInfrequentProperties.Optional(ContractTermInfrequentProperties.SubType,
                new NameValuePair(
                    new LocalizedLabel("contract.term.subType"),
                    new CodeableConcept()
                )
            ),
            globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Text,
                new NameValuePair(
                    new LocalizedLabel("contract.term.text"),
                    new Text("@value")
                )
            ),
            globalInfrequentProperties.Condition(ContractTermInfrequentProperties.SecurityLabel,
                new Container([
                    new TextContainer(
                        TextStyle.Bold,
                        [new LocalizedLabel("contract.term.securityLabel-plural"), new ConstantText(":")]
                    ),
                    new ListBuilder("f:securityLabel", FlexDirection.Row, _ =>
                        [
                            new Card(null, new Container([
                                new Condition("f:number", new NameValuePair(
                                    new LocalizedLabel("contract.term.securityLabel.number"),
                                    new CommaSeparatedBuilder("f:number", _ => new Text("@value"))
                                )),
                                new ChangeContext("f:classification", new NameValuePair(
                                    new LocalizedLabel("contract.term.securityLabel.classification"),
                                    new Coding()
                                )),
                                new Condition("f:category", new NameValuePair(
                                    new LocalizedLabel("contract.term.securityLabel.category"),
                                    new CommaSeparatedBuilder("f:category", _ => new Coding())
                                )),
                                new Condition("f:control", new NameValuePair(
                                    new LocalizedLabel("contract.term.securityLabel.control"),
                                    new CommaSeparatedBuilder("f:control", _ => new Coding())
                                )),
                            ])),
                        ]
                    ),
                ], optionalClass: "span-over-full-name-value-pair-cell")),
        ], optionalClass: "name-value-pair-wrapper");

        var offerBadge = new PlainBadge(new LocalizedLabel("contract.term.offer"));
        var offerInfo = new Container([
            //ignore identifier
            new Condition("f:party",
                new TextContainer(TextStyle.Bold,
                    [new LocalizedLabel("contract.term.offer.party-plural"), new ConstantText(":")]),
                new ListBuilder("f:party", FlexDirection.Row, _ =>
                [
                    new Card(new LocalizedLabel("contract.term.offer.party"), new Concat([
                        new NameValuePair(
                            new LocalizedLabel("contract.term.offer.party.reference"),
                            new CommaSeparatedBuilder("f:reference", _ => new AnyReferenceNamingWidget())
                        ),
                        new NameValuePair(
                            new LocalizedLabel("contract.term.offer.party.role"),
                            new ChangeContext("f:role", new CodeableConcept())
                        ),
                    ])),
                ])
            ),
            globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Topic,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("contract.term.offer.topic"),
                    }
                )
            ),
            globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Type,
                new NameValuePair(
                    new LocalizedLabel("contract.term.offer.type"),
                    new CodeableConcept()
                )
            ),
            globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Decision,
                new NameValuePair(
                    new LocalizedLabel("contract.term.offer.decision"),
                    new CodeableConcept()
                )
            ),
            new Condition("f:decisionMode", new NameValuePair(
                    new LocalizedLabel("contract.term.offer.decisionMode"),
                    new CommaSeparatedBuilder("f:decisionMode", _ => new CodeableConcept())
                )
            ),
            new Condition("f:answer", new NameValuePair(
                    new LocalizedLabel("contract.term.offer.answer"),
                    new CommaSeparatedBuilder("f:answer", _ => new OpenTypeElement(null))
                    // boolean | Decimal | Integer | Date | DateTime | Time | String | Uri | Attachment | Coding | Quantity | Reference(Any)
                )
            ),
            new Optional("f:text",
                new NameValuePair(
                    new LocalizedLabel("contract.term.offer.text"),
                    new Text("@value")
                )
            ),
            //ignore linkId
            //ignore securityLabelNumber
        ], optionalClass: "name-value-pair-wrapper w-fit-content");

        var assetInfo = new ListBuilder("f:asset", FlexDirection.Column, _ =>
        [
            new Collapser([
                new LocalizedLabel("contract.term.asset"),
                new Optional("f:type|f:subType",
                    new ConstantText(" ("),
                    new CodeableConcept(),
                    new ConstantText(")")
                ),
            ], [
                new Container([
                    globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Scope,
                        new NameValuePair(
                            new LocalizedLabel("contract.term.asset.scope"),
                            new CodeableConcept()
                        )),
                    new Condition("f:type", new NameValuePair(
                        new LocalizedLabel("contract.term.asset.type"),
                        new CommaSeparatedBuilder("f:type", _ => new CodeableConcept())
                    )),
                    new Condition("f:subtype", new NameValuePair(
                        new LocalizedLabel("contract.term.asset.subtype"),
                        new CommaSeparatedBuilder("f:subtype", _ => new CodeableConcept())
                    )),
                    new Condition("f:typeReference", new NameValuePair(
                        new LocalizedLabel("contract.term.asset.typeReference"),
                        new CommaSeparatedBuilder("f:typeReference", _ => new AnyReferenceNamingWidget())
                    )),
                    globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Relationship,
                        new NameValuePair(
                            new LocalizedLabel("contract.term.asset.relationship"),
                            new Coding()
                        )),
                    new Condition("f:context",
                        new TextContainer(
                            TextStyle.Bold,
                            [new LocalizedLabel("contract.term.asset.context-plural"), new ConstantText(":")]
                        ),
                        new ListBuilder("f:context", FlexDirection.Row, _ =>
                        [
                            new Card(new LocalizedLabel("contract.term.asset.context"),
                                new Concat([
                                    globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Reference,
                                        new AnyReferenceNamingWidget(
                                            widgetModel: new ReferenceNamingWidgetModel
                                            {
                                                Type = ReferenceNamingWidgetType.NameValuePair,
                                                LabelOverride =
                                                    new LocalizedLabel("contract.term.asset.context.reference"),
                                            }
                                        )
                                    ),
                                    new Condition("f:code",
                                        new NameValuePair(
                                            new LocalizedLabel("contract.term.asset.context.code"),
                                            new CommaSeparatedBuilder("f:code", _ => new CodeableConcept())
                                        )
                                    ),
                                    globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Text,
                                        new NameValuePair(
                                            new LocalizedLabel("contract.text.asset.context.text"),
                                            new Text("@value")
                                        ))
                                ])
                            )
                        ])
                    ),
                    globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Condition,
                        new NameValuePair(
                            new LocalizedLabel("contract.term.asset.condition"),
                            new Text("@value")
                        )),
                    new Condition("f:periodType", new NameValuePair(
                        new LocalizedLabel("contract.term.asset.periodType"),
                        new CommaSeparatedBuilder("f:periodType", _ => new CodeableConcept())
                    )),
                    new Condition("f:period", new NameValuePair(
                        new LocalizedLabel("contract.term.asset.period"),
                        new CommaSeparatedBuilder("f:period", _ => new ShowPeriod())
                    )),
                    new Condition("f:usePeriod", new NameValuePair(
                        new LocalizedLabel("contract.term.asset.usePeriod"),
                        new CommaSeparatedBuilder("f:usePeriod", _ => new ShowPeriod())
                    )),
                    globalInfrequentProperties.Optional(ContractTermInfrequentProperties.Text,
                        new NameValuePair(
                            new LocalizedLabel("contract.term.asset.text"),
                            new Text("@value")
                        )),
                    //ignore linkId
                    //ignore securityLabelNumber
                    new Condition("f:answer", new NameValuePair(
                        new LocalizedLabel("contract.term.asset.answer"),
                        new CommaSeparatedBuilder("f:answer",
                            _ => new OpenTypeElement(
                                null)) // boolean | Decimal | Integer | Date | DateTime | Time | String | Uri | Attachment | Coding | Quantity | Reference(Any)
                    )),
                    new Condition(
                        "f:valuedItem",
                        new TextContainer(
                            TextStyle.Bold,
                            [new LocalizedLabel("contract.term.asset.valuedItem-plural"), new ConstantText(":")]
                        ),
                        new ListBuilder("f:valuedItem", FlexDirection.Row, (_, nav) =>
                        {
                            var itemInfrequentProps =
                                InfrequentProperties.Evaluate<ContractTermInfrequentProperties>(nav);

                            return
                            [
                                new Card(new LocalizedLabel("contract.term.asset.valuedItem"),
                                    new Concat([
                                        itemInfrequentProps.Optional(ContractTermInfrequentProperties.Entity,
                                            new NameValuePair(
                                                new LocalizedLabel("contract.term.asset.valuedItem.entity"),
                                                new OpenTypeElement(null, "entity") // CodeableConcept | Reference(Any)
                                            )
                                        ),
                                        //ignore identifier
                                        itemInfrequentProps.Optional(ContractTermInfrequentProperties.EffectiveTime,
                                            new NameValuePair(
                                                new LocalizedLabel("contract.term.asset.valuedItem.effectiveTime"),
                                                new ShowDateTime()
                                            )),
                                        itemInfrequentProps.Optional(ContractTermInfrequentProperties.Quantity,
                                            new NameValuePair(
                                                new LocalizedLabel("contract.term.asset.valuedItem.quantity"),
                                                new ShowQuantity()
                                            )),
                                        itemInfrequentProps.Optional(ContractTermInfrequentProperties.UnitPrice,
                                            new NameValuePair(
                                                new LocalizedLabel("contract.term.asset.valuedItem.unitPrice"),
                                                new ShowMoney()
                                            )),
                                        itemInfrequentProps.Optional(ContractTermInfrequentProperties.Factor,
                                            new NameValuePair(
                                                new LocalizedLabel("contract.term.asset.valuedItem.factor"),
                                                new ShowDecimal()
                                            )),
                                        itemInfrequentProps.Optional(ContractTermInfrequentProperties.Optional,
                                            new NameValuePair(
                                                new LocalizedLabel("contract.term.asset.valuedItem.net"),
                                                new ShowMoney()
                                            )),
                                        itemInfrequentProps.Optional(ContractTermInfrequentProperties.Payment,
                                            new NameValuePair(
                                                new LocalizedLabel("contract.term.asset.valuedItem.payment"),
                                                new Text("@value")
                                            )),
                                        itemInfrequentProps.Optional(ContractTermInfrequentProperties.PaymentDate,
                                            new NameValuePair(
                                                new LocalizedLabel("contract.term.asset.valuedItem.paymentDate"),
                                                new ShowDateTime()
                                            )),
                                        itemInfrequentProps.Optional(ContractTermInfrequentProperties.Responsible,
                                            new NameValuePair(
                                                new LocalizedLabel("contract.term.asset.valuedItem.responsible"),
                                                new AnyReferenceNamingWidget()
                                            )),
                                        itemInfrequentProps.Optional(ContractTermInfrequentProperties.Recipient,
                                            new NameValuePair(
                                                new LocalizedLabel("contract.term.asset.valuedItem.recipient"),
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

        var actionBadge = new PlainBadge(new LocalizedLabel("contract.term.action"));
        var actionInfo = new ListBuilder("f:action", FlexDirection.Column, (_, nav) =>
        {
            var infrequentProperties = InfrequentProperties.Evaluate<ContractTermInfrequentProperties>(nav);

            Widget[] tree =
            [
                infrequentProperties.Optional(ContractTermInfrequentProperties.DoNotPerform,
                    new NameValuePair(
                        new LocalizedLabel("contract.term.action.doNotPerform"),
                        new ShowDoNotPerform()
                    )),
                new NameValuePair(
                    new LocalizedLabel("contract.term.action.type"),
                    new CodeableConcept()
                ),
                new Condition("f:subject",
                    new ConditionalWrapper(
                        x =>
                        {
                            return x.SelectAllNodes("f:subject")
                                .All(subjectNode => subjectNode.IsSubjectFromComposition());
                        },
                        new NameValuePair(
                            new LocalizedLabel("contract.subject"),
                            new CommaSeparatedBuilder("f:subject", _ =>
                            [
                                new ConditionalWrapper(x => x.IsSubjectFromComposition(),
                                    new AnyReferenceNamingWidget(),
                                    new Optional("f:role", new NameValuePair(
                                        new LocalizedLabel("contract.term.action.subject.role"),
                                        new CodeableConcept()
                                    ))
                                ),
                            ])
                        )
                    )
                ),
                new NameValuePair(
                    new LocalizedLabel("contract.term.action.intent"),
                    new ChangeContext("f:intent", new CodeableConcept())
                ),
                // ignore linkId
                new NameValuePair(
                    new LocalizedLabel("contract.term.action.status"),
                    new ChangeContext("f:status", new CodeableConcept())
                ),
                infrequentProperties.Optional(ContractTermInfrequentProperties.Context,
                    new NameValuePair(
                        new LocalizedLabel("contract.term.action.context"),
                        new AnyReferenceNamingWidget()
                    )),
                // ignore contextLinkId
                new If(_ => infrequentProperties.Contains(ContractTermInfrequentProperties.Occurrence),
                    new NameValuePair(
                        new LocalizedLabel("contract.term.action.occurence"),
                        new Chronometry("occurrence")
                    )
                ),
                new InfrequentProperties.Builder<ContractTermInfrequentProperties>(
                    infrequentProperties,
                    ContractTermInfrequentProperties.Requester,
                    items =>
                    [
                        new NameValuePair(
                            new LocalizedLabel("contract.term.action.requester"),
                            new ConcatBuilder(
                                items,
                                (_, _, _) => [new AnyReferenceNamingWidget()],
                                new ConstantText(", ")
                            )
                        ),
                    ]
                ),
                // ignore requesterLinkId
                infrequentProperties.Condition(ContractTermInfrequentProperties.PerformerType,
                    new NameValuePair(
                        new LocalizedLabel("contract.term.action.performerType"),
                        new CommaSeparatedBuilder(
                            ContractTermInfrequentProperties.PerformerType.ToString().ToLowerInvariant(),
                            _ => new CodeableConcept())
                    )),
                infrequentProperties.Optional(ContractTermInfrequentProperties.PerformerRole,
                    new NameValuePair(
                        new LocalizedLabel("contract.term.action.performerRole"),
                        new CodeableConcept()
                    )),
                new Optional("f:performer", new NameValuePair(
                    new LocalizedLabel("contract.term.action.performer"),
                    new AnyReferenceNamingWidget()
                )),
                // ignore performerLinkId
                new Condition("f:reasonCode|f:reasonReference", new NameValuePair(
                    new LocalizedLabel("contract.term.action.reasonX"),
                    new Concat([
                        new CommaSeparatedBuilder("f:reasonCode", _ => new CodeableConcept()),
                        new CommaSeparatedBuilder("f:reasonReference", _ => new AnyReferenceNamingWidget())
                    ], ", ")
                )),
                new Condition("f:reason", new NameValuePair(
                    new LocalizedLabel("contract.term.action.reason"),
                    new CommaSeparatedBuilder("f:reason", _ => new Text("@value"))
                )),
                // ignore reasonLinkId
                new Condition("f:note", new NameValuePair(
                    new LocalizedLabel("contract.term.action.note"),
                    new ListBuilder("f:note", FlexDirection.Column, _ => [new ShowAnnotationCompact()])
                ))
                // ignore securityLabelNumber
            ];

            return tree;
        }, flexContainerClasses: string.Empty); // Overrides the default class

        var complete =
            new Container([
                new Collapser([headerInfo], [
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
                        new PlainBadge(new LocalizedLabel("contract.term.asset-plural")),
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
                        new PlainBadge(new LocalizedLabel("contract.term.group")),
                        new ListBuilder("f:group", FlexDirection.Column, _ => [new ContractTerm()],
                            flexContainerClasses: string.Empty) // Overrides the default class
                    )
                ])
            ]);


        return complete.Render(navigator, renderer, context);
    }

    private enum ContractTermInfrequentProperties
    {
        [OpenType("topic")] Topic,
        [OpenType("entity")] Entity,
        [OpenType("occurrence")] Occurrence,
        Issued,
        Applies,
        Type,
        SubType,
        Text,
        Decision,
        Scope,
        Relationship,
        Reference,
        Condition,
        EffectiveTime,
        Quantity,
        UnitPrice,
        Factor,
        Optional,
        Payment,
        PaymentDate,
        Responsible,
        Recipient,
        DoNotPerform,
        Role,
        Context,
        Requester,
        PerformerType,
        PerformerRole,
        SecurityLabel,
    }
}