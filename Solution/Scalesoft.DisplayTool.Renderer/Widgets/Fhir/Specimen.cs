using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Specimen : ColumnResourceBase<Specimen>, IResourceWidget
{
    public static string ResourceType => "Specimen";
    
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer, RenderContext context)
    {
        var headerInfo = new Container([
            new Container([
                new ConstantText("Vzorek"),
                new HideableDetails(new Optional("f:accessionIdentifier",
                    new ConstantText(" ("),
                    new ShowIdentifier(),
                    new ConstantText(")")
                )),
            ], ContainerType.Span),
            new HideableDetails(new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/specimen-status",
                new DisplayLabel(LabelCodes.Status)))
        ], ContainerType.Div, "d-flex align-items-center gap-1");

        var badge = new PlainBadge(new ConstantText("Základní informace"));

        var basicInfo = new Container([
            new Optional("f:accessionIdentifier", new HideableDetails(new NameValuePair(
                new ConstantText("Identifikátor přístupu"),
                new ShowIdentifier()
            ))),
            new Optional("f:type", new NameValuePair(
                new ConstantText("Typ"),
                new CodeableConcept()
            )),
            new Condition("f:condition", new HideableDetails(new NameValuePair(
                new ConstantText("Kondice"),
                new CommaSeparatedBuilder("f:condition", _ => [new CodeableConcept()])
            ))),
            new Optional("f:subject", new NameValuePair(
                new ConstantText("Původ vzorku"),
                new AnyReferenceNamingWidget()
            )),
            new Optional("f:receivedTime", new NameValuePair(
                new ConstantText("Čas přijetí"),
                new ShowDateTime()
            )),
            new Condition("f:parent", new HideableDetails(new NameValuePair(
                new ConstantText("Rodiče"),
                new CommaSeparatedBuilder("f:parent", _ => [new AnyReferenceNamingWidget()])
            ))),
            new Condition("f:request", new HideableDetails(new NameValuePair(
                new ConstantText("Žádosti"),
                new CommaSeparatedBuilder("f:request", _ => [new AnyReferenceNamingWidget()])
            )))
        ]);

        InfrequentPropertiesData<InfrequentProperties>? collectionInfrequentProperties = null;

        if (navigator.EvaluateCondition("f:collection"))
        {
            collectionInfrequentProperties =
                Widgets.InfrequentProperties.Evaluate<InfrequentProperties>([
                    navigator.SelectSingleNode("f:collection")
                ]);
        }


        var collectionBadge = new PlainBadge(new ConstantText("Informace o odběru"));
        var collectionInfo = new Container([
            new Optional("f:collector", new HideableDetails(new NameValuePair(
                new ConstantText("Sbíral"),
                new AnyReferenceNamingWidget()
            ))),
            new If(
                _ => collectionInfrequentProperties != null &&
                     collectionInfrequentProperties.Contains(InfrequentProperties.Collected),
                new NameValuePair(
                    new ConstantText("Čas odběru"),
                    new Chronometry("collected")
                )
            ),
            new Optional("f:duration", new HideableDetails(new NameValuePair(
                new ConstantText("Doba trvání odběru"),
                new ShowDuration()
            ))),
            new Optional("f:quantity", new HideableDetails(new NameValuePair(
                new ConstantText("Množství"),
                new ShowQuantity()
            ))),
            new Optional("f:method", new NameValuePair(
                new ConstantText("Metoda odběru"),
                new CodeableConcept()
            )),
            new Optional("f:bodySite", new NameValuePair(
                new ConstantText("Strana těla odběru"),
                new CodeableConcept()
            )),
            new If(_ => collectionInfrequentProperties != null &&
                        collectionInfrequentProperties.Contains(InfrequentProperties.BodySiteExtension),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Cílová anatomická poloha nebo struktura"),
                    new AnyReferenceNamingWidget(
                        "f:extension[@url='http://hl7.org/fhir/StructureDefinition/bodySite']/f:valueReference")
                ))
            ),
            new If(
                _ => collectionInfrequentProperties != null &&
                     collectionInfrequentProperties.Contains(InfrequentProperties.FastingStatus),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Stav půstu"),
                    new OpenTypeElement(null, "fastingStatus") // CodeableConcept | Duration
                ))
            )
        ]);

        var processingBadge = new PlainBadge(new ConstantText("Detaily zpracování"));
        var processingInfo = new ListBuilder("f:processing", FlexDirection.Row, (_, nav) =>
        {
            var infrequentProperties =
                Widgets.InfrequentProperties.Evaluate<InfrequentProperties>([nav]);

            var card = new Card(
                new ConstantText("Zpracování vzorku"),
                new Concat([
                    new Optional("f:description", new NameValuePair(
                        new DisplayLabel(LabelCodes.Description),
                        new Text("@value")
                    )),
                    new Optional("f:procedure", new NameValuePair(
                        new ConstantText("Postup"),
                        new CodeableConcept()
                    )),
                    new Condition("f:additive", new NameValuePair(
                        new ConstantText("Přídavky"),
                        new CommaSeparatedBuilder("f:additive", _ => [new AnyReferenceNamingWidget()])
                    )),
                    new If(_ => infrequentProperties.Contains(InfrequentProperties.Time),
                        new NameValuePair(
                            new ConstantText("Čas zpracování"),
                            new Chronometry("time")
                        )
                    ),
                ])
            );

            return [card];
        }, flexContainerClasses: "gap-2");

        var containerBadge = new PlainBadge(new ConstantText("Informace o nádobách"));
        var containerInfo = new ListBuilder("f:container", FlexDirection.Row, (_, nav) =>
        {
            var infrequentProperties =
                Widgets.InfrequentProperties.Evaluate<InfrequentProperties>([nav]);

            var card = new Card(
                new ConstantText("Nádoba"),
                new Concat([
                    new Optional("f:description", new NameValuePair(
                        new DisplayLabel(LabelCodes.Description),
                        new Text("@value")
                    )),
                    new Optional("f:type", new NameValuePair(
                        new ConstantText("Typ"),
                        new CodeableConcept()
                    )),
                    new Optional("f:capacity", new NameValuePair(
                        new ConstantText("Kapacita"),
                        new ShowQuantity()
                    )),
                    new Optional("f:specimenQuantity", new NameValuePair(
                        new ConstantText("Množství vzorku"),
                        new ShowQuantity()
                    )),
                    new If(_ => infrequentProperties.Contains(InfrequentProperties.Device),
                        new NameValuePair(
                            new ConstantText("Zařízení"),
                            new AnyReferenceNamingWidget(
                                "f:extension[@url='http://hl7.org/fhir/5.0/StructureDefinition/extension-Specimen.container.device']/f:valueReference")
                        )
                    ),
                    new If(_ => infrequentProperties.Contains(InfrequentProperties.Additive),
                        new NameValuePair(
                            new ConstantText("Přídavky"),
                            new OpenTypeElement(null, "additive") // CodeableConcept | Reference(Substance)
                        )
                    ),
                ])
            );

            return [card];
        }, flexContainerClasses: "gap-2");


        var complete =
            new Collapser([headerInfo], [], [
                    new Row([
                        new Condition(
                            "f:accessionIdentifier or f:status or f:type or f:subject or f:receivedTime or f:parent or f:request",
                            new Container([
                                badge,
                                basicInfo
                            ])
                        ),
                        new Optional("f:collection",
                            new Container([
                                collectionBadge,
                                collectionInfo
                            ])
                        ),
                        new Condition("f:processing",
                            new HideableDetails(new Container([
                                processingBadge,
                                processingInfo
                            ]))
                        ),
                        new Condition("f:container",
                            new HideableDetails(new Container([
                                containerBadge,
                                containerInfo
                            ]))
                        ),
                        new Condition("f:note",
                            new Container([
                                new PlainBadge(new ConstantText("Anotace")),
                                new ListBuilder("f:note", FlexDirection.Column, _ => [new ShowAnnotationCompact()])
                            ])
                        )
                    ]),
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
        [OpenType("collected")] Collected,
        [OpenType("fastingStatus")] FastingStatus,
        [OpenType("time")] Time,
        [OpenType("additive")] Additive,

        [Extension("http://hl7.org/fhir/StructureDefinition/bodySite")]
        BodySiteExtension,

        [Extension("http://hl7.org/fhir/5.0/StructureDefinition/extension-Specimen.container.device")]
        Device,
    }
}