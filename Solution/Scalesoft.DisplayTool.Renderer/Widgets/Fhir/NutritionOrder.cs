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
            new NameValuePair([new ConstantText("Záměr")],
                [new EnumLabel("f:intent", "http://hl7.org/fhir/ValueSet/request-intent")]),
            // ignore patient
            new NameValuePair([new ConstantText("Datum vytvoření")], [new ShowDateTime("f:dateTime")]),
            new Optional("f:orderer",
                new NameValuePair(new ConstantText("Žadatel"), new AnyReferenceNamingWidget())),
        ];
        var labelCollapser =
            ReferenceHandler.BuildCollapserByMultireference(AllergyBuilder, navigator, context, "f:allergyIntolerance",
                "Alergie");
        widgetTree.AddRange(labelCollapser);
        widgetTree.AddRange([
            new Optional("f:foodPreferenceModifier", new NameValuePair([new ConstantText("Stravovací preference")],
                [new ItemListBuilder(".", ItemListType.Unordered, _ => [new CodeableConcept()])])),
            new Optional("f:excludeFoodModifier", new NameValuePair([new ConstantText("Vyloučit potraviny")],
                [new ItemListBuilder(".", ItemListType.Unordered, _ => [new CodeableConcept()])])),
            new Optional("f:oralDiet", new Container([
                new Card(new ConstantText("Perorální dieta"), new Container([
                    new Optional("f:type", new NameValuePair([new ConstantText("Typ diety / omezení")],
                        [new ItemListBuilder(".", ItemListType.Unordered, _ => [new CodeableConcept()])])),
                    new Optional("f:schedule", new NameValuePair([new ConstantText("Plán")],
                        [new ItemListBuilder(".", ItemListType.Unordered, _ => [new ShowTiming()])])),
                    new Optional("f:nutrient", new Card(new ConstantText("Úpravy složek potravy"), new Container([
                        new ItemListBuilder(".", ItemListType.Unordered, _ =>
                        [
                            new Optional("f:modifier",
                                new NameValuePair([new ConstantText("Složka")], [new CodeableConcept()])),
                            new Optional("f:amount",
                                new NameValuePair([new ConstantText("Množství")], [new ShowQuantity()])),
                        ]),
                    ]))),
                    new Optional("f:texture", new Card(new ConstantText("Úpravy struktury"), new Container([
                        new ItemListBuilder(".", ItemListType.Unordered, _ =>
                        [
                            new Optional("f:modifier",
                                new NameValuePair([new ConstantText("Struktura")], [new CodeableConcept()])),
                            new Optional("f:foodType",
                                new NameValuePair([new ConstantText("Typ jídla")], [new CodeableConcept()])),
                        ]),
                    ]))),
                    new Optional("f:fluidConsistencyType", new NameValuePair([new ConstantText("Konzistence tekutin")],
                        [new ItemListBuilder(".", ItemListType.Unordered, _ => [new CodeableConcept()])])),
                    new Optional("f:instruction",
                        new NameValuePair([new ConstantText("Instrukce / dodatečné informace")], [new Text("@value")])),
                ])),
            ], ContainerType.Div, "my-2")),
            new Optional("f:supplement", new ConcatBuilder(".", _ =>
            [
                new Container([
                    new Card(new ConstantText("Doplněk stravy"), new Container([
                        new Optional("f:type", new NameValuePair([new ConstantText("Typ")], [new CodeableConcept()])),
                        new Optional("f:productName",
                            new NameValuePair([new ConstantText("Název výrobku")], [new Text("@value")])),
                        new Optional("f:schedule", new NameValuePair([new ConstantText("Plán")],
                            [new ItemListBuilder(".", ItemListType.Unordered, _ => [new ShowTiming()])])),
                        new Optional("f:quantity",
                            new NameValuePair([new ConstantText("Množství")], [new ShowQuantity()])),
                        new Optional("f:instruction",
                            new NameValuePair([new ConstantText("Instrukce / dodatečné informace")],
                                [new Text("@value")])),
                    ])),
                ], ContainerType.Div, "my-2")
            ])),
            new Optional("f:enteralFormula", new Container([
                new Card(new ConstantText("Enterální směs"), new Container([
                    new Optional("f:baseFormulaType",
                        new NameValuePair([new ConstantText("Typ směsi")], [new CodeableConcept()])),
                    new Optional("f:baseFormulaProductName",
                        new NameValuePair([new ConstantText("Název výrobku - směs")], [new Text("@value")])),
                    new Optional("f:additiveType",
                        new NameValuePair([new ConstantText("Typ doplňkové látky")], [new CodeableConcept()])),
                    new Optional("f:additiveProductName",
                        new NameValuePair([new ConstantText("Název výrobku - doplňková látka")], [new Text("@value")])),
                    new Optional("f:caloricDensity",
                        new NameValuePair([new ConstantText("Kalorická hustota")], [new ShowQuantity()])),
                    new Optional("f:routeofAdministration",
                        new NameValuePair([new DisplayLabel(LabelCodes.AdministrationRoute)], [new CodeableConcept()])),
                    new Optional("f:administration", new Card(new ConstantText("Instrukce k podání"), new Container([
                        new ItemListBuilder(".", ItemListType.Unordered, _ =>
                        [
                            new Card(null, new Container([
                                new Optional("f:schedule",
                                    new NameValuePair([new ConstantText("Frekvence")], [new ShowTiming()])),
                                new Optional("f:quantity",
                                    new NameValuePair([new ConstantText("Množství")], [new ShowQuantity()])),
                                new Optional("f:rateQuantity",
                                    new NameValuePair([new ConstantText("Rychlost")], [new ShowQuantity()])),
                                new Optional("f:rateRatio",
                                    new NameValuePair([new ConstantText("Rychlost")], [new ShowRatio()])),
                            ])),
                        ])
                    ]))),
                    new Optional("f:administrationInstruction",
                        new NameValuePair([new ConstantText("Instrukce k podání")], [new Text("@value")])),
                    new Optional("f:maxVolumeToDeliver",
                        new NameValuePair([new ConstantText("Maximální objem za jednotku času")],
                            [new ShowQuantity()])),
                ]))
            ], ContainerType.Div, "my-2")),
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
            new Choose([
                new When("f:text",
                    new NarrativeCollapser()
                )
            ]),
        ]);
        // ignore note

        var widgetCollapser = new Collapser([
                new Row([
                    new ConstantText("Výživová doporučení"),
                    new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/request-status",
                        new DisplayLabel(LabelCodes.Status))
                ], flexContainerClasses: "gap-1")
            ], [],
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