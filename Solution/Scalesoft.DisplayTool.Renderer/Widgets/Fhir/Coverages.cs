using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Coverages : AlternatingBackgroundColumnResourceBase<Coverages>, IResourceWidget
{
    public static string ResourceType => "Coverage";

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<CoverageInfrequentProperties>([navigator]);

        Widget[] output =
        [
            new Column(
            [
                new Row([
                    new Heading(
                    [
                        new TextContainer(TextStyle.Bold,
                            [new ChangeContext("f:payor", new AnyReferenceNamingWidget())])
                    ], HeadingSize.H5, customClass: "m-0 blue-color"),
                    new EnumIconTooltip("f:status",
                        "http://hl7.org/fhir/ValueSet/fm-status",
                        new DisplayLabel(LabelCodes.Status)),
                    new NarrativeModal(alignRight: false),
                ]),
                new Row([
                    new HideableDetails(
                        new NameValuePair(
                            new ConstantText("Příjemce"),
                            new AnyReferenceNamingWidget("f:beneficiary"),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Primary
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.Identifier,
                        new NameValuePair(
                            new ConstantText("Číslo pojistné smlouvy"),
                            new ShowIdentifier(),
                            direction: FlexDirection.Column,
                            style: NameValuePair.NameValuePairStyle.Primary
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.Type,
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Typ"),
                                new CodeableConcept(),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.PolicyHolder,
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Pojistník"),
                                new AnyReferenceNamingWidget(),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.Subscriber,
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Odběratel"),
                                new AnyReferenceNamingWidget(),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.SubscriberId,
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Číslo pojištěnce odběratele"),
                                new Text("@value"),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.Dependent,
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Identifikátor závislé osoby"),
                                new Text("@value"),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties,
                        CoverageInfrequentProperties.Relationship,
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Vztah mezi příjemcem a odběratelem"),
                                new CodeableConcept(),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.Period,
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Období"),
                                new ShowPeriod(),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.Order,
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Pořadí"),
                                new Text("@value"),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.Network,
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Síť pojišťoven"),
                                new Text("@value"),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.Subrogation,
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Náhrada pojišťovně"),
                                new ShowBoolean(
                                    new DisplayLabel(LabelCodes.No),
                                    new DisplayLabel(LabelCodes.Yes)
                                ),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.Contract,
                        new HideableDetails(
                            new NameValuePair(
                                new ConstantText("Smlouva"),
                                new AnyReferenceNamingWidget(),
                                direction: FlexDirection.Column,
                                style: NameValuePair.NameValuePairStyle.Primary
                            )
                        )
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.Class,
                        items =>
                        [
                            new HideableDetails(
                                new NameValuePair(
                                    new TextContainer(TextStyle.Bold, new ConstantText("Dodatečná klasifikace")),
                                    new Container([
                                        new ListBuilder(items, FlexDirection.Row, (_, _) =>
                                        [
                                            new NameValuePair(
                                                [new ChangeContext("f:type", new CodeableConcept())],
                                                [new Text("f:value/@value")],
                                                direction: FlexDirection.Row
                                            ),
                                        ]),
                                    ]),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary
                                )
                            ),
                        ]
                    ),
                    InfrequentProperties.Optional(infrequentProperties, CoverageInfrequentProperties.CostToBeneficiary,
                        items =>
                        [
                            new HideableDetails(
                                new NameValuePair(
                                    new TextContainer(TextStyle.Bold, new ConstantText("Náklady samoplátce")),
                                    new ListBuilder(items, FlexDirection.Column, (_, _) =>
                                    [
                                        new Row([
                                            new Optional("f:type",
                                                new NameValuePair([new ConstantText("Typ")],
                                                    [new CodeableConcept()])),
                                            new NameValuePair([new ConstantText("Hodnota")],
                                                [new OpenTypeElement(null)]), // 	SimpleQuantity | Money
                                            new ListBuilder("f:exception", FlexDirection.Column, _ =>
                                            [
                                                new TextContainer(TextStyle.Regular, [
                                                    new ConstantText("Výjimka"),
                                                    new Optional("f:period", new ConstantText(" po dobu "),
                                                        new ShowPeriod()),
                                                    new ConstantText(" typu "),
                                                    new ChangeContext("f:type", new CodeableConcept()),
                                                ]),
                                            ], flexContainerClasses: "gap-0"),
                                        ], flexContainerClasses: "row-gap-0 column-gap-5"),
                                    ]),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary
                                )
                            ),
                        ]
                    ),
                ], flexContainerClasses: "column-gap-6 row-gap-1"),
            ], flexContainerClasses: "gap-1"),
        ];

        return output.RenderConcatenatedResult(navigator, renderer, context);
    }

    private enum CoverageInfrequentProperties
    {
        Identifier,
        Type,
        PolicyHolder,
        Subscriber,
        SubscriberId,
        Dependent,
        Relationship,
        Period,
        Class,
        Order,
        Network,
        CostToBeneficiary,
        Subrogation,
        Contract,
    }
}