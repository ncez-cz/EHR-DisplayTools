using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class VisionPrescriptionCard : ColumnResourceBase<VisionPrescriptionCard>, IResourceWidget
{
    public static string ResourceType => "VisionPrescription";

    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var navigators = navigator.SelectAllNodes("f:lensSpecification").ToList();
        List<Widget> cardsWithHeader =
        [
            new Row([
                new NameValuePair(
                    new LocalizedLabel("vision-prescription.patient"),
                    new ChangeContext("f:patient", new AnyReferenceNamingWidget()),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                ),
                new NameValuePair(
                    new LocalizedLabel("vision-prescription.prescriber"),
                    new ChangeContext("f:prescriber", new AnyReferenceNamingWidget()),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                ),
                new NameValuePair(
                    new LocalizedLabel("vision-prescription.dateWritten"),
                    new ShowDateTime("f:dateWritten"),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                ),
            ], flexContainerClasses: "column-gap-6"),
        ];

        List<Widget> lensSpecificationWidget = [];


        foreach (var lensSpecification in navigators)
        {
            var infrequentProperties =
                InfrequentProperties.Evaluate<LensSpecificationsInfrequentProperties>(lensSpecification);

            lensSpecificationWidget.Add(new ChangeContext(lensSpecification,
                new Card(null,
                    new Concat([
                        new HeadingNoMargin([new EnumLabel("f:eye", "http://hl7.org/fhir/ValueSet/vision-eye-codes")],
                            HeadingSize.H6, "blue-color capitalize-first fw-bold"),
                        new Row([
                            infrequentProperties.Optional(LensSpecificationsInfrequentProperties.Sphere,
                                new NameValuePair(
                                    new LocalizedLabel("vision-prescription.sphere"),
                                    new ShowDecimal(),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary)
                            ),
                            infrequentProperties.Optional(LensSpecificationsInfrequentProperties.BackCurve,
                                new NameValuePair(
                                    new LocalizedLabel("vision-prescription.backCurve"),
                                    new ShowDecimal(),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary)
                            ),
                            infrequentProperties.Optional(LensSpecificationsInfrequentProperties.Power,
                                new NameValuePair(
                                    new LocalizedLabel("vision-prescription.power"),
                                    new ShowDecimal(),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary)
                            ),
                            infrequentProperties.Optional(LensSpecificationsInfrequentProperties.Diameter,
                                new NameValuePair(
                                    new LocalizedLabel("vision-prescription.diameter"),
                                    new ShowDecimal(),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary)
                            ),
                            infrequentProperties.Optional(LensSpecificationsInfrequentProperties.Axis,
                                new NameValuePair(
                                    new LocalizedLabel("vision-prescription.axis"),
                                    new ShowDecimal(),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary)
                            ),
                            infrequentProperties.Optional(LensSpecificationsInfrequentProperties.Cylinder,
                                new NameValuePair(
                                    new LocalizedLabel("vision-prescription.cylinder"),
                                    new ShowDecimal(),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary)
                            ),
                            infrequentProperties.Condition(LensSpecificationsInfrequentProperties.Prism,
                                new NameValuePair(
                                    [new LocalizedLabel("vision-prescription.prism")],
                                    [
                                        new NameValuePair(new LocalizedLabel("vision-prescription.prism.amount"),
                                            new ConcatBuilder("f:prism", _ => [new ShowDecimal("f:amount")],
                                                separator: "; "),
                                            style: NameValuePair.NameValuePairStyle.Secondary),
                                        new NameValuePair(new LocalizedLabel("vision-prescription.prism.base"),
                                            new CommaSeparatedBuilder("f:prism", _ =>
                                                [new EnumLabel("f:base", "http://hl7.org/fhir/ValueSet/prism-base")]
                                            ), style: NameValuePair.NameValuePairStyle.Secondary
                                        )
                                    ], direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary)
                            ),
                            infrequentProperties.Optional(LensSpecificationsInfrequentProperties.Add,
                                new NameValuePair(
                                    new LocalizedLabel("vision-prescription.add"),
                                    new ShowDecimal(),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary)
                            ),
                            infrequentProperties.Optional(LensSpecificationsInfrequentProperties.Duration,
                                new NameValuePair(
                                    new LocalizedLabel("vision-prescription.duration"),
                                    new ShowQuantity(),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary)
                            ),
                            infrequentProperties.Optional(LensSpecificationsInfrequentProperties.Color,
                                new NameValuePair(
                                    new LocalizedLabel("vision-prescription.color"),
                                    new Text("@value"),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary)
                            ),
                            infrequentProperties.Optional(LensSpecificationsInfrequentProperties.Brand,
                                new NameValuePair(
                                    new LocalizedLabel("vision-prescription.brand"),
                                    new Text("@value"),
                                    direction: FlexDirection.Column,
                                    style: NameValuePair.NameValuePairStyle.Primary)
                            ),
                            infrequentProperties.Condition(LensSpecificationsInfrequentProperties.Note,
                                new NameValuePair(
                                    new LocalizedLabel("vision-prescription.note"),
                                    new ConcatBuilder("f:note", _ => [new ShowAnnotationCompact()]),
                                    direction: FlexDirection.Column,
                                    optionalClasses: new NameValuePair.NameValuePairClasses
                                    {
                                        OuterClass = "flex-grow-1 flex-basis-min-content",
                                    },
                                    style: NameValuePair.NameValuePairStyle.Primary)),
                        ]),
                    ]), optionalClass: "flex-fill")));
        }

        cardsWithHeader.Add(
            new Row(lensSpecificationWidget,
                flexContainerClasses: "justify-content-between column-gap-2",
                flexWrap: false)
        );

        var container = new Column(cardsWithHeader, flexContainerClasses: "row-gap-2 justify-content-start");

        var outerContainer = new Collapser([
                new LocalizedLabel("vision-prescription"),
                new Container([
                    GetLensProductTypeLabel(navigator),
                ], ContainerType.Span, "fw-bold black ms-4 capitalize-first"),
                new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/vision-prescription-status",
                    new EhdsiDisplayLabel(LabelCodes.Status)),
            ],
            [
                container,
            ],
            footer:
            navigator.EvaluateCondition("f:encounter or f:text")
                ?
                [
                    new Optional("f:encounter",
                        new ShowMultiReference(".",
                            (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                            x =>
                            [
                                new Collapser([new LocalizedLabel("node-names.Encounter")], x.ToList(),
                                    isCollapsed: true),
                            ]
                        )
                    ),
                    new If(_ => navigator.EvaluateCondition("f:text"),
                        new NarrativeCollapser()
                    ),
                ]
                : null,
            iconPrefix: [new NarrativeModal()]
        );
        return outerContainer.Render(navigator, renderer, context);
    }

    private Widget GetLensProductTypeLabel(XmlDocumentNavigator navigator)
    {
        var count = navigator.EvaluateNumber("count(f:product)");
        var types = navigator.SelectAllNodes("f:product/f:coding/f:code/@value").ToList();
        types.AddRange(navigator.SelectAllNodes("f:product/f:text/@value").ToList());

        var fallback = new LocalizedLabel("vision.prescription.lense-or-contacts");

        if (count == null || (int)count != types.Count || types.Distinct().Count() > 1)
        {
            return fallback;
        }


        return new ChangeContext(navigator.SelectSingleNode("f:lensSpecification/f:product"), new CodeableConcept());
    }
}

public enum LensSpecificationsInfrequentProperties
{
    Sphere,
    BackCurve,
    Power,
    Diameter,
    Axis,
    Cylinder,
    Prism,
    Add,
    Duration,
    Color,
    Brand,
    Note,
}