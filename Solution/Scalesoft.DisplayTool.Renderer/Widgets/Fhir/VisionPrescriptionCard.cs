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

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var navigators = navigator.SelectAllNodes("f:lensSpecification");
        List<Widget> specifications =
        [
            new Card(new ConstantText("Základní informace"),
                new Container([
                    new NameValuePair(
                        new ConstantText("Pacient"),
                        new ChangeContext("f:patient", new AnyReferenceNamingWidget())
                    ),
                    new NameValuePair(
                        new ConstantText("Předepisující"),
                        new ChangeContext("f:prescriber", new AnyReferenceNamingWidget())
                    ),
                    new NameValuePair(
                        new ConstantText("Datum a čas"),
                        new ShowDateTime("f:dateWritten")
                    ),
                ]))
        ];


        foreach (var lensSpecification in navigators)
        {
            var infrequentProperties =
                InfrequentProperties
                    .Evaluate<
                        VisionPrescriptionLensSpecificationInfrequentProperties>([lensSpecification]);

            specifications.Add(new ChangeContext(lensSpecification,
                new Card(new EnumLabel("f:eye", "http://hl7.org/fhir/ValueSet/vision-eye-codes"),
                    new Container([
                        new NameValuePair(new ConstantText("Produkt"),
                            new ChangeContext("f:product", new CodeableConcept())),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.Sphere),
                            new NameValuePair(new ConstantText("Sféra"), new ShowDecimal("f:sphere"))),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.BackCurve),
                            new NameValuePair(new ConstantText("Zakřivení"), new ShowDecimal("f:backCurve"))),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.Power),
                            new NameValuePair(new ConstantText("Síla"), new ShowDecimal("f:power"))),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.Diameter),
                            new NameValuePair(new ConstantText("Průměr"), new ShowDecimal("f:diameter"))),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.Axis),
                            new NameValuePair(new ConstantText("Osa"), new ShowDecimal("f:axis"))),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.Cylinder),
                            new NameValuePair(new ConstantText("Cylindr"), new ShowDecimal("f:cylinder"))),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.Prism),
                            new ConcatBuilder("f:prism",
                                _ =>
                                [
                                    new NameValuePair([new ConstantText("Prisma")],
                                    [
                                        new NameValuePair(new ConstantText("Množství"),
                                            new ShowDecimal("f:amount")),
                                        new NameValuePair(new ConstantText("Základ"),
                                            new EnumLabel("f:base", "http://hl7.org/fhir/ValueSet/prism-base"))
                                    ])
                                ])),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.Add),
                            new NameValuePair(new ConstantText("Addice"), new ShowDecimal("f:add"))),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.Duration),
                            new NameValuePair(new ConstantText("Doba nošení"), new ShowQuantity("f:duration"))),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.Color),
                            new NameValuePair(new ConstantText("Barva"), new Text("f:color/@value"))),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.Brand),
                            new NameValuePair(new ConstantText("Značka"), new Text("f:brand/@value"))),
                        new If(
                            _ => infrequentProperties.Contains(
                                VisionPrescriptionLensSpecificationInfrequentProperties.Note),
                            new NameValuePair(new ConstantText("Poznámka"),
                                new ConcatBuilder("f:note", _ => [new ShowAnnotationCompact()])))
                    ]))));
        }

        var container = new Row(specifications);

        var outerContainer = new Collapser([
                new ConstantText("Recept"),
                new ConstantText(" ("),
                GetLensProductTypeLabel(navigator),
                new ConstantText(")"),
                new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/vision-prescription-status",
                    new DisplayLabel(LabelCodes.Status)),
            ], [],
            [
                new Container([
                    container,
                    new Optional("f:encounter",
                        new ShowMultiReference(".",
                            (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                            x =>
                            [
                                new Collapser([new ConstantText(Labels.Encounter)], [], x.ToList(),
                                    isCollapsed: true)
                            ]
                        )
                    ),
                    new If(_ => navigator.EvaluateCondition("f:text"),
                        new NarrativeCollapser()
                    )
                ])
            ],
            iconPrefix: [new NarrativeModal()]
        );
        return outerContainer.Render(navigator, renderer, context);
    }

    private Widget GetLensProductTypeLabel(XmlDocumentNavigator navigator)
    {
        var count = navigator.EvaluateNumber("count(f:product)");
        var types = navigator.SelectAllNodes("f:product/f:coding/f:code/@value").ToList();
        types.AddRange(navigator.SelectAllNodes("f:product/f:text/@value").ToList());

        var fallback = new ConstantText("čočky / kontaktní čočky");

        if (count == null || (int)count != types.Count || types.Distinct().Count() > 1)
        {
            return fallback;
        }


        return new ChangeContext(navigator.SelectSingleNode("f:lensSpecification/f:product"), new CodeableConcept());
    }
}

public enum VisionPrescriptionLensSpecificationInfrequentProperties
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
    Note
}