using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Allergy;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class NutritionOrder : SequentialResourceBase<NutritionOrder>, IResourceWidget
{
    public static string ResourceType => "NutritionOrder";
    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            // ignore identifier
            // ignore instantiatesCanonical
            // ignore instantiatesUri
            // ignore instantiates
            new NameValuePair([new LocalizedLabel("nutrition-order.intent")],
                [new EnumLabel("f:intent", "http://hl7.org/fhir/ValueSet/request-intent")]),
            // ignore patient
            new NameValuePair([new LocalizedLabel("nutrition-order.dateTime")], [new ShowDateTime("f:dateTime")]),
            new Optional("f:orderer",
                new NameValuePair(new LocalizedLabel("nutrition-order.orderer"), new AnyReferenceNamingWidget())),
        ];
        var labelCollapser =
            ReferenceHandler.BuildCollapserByMultireference(AllergyBuilder, navigator, context, "f:allergyIntolerance",
                new LocalizedLabel("node-names.AllergyIntolerance"));
        widgetTree.AddRange(labelCollapser);
        widgetTree.AddRange([
            new Optional("f:foodPreferenceModifier", new NameValuePair(
                [new LocalizedLabel("nutrition-order.foodPreferenceModifier")],
                [new ItemListBuilder(".", ItemListType.Unordered, _ => [new CodeableConcept()])])),
            new Optional("f:excludeFoodModifier", new NameValuePair(
                [new LocalizedLabel("nutrition-order.excludeFoodModifier")],
                [new ItemListBuilder(".", ItemListType.Unordered, _ => [new CodeableConcept()])])),
            new Optional("f:oralDiet", new Container([
                new Card(new LocalizedLabel("nutrition-order.oralDiet"), new Container([
                    new Optional("f:type", new NameValuePair([new LocalizedLabel("nutrition-order.oralDiet.type")],
                        [new ItemListBuilder(".", ItemListType.Unordered, _ => [new CodeableConcept()])])),
                    new Optional("f:schedule", new NameValuePair(
                        [new LocalizedLabel("nutrition-order.oralDiet.schedule")],
                        [
                            new ItemListBuilder(".", ItemListType.Unordered,
                                _ => [new ShowTiming(nameValuePairStyle: NameValuePair.NameValuePairStyle.Initial)])
                        ])),
                    new Optional("f:nutrient", new Card(new LocalizedLabel("nutrition-order.oralDiet.nutrient"),
                        new Container([
                            new ItemListBuilder(".", ItemListType.Unordered, _ =>
                            [
                                new Optional("f:modifier",
                                    new NameValuePair(
                                        [new LocalizedLabel("nutrition-order.oralDiet.nutrient.modifier")],
                                        [new CodeableConcept()])),
                                new Optional("f:amount",
                                    new NameValuePair([new LocalizedLabel("nutrition-order.oralDiet.nutrient.amount")],
                                        [new ShowQuantity()])),
                            ]),
                        ]))),
                    new Optional("f:texture", new Card(new LocalizedLabel("nutrition-order.oralDiet.texture"),
                        new Container([
                            new ItemListBuilder(".", ItemListType.Unordered, _ =>
                            [
                                new Optional("f:modifier",
                                    new NameValuePair([new LocalizedLabel("nutrition-order.oralDiet.texture.modifier")],
                                        [new CodeableConcept()])),
                                new Optional("f:foodType",
                                    new NameValuePair([new LocalizedLabel("nutrition-order.oralDiet.texture.foodType")],
                                        [new CodeableConcept()])),
                            ]),
                        ]))),
                    new Optional("f:fluidConsistencyType", new NameValuePair(
                        [new LocalizedLabel("nutrition-order.oralDiet.fluidConsistencyType")],
                        [new ItemListBuilder(".", ItemListType.Unordered, _ => [new CodeableConcept()])])),
                    new Optional("f:instruction",
                        new NameValuePair([new LocalizedLabel("nutrition-order.oralDiet.instruction")],
                            [new Text("@value")])),
                ])),
            ], ContainerType.Div, "my-2")),
            new Optional("f:supplement", new ConcatBuilder(".", _ =>
            [
                new Container([
                    new Card(new LocalizedLabel("nutrition-order.supplement"), new Container([
                        new Optional("f:type",
                            new NameValuePair([new LocalizedLabel("nutrition-order.supplement.type")],
                                [new CodeableConcept()])),
                        new Optional("f:productName",
                            new NameValuePair([new LocalizedLabel("nutrition-order.supplement.productName")],
                                [new Text("@value")])),
                        new Optional("f:schedule", new NameValuePair(
                            [new LocalizedLabel("nutrition-order.supplement.schedule")],
                            [
                                new ItemListBuilder(".", ItemListType.Unordered,
                                    _ => [new ShowTiming(nameValuePairStyle: NameValuePair.NameValuePairStyle.Initial)])
                            ])),
                        new Optional("f:quantity",
                            new NameValuePair([new LocalizedLabel("nutrition-order.supplement.quantity")],
                                [new ShowQuantity()])),
                        new Optional("f:instruction",
                            new NameValuePair([new LocalizedLabel("nutrition-order.supplement.instruction")],
                                [new Text("@value")])),
                    ])),
                ], ContainerType.Div, "my-2")
            ])),
            new Optional("f:enteralFormula", new Container([
                new Card(new LocalizedLabel("nutrition-order.enteralFormula"), new Container([
                    new Optional("f:baseFormulaType",
                        new NameValuePair([new LocalizedLabel("nutrition-order.enteralFormula.baseFormulaType")],
                            [new CodeableConcept()])),
                    new Optional("f:baseFormulaProductName",
                        new NameValuePair([new LocalizedLabel("nutrition-order.enteralFormula.baseFormulaProductName")],
                            [new Text("@value")])),
                    new Optional("f:additiveType",
                        new NameValuePair([new LocalizedLabel("nutrition-order.enteralFormula.additiveType")],
                            [new CodeableConcept()])),
                    new Optional("f:additiveProductName",
                        new NameValuePair([new LocalizedLabel("nutrition-order.enteralFormula.additiveProductName")],
                            [new Text("@value")])),
                    new Optional("f:caloricDensity",
                        new NameValuePair([new LocalizedLabel("nutrition-order.enteralFormula.caloricDensity")],
                            [new ShowQuantity()])),
                    new Optional("f:routeofAdministration",
                        new NameValuePair([new EhdsiDisplayLabel(LabelCodes.AdministrationRoute)],
                            [new CodeableConcept()])),
                    new Optional("f:administration", new Card(
                        new LocalizedLabel("nutrition-order.enteralFormula.administration"), new Container([
                            new ItemListBuilder(".", ItemListType.Unordered, _ =>
                            [
                                new Card(null, new Container([
                                    new Optional("f:schedule",
                                        new NameValuePair(
                                            [
                                                new LocalizedLabel(
                                                    "nutrition-order.enteralFormula.administration.schedule")
                                            ],
                                            [
                                                new ShowTiming(
                                                    nameValuePairStyle: NameValuePair.NameValuePairStyle.Initial)
                                            ])),
                                    new Optional("f:quantity",
                                        new NameValuePair(
                                        [
                                            new LocalizedLabel("nutrition-order.enteralFormula.administration.quantity")
                                        ], [new ShowQuantity()])),
                                    new Optional("f:rateQuantity",
                                        new NameValuePair(
                                            [new LocalizedLabel("nutrition-order.enteralFormula.administration.rate")],
                                            [new ShowQuantity()])),
                                    new Optional("f:rateRatio",
                                        new NameValuePair(
                                            [new LocalizedLabel("nutrition-order.enteralFormula.administration.rate")],
                                            [new ShowRatio()])),
                                ])),
                            ]),
                        ]))),
                    new Optional("f:administrationInstruction",
                        new NameValuePair(
                            [new LocalizedLabel("nutrition-order.enteralFormula.administrationInstruction")],
                            [new Text("@value")])),
                    new Optional("f:maxVolumeToDeliver",
                        new NameValuePair([new LocalizedLabel("nutrition-order.enteralFormula.maxVolumeToDeliver")],
                            [new ShowQuantity()])),
                ])),
            ], ContainerType.Div, "my-2")),
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
            new Choose([
                new When("f:text",
                    new NarrativeCollapser()
                ),
            ]),
        ]);
        // ignore note

        var widgetCollapser = new Collapser([
                new Row([
                    new LocalizedLabel("nutrition-order"),
                    new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/request-status",
                        new EhdsiDisplayLabel(LabelCodes.Status)),
                ], flexContainerClasses: "gap-1"),
            ],
            widgetTree, iconPrefix: [new NarrativeModal()]);

        return widgetCollapser.Render(navigator, renderer, context);
    }

    private Widget AllergyBuilder(List<ReferenceNavigatorOrDisplay> referenceData)
    {
        var result = new List<Widget>();
        var referencesWithResources = new List<XmlDocumentNavigator>();
        var referencesWithDisplay = new List<string>();
        foreach (var navigatorOrDisplay in referenceData)
        {
            if (navigatorOrDisplay.ResourceReferencePresent)
            {
                referencesWithResources.Add(navigatorOrDisplay.Navigator);
            }
            else
            {
                if (navigatorOrDisplay.ReferenceDisplay != null)
                {
                    referencesWithDisplay.Add(navigatorOrDisplay.ReferenceDisplay);
                }
            }
        }

        if (referencesWithResources.Count != 0)
        {
            result.Add(new AllergiesAndIntolerances(referencesWithResources));
        }

        if (referencesWithDisplay.Count != 0)
        {
            result.Add(new ItemList(ItemListType.Unordered,
                [..referencesWithDisplay.Select(x => new ConstantText(x))]));
        }

        return new Container(result);
    }
}