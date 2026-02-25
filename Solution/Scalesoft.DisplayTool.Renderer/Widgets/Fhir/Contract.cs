using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Contract : ColumnResourceBase<Contract>, IResourceWidget
{
    public static string ResourceType => "Contract";
    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var headerInfo = new Container([
            new LocalizedLabel("contract"),
            new Optional("f:title|f:alias|f:subtitle|f:name",
                new ConstantText(" ("),
                new Text("@value"),
                new ConstantText(")")
            ),
            new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/contract-status",
                new EhdsiDisplayLabel(LabelCodes.Status))
        ]);

        var infrequentProperties = InfrequentProperties.Evaluate<ContractInfrequentProperties>(navigator);

        var badge = new PlainBadge(new LocalizedLabel("general.basic-information"));

        var basicInfo = new Container([
            new Optional("f:title|f:alias|f:name", new NameValuePair(
                new EhdsiDisplayLabel(LabelCodes.Name),
                new Text("@value")
            )),
            infrequentProperties.Optional(ContractInfrequentProperties.Subtitle,
                new NameValuePair(
                    new LocalizedLabel("contract.subtitle"),
                    new Text("@value")
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.LegalState,
                new NameValuePair(
                    new LocalizedLabel("contract.legalState"),
                    new CodeableConcept()
                )
            ),
            //ignore instatiatesCanonical
            //ignore instatiatesUri
            infrequentProperties.Condition(ContractInfrequentProperties.Topic,
                new NameValuePair(
                    new LocalizedLabel("contract.topic"),
                    new OpenTypeElement(null, "topic") // CodeableConcept | Reference(Any)
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.Scope,
                new NameValuePair(
                    new LocalizedLabel("contract.scope"),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.Type,
                new NameValuePair(
                    new LocalizedLabel("contract.type"),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Condition(ContractInfrequentProperties.Subtype,
                new NameValuePair(
                    new LocalizedLabel("contract.subtype"),
                    new CommaSeparatedBuilder("f:subtype", _ => [new CodeableConcept()])
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.ContentDerivative,
                new NameValuePair(
                    new LocalizedLabel("contract.contentDerivative"),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.Issued,
                new NameValuePair(
                    new LocalizedLabel("contract.issued"),
                    new ShowDateTime()
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.Applies,
                new NameValuePair(
                    new LocalizedLabel("contract.applies"),
                    new ShowPeriod()
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.ExpirationType,
                new NameValuePair(
                    new LocalizedLabel("contract.expirationType"),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Condition(ContractInfrequentProperties.SupportingInfo,
                new NameValuePair(
                    new LocalizedLabel("contract.supportingInfo"),
                    new CommaSeparatedBuilder("f:supportingInfo", _ => [new AnyReferenceNamingWidget()])
                )
            ),
            infrequentProperties.Condition(ContractInfrequentProperties.RelevantHistory,
                new NameValuePair(
                    new LocalizedLabel("contract.relevantHistory"),
                    new CommaSeparatedBuilder("f:relevantHistory", _ => [new AnyReferenceNamingWidget()])
                )
            ),
        ], optionalClass: "name-value-pair-wrapper w-fit-content");

        var actorsBadge = new PlainBadge(new LocalizedLabel("general.actors"));
        var actorsInfo = new Container([
            new Condition("f:subject",
                new ConditionalWrapper(
                    x =>
                    {
                        return x.SelectAllNodes("f:subject").All(subjectNode => subjectNode.IsSubjectFromComposition());
                    },
                    new NameValuePair(
                        new LocalizedLabel("contract.subject"),
                        new CommaSeparatedBuilder("f:subject", _ => new AnyReferenceNamingWidget())
                    )
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.Author,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("contract.author"),
                    }
                )
            ),
            infrequentProperties.Condition(ContractInfrequentProperties.Authority,
                new NameValuePair(
                    new LocalizedLabel("contract.authority"),
                    new CommaSeparatedBuilder("f:authority", _ => [new AnyReferenceNamingWidget()])
                )
            ),
            infrequentProperties.Condition(ContractInfrequentProperties.Domain,
                new NameValuePair(
                    new LocalizedLabel("contract.domain"),
                    new CommaSeparatedBuilder("f:domain", _ => [new AnyReferenceNamingWidget()])
                )
            ),
            infrequentProperties.Condition(ContractInfrequentProperties.Site,
                new NameValuePair(
                    new LocalizedLabel("contract.site"),
                    new CommaSeparatedBuilder("f:site", _ => [new AnyReferenceNamingWidget()])
                )
            ),
            infrequentProperties.Condition(ContractInfrequentProperties.Signer,
                new TextContainer(
                    TextStyle.Bold,
                    [new LocalizedLabel("contract.signer-plural"), new ConstantText(":")]
                ),
                new ListBuilder("f:signer", FlexDirection.Row, _ =>
                [
                    new Card(new LocalizedLabel("contract.signer"),
                        new Container([
                            new NameValuePair(
                                new LocalizedLabel("contract.signer.role"),
                                new ChangeContext("f:type", new Coding())
                            ),
                            new AnyReferenceNamingWidget("f:party",
                                widgetModel: new ReferenceNamingWidgetModel
                                {
                                    Type = ReferenceNamingWidgetType.NameValuePair,
                                    LabelOverride = new LocalizedLabel("contract.signer.party"),
                                }
                            ),

                            new TextContainer(
                                TextStyle.Bold,
                                [new LocalizedLabel("contract.signer.signature-plural"), new ConstantText(":")]
                            ),
                            new ListBuilder("f:signature", FlexDirection.Row,
                                _ => [new Card(null, new ShowSignature("."))])
                        ])
                    )
                ])
            )
        ], optionalClass: "name-value-pair-wrapper w-fit-content");

        var precursorBadge = new PlainBadge(new LocalizedLabel("contract.contentDefinition"));
        var precursorInfo = new Container([
            new NameValuePair(
                new LocalizedLabel("contract.contentDefinition.type"),
                new ChangeContext("f:type", new CodeableConcept())
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.Subtype,
                new NameValuePair(
                    new LocalizedLabel("contract.contentDefinition.subType"),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.Publisher,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("contract.contentDefinition.publisher"),
                    }
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.PublicationDate,
                new NameValuePair(
                    new LocalizedLabel("contract.contentDefinition.publicationDate"),
                    new ShowDateTime()
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.PublicationStatus,
                new NameValuePair(
                    new LocalizedLabel("contract.contentDefinition.publicationStatus"),
                    new EnumLabel(".", "http://hl7.org/fhir/ValueSet/contract-publicationstatus")
                )
            ),
            infrequentProperties.Optional(ContractInfrequentProperties.Copyright,
                new NameValuePair(
                    new LocalizedLabel("contract.contentDefinition.copyright"),
                    new Markdown("@value")
                )
            ),
        ], optionalClass: "name-value-pair-wrapper w-fit-content");

        var contractsBadge = new PlainBadge(new LocalizedLabel("contract.legal-content-label"));
        var contractsInfo = new Row([
            new Condition("f:friendly",
                new Container([
                    new TextContainer(TextStyle.Bold, [new LocalizedLabel("contract.friendly-plural")]),
                    new ItemListBuilder("f:friendly", ItemListType.Unordered, _ =>
                        [
                            new OpenTypeElement(null,
                                "content"), // Attachment | Reference(Composition | DocumentReference | QuestionnaireResponse)
                        ]
                    )
                ])
            ),
            new Condition("f:legal",
                new Container([
                    new TextContainer(TextStyle.Bold, [new LocalizedLabel("contract.legal")]),
                    new ItemListBuilder("f:legal", ItemListType.Unordered, _ =>
                        [
                            new OpenTypeElement(null,
                                "content"), // Attachment | Reference(Composition | DocumentReference | QuestionnaireResponse)
                        ]
                    )
                ])
            ),
            new Condition("f:rule",
                new Container([
                    new TextContainer(TextStyle.Bold, [new LocalizedLabel("contract.rule")]),
                    new ItemListBuilder("f:rule", ItemListType.Unordered, _ =>
                        [
                            new OpenTypeElement(null,
                                "content") // Attachment | Reference(Composition | DocumentReference | QuestionnaireResponse | Contract)
                        ]
                    )
                ])
            ),
            infrequentProperties.Condition(ContractInfrequentProperties.LegallyBinding,
                new Container([
                    new TextContainer(TextStyle.Bold, [new LocalizedLabel("contract.legallyBinding")]),
                    new ItemList(ItemListType.Unordered,
                        [
                            new OpenTypeElement(null, "legallyBinding")
                        ] // Attachment | Reference(Composition | DocumentReference | QuestionnaireResponse | Contract)	
                    ),
                ])
            ),
        ], flexContainerClasses: "gap-2");

        var complete =
            new Collapser([headerInfo], [
                    new If(
                        _ => navigator.EvaluateCondition(
                                 "f:title or f:alias or f:subtitle or f:name or f:status or f:legalState or " +
                                 "f:scope or f:type or f:subtype or f:contentDerivative or f:issued or f:applies or f:expirationType") ||
                             infrequentProperties.Contains(ContractInfrequentProperties.Topic),
                        badge,
                        basicInfo,
                        new Condition(
                            "f:subject or f:author or f:authority or f:domain or f:site or f:signer or f:contentDefinition or f:term",
                            new ThematicBreak()
                        )
                    ),
                    new Condition("f:subject or f:author or f:authority or f:domain or f:site or f:signer",
                        actorsBadge,
                        actorsInfo,
                        new Condition("f:contentDefinition or f:term",
                            new ThematicBreak()
                        )
                    ),
                    new Optional("f:contentDefinition",
                        precursorBadge,
                        precursorInfo,
                        new If(_ => navigator.EvaluateCondition("f:term or f:friendly or f:legal or f:rule") ||
                                    infrequentProperties.Contains(ContractInfrequentProperties.LegallyBinding),
                            new ThematicBreak()
                        )
                    ),
                    new If(_ => navigator.EvaluateCondition("f:friendly or f:legal or f:rule") ||
                                infrequentProperties.Contains(ContractInfrequentProperties.LegallyBinding),
                        contractsBadge,
                        contractsInfo,
                        new Condition("f:term",
                            new ThematicBreak()
                        )
                    ),
                    new Condition("f:term",
                        new PlainBadge(new LocalizedLabel("contract.term")),
                        new ListBuilder("f:term", FlexDirection.Column, _ => [new ContractTerm()],
                            flexContainerClasses: string.Empty) // Overrides the default class
                    ),
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

    private enum ContractInfrequentProperties
    {
        [OpenType("topic")] Topic,
        [OpenType("legallyBinding")] LegallyBinding,
        Subtitle,
        LegalState,
        Scope,
        Type,
        ContentDerivative,
        Issued,
        Applies,
        ExpirationType,
        Author,
        Subtype,
        Publisher,
        PublicationDate,
        PublicationStatus,
        Copyright,
        SupportingInfo,
        RelevantHistory,
        Authority,
        Domain,
        Site,
        Signer,
    }
}