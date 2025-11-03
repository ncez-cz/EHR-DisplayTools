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
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var headerInfo = new Container([
            new ConstantText("Smlouva"),
            new Optional("f:title|f:alias|f:subtitle|f:name",
                new ConstantText(" ("),
                new Text("@value"),
                new ConstantText(")")
            ),
            new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/contract-status",
                new DisplayLabel(LabelCodes.Status))
        ]);

        var globalInfrequentProperties =
            Widgets.InfrequentProperties.Evaluate<InfrequentProperties>([navigator]);

        var badge = new PlainBadge(new ConstantText("Základní informace"));

        var basicInfo = new Container([
            new Optional("f:title|f:alias|f:name", new NameValuePair(
                new DisplayLabel(LabelCodes.Name),
                new Text("@value")
            )),
            new Optional("f:subtitle", new NameValuePair(
                new ConstantText("Podnázev"),
                new Text("@value")
            )),
            new Optional("f:legalState", new NameValuePair(
                new ConstantText("Stav vyjednání"),
                new CodeableConcept()
            )),
            //ignore instatiatesCanonical
            //ignore instatiatesUri
            new If(_ => globalInfrequentProperties.Contains(InfrequentProperties.Topic),
                new NameValuePair(
                    new ConstantText("Téma"),
                    new OpenTypeElement(null, "topic") // CodeableConcept | Reference(Any)
                )
            ),
            new Optional("f:scope", new NameValuePair(
                new ConstantText("Rozsah"),
                new CodeableConcept()
            )),
            new Optional("f:type", new NameValuePair(
                new ConstantText("Typ"),
                new CodeableConcept()
            )),
            new Condition("f:subtype", new NameValuePair(
                new ConstantText("Podtyp"),
                new CommaSeparatedBuilder("f:subtype", _ => [new CodeableConcept()])
            )),
            new Optional("f:contentDerivative", new NameValuePair(
                new ConstantText("Odvozený obsah"),
                new CodeableConcept()
            )),
            new Optional("f:issued", new NameValuePair(
                new ConstantText("Datum vydání"),
                new ShowDateTime()
            )),
            new Optional("f:applies", new NameValuePair(
                new ConstantText("Datum platnosti"),
                new ShowPeriod()
            )),
            new Optional("f:expirationType", new NameValuePair(
                new ConstantText("Důvod expirace"),
                new CodeableConcept()
            )),
            new Condition("f:supportingInfo", new NameValuePair(
                new ConstantText("Podpůrné informace"),
                new CommaSeparatedBuilder("f:supportingInfo", _ => [new AnyReferenceNamingWidget()])
            )),
            new Condition("f:relevantHistory", new NameValuePair(
                new ConstantText("Relevantní historie"),
                new CommaSeparatedBuilder("f:relevantHistory", _ => [new AnyReferenceNamingWidget()])
            )),
        ]);

        var actorsBadge = new PlainBadge(new ConstantText("Činitelé"));
        var actorsInfo = new Container([
            new Condition("f:subject", new NameValuePair(
                new ConstantText("Předmět"),
                new CommaSeparatedBuilder("f:subject", _ => new AnyReferenceNamingWidget())
            )),
            new Optional("f:author", new NameValuePair(
                new ConstantText("Autor"),
                new AnyReferenceNamingWidget()
            )),
            new Condition("f:authority", new NameValuePair(
                new ConstantText("Autorita"),
                new CommaSeparatedBuilder("f:authority", _ => [new AnyReferenceNamingWidget()])
            )),
            new Condition("f:domain", new NameValuePair(
                new ConstantText("Doména"),
                new CommaSeparatedBuilder("f:domain", _ => [new AnyReferenceNamingWidget()])
            )),
            new Condition("f:site", new NameValuePair(
                new ConstantText("Místo"),
                new CommaSeparatedBuilder("f:site", _ => [new AnyReferenceNamingWidget()])
            )),
            new Condition("f:signer",
                new TextContainer(TextStyle.Bold, [new ConstantText("Signatáři:")]),
                new ListBuilder("f:signer", FlexDirection.Row, _ =>
                [
                    new Card(new ConstantText("Signatář"),
                        new Container([
                            new NameValuePair(
                                new ConstantText("Role"),
                                new ChangeContext("f:type", new Coding())
                            ),
                            new NameValuePair(
                                new ConstantText("Smluvní strana"),
                                new AnyReferenceNamingWidget("f:party")
                            ),

                            new TextContainer(TextStyle.Bold, new ConstantText("Podpisy:")),
                            new ListBuilder("f:signature", FlexDirection.Row,
                                _ => [new Card(null, new ShowSignature("."))])
                        ])
                    )
                ])
            )
        ]);

        var precursorBadge = new PlainBadge(new ConstantText("Předzvěst smlouvy"));
        var precursorInfo = new Container([
            new NameValuePair(
                new ConstantText("Typ"),
                new ChangeContext("f:type", new CodeableConcept())
            ),
            new Optional("f:subType", new NameValuePair(
                new ConstantText("Podtyp"),
                new CodeableConcept()
            )),
            new Optional("f:publisher", new NameValuePair(
                new ConstantText("Vydavatel"),
                new AnyReferenceNamingWidget()
            )),
            new Optional("f:publicationDate", new NameValuePair(
                new ConstantText("Datum vydání"),
                new ShowDateTime()
            )),
            new Optional("f:publicationStatus", new NameValuePair(
                new ConstantText("Stav vydání"),
                new EnumLabel(".", "http://hl7.org/fhir/ValueSet/contract-publicationstatus")
            )),
            new Optional("f:copyright", new NameValuePair(
                new ConstantText("Autorská práva"),
                new Markdown("@value")
            )),
        ]);

        var contractsBadge = new PlainBadge(new ConstantText("Právní obsah"));
        var contractsInfo = new Row([
            new Condition("f:friendly",
                new Container([
                    new TextContainer(TextStyle.Bold, [new ConstantText("Verze srozumitelné pro pacienta")]),
                    new ItemListBuilder("f:friendly", ItemListType.Unordered, _ =>
                        [
                            new OpenTypeElement(null,
                                "content") // Attachment | Reference(Composition | DocumentReference | QuestionnaireResponse)
                        ]
                    )
                ])
            ),
            new Condition("f:legal",
                new Container([
                    new TextContainer(TextStyle.Bold, [new ConstantText("Legální verze")]),
                    new ItemListBuilder("f:legal", ItemListType.Unordered, _ =>
                        [
                            new OpenTypeElement(null,
                                "content") // Attachment | Reference(Composition | DocumentReference | QuestionnaireResponse)
                        ]
                    )
                ])
            ),
            new Condition("f:rule",
                new Container([
                    new TextContainer(TextStyle.Bold, [new ConstantText("Strojové verze")]),
                    new ItemListBuilder("f:rule", ItemListType.Unordered, _ =>
                        [
                            new OpenTypeElement(null,
                                "content") // Attachment | Reference(Composition | DocumentReference | QuestionnaireResponse | Contract)
                        ]
                    )
                ])
            ),
            new If(_ => globalInfrequentProperties.Contains(InfrequentProperties.LegallyBinding),
                new Container([
                    new TextContainer(TextStyle.Bold, [new ConstantText("Právně závazná verze")]),
                    new ItemList(ItemListType.Unordered,
                        [
                            new OpenTypeElement(null, "legallyBinding")
                        ] // Attachment | Reference(Composition | DocumentReference | QuestionnaireResponse | Contract)	
                    )
                ])
            )
        ], flexContainerClasses: "gap-2");

        var complete =
            new Collapser([headerInfo], [], [
                    new If(
                        _ => navigator.EvaluateCondition(
                                 "f:title or f:alias or f:subtitle or f:name or f:status or f:legalState or " +
                                 "f:scope or f:type or f:subtype or f:contentDerivative or f:issued or f:applies or f:expirationType") ||
                             globalInfrequentProperties.Contains(InfrequentProperties.Topic),
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
                                    globalInfrequentProperties.Contains(InfrequentProperties.LegallyBinding),
                            new ThematicBreak()
                        )
                    ),
                    new If(_ => navigator.EvaluateCondition("f:friendly or f:legal or f:rule") ||
                                globalInfrequentProperties.Contains(InfrequentProperties.LegallyBinding),
                        contractsBadge,
                        contractsInfo,
                        new Condition("f:term",
                            new ThematicBreak()
                        )
                    ),
                    new Condition("f:term",
                        new PlainBadge(new ConstantText("Smluvní ustanovení")),
                        new ListBuilder("f:term", FlexDirection.Row, _ => [new ContractTerm()])
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

    private enum InfrequentProperties
    {
        [OpenType("topic")] Topic,
        [OpenType("legallyBinding")] LegallyBinding,
    }
}