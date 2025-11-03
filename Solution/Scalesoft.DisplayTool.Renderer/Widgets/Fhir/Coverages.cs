using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Person;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Coverages(
    List<XmlDocumentNavigator> items,
    bool skipIdPopulation = false
)
    : Widget, IResourceWidget
{
    public static string ResourceType => "Coverage";

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return [new Coverages(items)];
    }
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<CoverageInfrequentProperties>(items);

        var tree = new Table(
            [
                new TableHead([
                    new TableRow([
                        new HideableDetails(new If(
                                _ => infrequentProperties.Contains(CoverageInfrequentProperties.Identifier),
                                new TableCell([new ConstantText("Identifikátor")], TableCellType.Header)), new If(
                                _ => infrequentProperties.Contains(CoverageInfrequentProperties.Type),
                                new TableCell([new ConstantText("Typ")], TableCellType.Header)), new If(
                                _ => infrequentProperties.Contains(CoverageInfrequentProperties.PolicyHolder),
                                new TableCell([new ConstantText("Pojistník")], TableCellType.Header)), new If(
                                _ => infrequentProperties.Contains(CoverageInfrequentProperties.Subscriber),
                                new TableCell([new ConstantText("Odběratel")], TableCellType.Header)), new If(
                                _ => infrequentProperties.Contains(CoverageInfrequentProperties.SubscriberId),
                                new TableCell([new ConstantText("ID Odběratele")], TableCellType.Header)),
                            new TableCell([new ConstantText("Příjemce")], TableCellType.Header), new If(
                                _ => infrequentProperties.Contains(CoverageInfrequentProperties.Dependent),
                                new TableCell([new ConstantText("Identifikátor závislé osoby")], TableCellType.Header)),
                            new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Relationship),
                                new TableCell([new ConstantText("Vztah mezi *beneficiary* a *subscriber*")],
                                    TableCellType.Header)), new If(
                                _ => infrequentProperties.Contains(CoverageInfrequentProperties.Period),
                                new TableCell([new ConstantText("Období")], TableCellType.Header))),
                        new TableCell([new ConstantText("Plátce")], TableCellType.Header),
                        new HideableDetails(new If(
                            _ => infrequentProperties.Contains(CoverageInfrequentProperties.Class),
                            new TableCell([new ConstantText("Dodatečná klasifikace")], TableCellType.Header)), new If(
                            _ => infrequentProperties.Contains(CoverageInfrequentProperties.Order),
                            new TableCell([new ConstantText("Pořadí")], TableCellType.Header)), new If(
                            _ => infrequentProperties.Contains(CoverageInfrequentProperties.Network),
                            new TableCell([new ConstantText("Síť poskytovatelů zdravotního pojištění")],
                                TableCellType.Header)), new If(
                            _ => infrequentProperties.Contains(CoverageInfrequentProperties.CostToBeneficiary),
                            new TableCell([new ConstantText("Náhrada pojišťovně")], TableCellType.Header)), new If(
                            _ => infrequentProperties.Contains(CoverageInfrequentProperties.Subrogation),
                            new TableCell([new ConstantText("Refundace pojistiteli")], TableCellType.Header))),
                        new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Text),
                            new NarrativeCell(false, TableCellType.Header)
                        )
                    ]),
                ]),
                ..items.Select(x => new ChangeContext(x, new CoverageRow(infrequentProperties, skipIdPopulation))),
            ],
            true
        );

        return tree.Render(navigator, renderer, context);
    }

    private class CoverageRow(
        InfrequentPropertiesData<CoverageInfrequentProperties> infrequentProperties,
        bool skipIdPopulation
    ) : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var collapsibleRow = new StructuredDetails();

            if (navigator.EvaluateCondition("f:text"))
            {
                collapsibleRow.AddCollapser(new DisplayLabel(LabelCodes.OriginalNarrative), new Narrative("f:text"));
            }

            if (navigator.EvaluateCondition("f:contract"))
            {
                collapsibleRow.AddCollapser(new ConstantText("Smlouva"),
                    new ShowMultiReference("f:contract", displayResourceType: false));
            }

            var row = new TableRow([
                new HideableDetails(new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Identifier),
                    new TableCell([
                        new ListBuilder("f:identifier", FlexDirection.Column, _ =>
                        [
                            new ShowIdentifier()
                        ], flexContainerClasses: "gap-0"),
                    ])), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Type),
                    new TableCell([
                        new ChangeContext("f:type", new CodeableConcept()),
                    ])), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.PolicyHolder),
                    new TableCell([
                        ShowSingleReference.WithDefaultDisplayHandler(
                            nav => [new Container([new PersonOrOrganization(nav)], idSource: nav)], "f:policyHolder"),
                    ])), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Subscriber),
                    new TableCell([
                        new AnyReferenceNamingWidget("f:subscriber"),
                    ])), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.SubscriberId),
                    new TableCell([
                        new Text("f:subscriberId/@value"),
                    ])), new TableCell([
                    new AnyReferenceNamingWidget("f:beneficiary"),
                ]), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Dependent),
                    new TableCell([
                        new Text("f:dependent/@value"),
                    ])), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Relationship),
                    new TableCell([
                        new ChangeContext("f:relationship", new CodeableConcept()),
                    ])), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Period),
                    new TableCell([
                        new ShowPeriod("f:period"),
                    ]))),
                new TableCell([
                    new AnyReferenceNamingWidget("f:payor"),
                ]),
                new HideableDetails(new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Class),
                    new TableCell([
                        new ItemListBuilder("f:class", ItemListType.Unordered, _ =>
                        [
                            new NameValuePair([new ChangeContext("f:type", new CodeableConcept())],
                                [new Text("f:value/@value")]),
                        ]),
                    ])), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Order),
                    new TableCell([
                        new Text("f:order/@value"),
                    ])), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Network),
                    new TableCell([
                        new Text("f:network/@value"),
                    ])), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.CostToBeneficiary),
                    new TableCell([
                        new ListBuilder("f:costToBeneficiary", FlexDirection.Column, _ =>
                        [
                            new Container([
                                new Optional("f:type",
                                    new NameValuePair([new ConstantText("Typ")], [new CodeableConcept()])),
                                new NameValuePair([new ConstantText("Hodnota")],
                                    [new OpenTypeElement(null)]), // 	SimpleQuantity | Money
                                new ItemListBuilder("f:exception", ItemListType.Unordered, _ =>
                                [
                                    new ConstantText("Výjimka"),
                                    new Optional("f:period", new ConstantText(" po dobu "), new ShowPeriod()),
                                    new ConstantText(" typu "),
                                    new ChangeContext("f:type", new CodeableConcept()),
                                ]),
                            ], ContainerType.Span)
                        ], separator: new LineBreak())
                    ])), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Subrogation),
                    new TableCell([
                        new ShowBoolean(new DisplayLabel(LabelCodes.No), new DisplayLabel(LabelCodes.Yes),
                            "f:subrogation"),
                    ])), new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Status),
                    new TableCell([
                        new EnumIconTooltip("f:status", "http://hl7.org/fhir/fm-status",
                            new DisplayLabel(LabelCodes.Status))
                    ])
                )),
                new If(_ => infrequentProperties.Contains(CoverageInfrequentProperties.Text),
                    new NarrativeCell()
                )
            ], collapsibleRow, idSource: skipIdPopulation ? null : new IdentifierSource(navigator));

            return row.Render(navigator, renderer, context);
        }
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
        Text,

        [EnumValueSet("http://hl7.org/fhir/fm-status")]
        Status
    }
}