using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PlanOfCare;

public class CarePlanDetails(XmlDocumentNavigator item) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator _,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        // Extract title for the card
        var title = item.SelectSingleNode("f:title/@value").Node?.Value ?? "Care Plan"; // Default title

        // Build the content widgets as before
        var contentWidgets = new List<Widget>
        {
            // ignore identifier
            // ignore instantiatesCanonical
            // ignore instantiatesUri
            // ignore basedOn
            // ignore replaces
            // ignore partOf
            new Optional("f:intent",
                new HideableDetails(new NameValuePair(
                    [new ConstantText("Záměr")],
                    [new EnumLabel(".", "http://hl7.org/fhir/ValueSet/care-plan-intent")]
                ))
            ),
            new Optional("f:category",
                new HideableDetails(new NameValuePair(
                    [new ConstantText("Kategorie")],
                    [new ItemListBuilder(".", ItemListType.Unordered, _ => [new CodeableConcept()])]
                ))
            ),
            new Optional("f:title",
                new NameValuePair(
                    [new DisplayLabel(LabelCodes.Name)],
                    [new Text("@value")]
                )
            ),
            new Optional("f:description",
                new NameValuePair(
                    [new DisplayLabel(LabelCodes.Description)],
                    [new Text("@value")]
                )
            ),
            // ignore subject
            // ignore encounter
            new Optional("f:period",
                new HideableDetails(new NameValuePair(
                    [new ConstantText("Období")],
                    [new ShowPeriod()]
                ))
            ),
            new Optional("f:created",
                new HideableDetails(new NameValuePair(
                    [new ConstantText("Datum vytvoření")],
                    [new ShowDateTime()]
                ))
            ),
            // ignore author
            // ignore contributor
            // ignore careTeam

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
            new NarrativeCollapser()
        };


        // Create the Card widget
        var card = new Card(
            new Row([
                new Container([
                    new ConstantText(title),
                    new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/request-status",
                        new DisplayLabel(LabelCodes.Status))
                ], ContainerType.Div, "d-flex align-items-center gap-1"),
                new NarrativeModal()
            ], flexContainerClasses: "align-items-center"),
            new Concat(contentWidgets));

        // Render the card using the original navigator context
        return card.Render(item, renderer, context);
    }
}