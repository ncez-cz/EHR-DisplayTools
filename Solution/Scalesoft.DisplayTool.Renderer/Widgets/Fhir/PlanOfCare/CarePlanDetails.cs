using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
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
        
        var infrequentProperties = InfrequentProperties.Evaluate<CarePlanInfrequentProperties>(item);
        
        var resultWidget = new Concat(
            [
                new Row(
                    [
                        new Heading(
                            [
                                new ConstantText(title),
                                new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/request-status",
                                    new EhdsiDisplayLabel(LabelCodes.Status)),
                            ],
                            HeadingSize.H5,
                            "m-0 blue-color"
                        ),
                        new NarrativeModal(alignRight: false),
                    ],
                    flexContainerClasses: "gap-1 align-items-center"
                ),
                new Column(
                    [
                        new Row(
                            [
                                // ignore identifier
                                // ignore instantiatesCanonical
                                // ignore instantiatesUri
                                // ignore basedOn
                                // ignore replaces
                                // ignore partOf
                                infrequentProperties.Optional(CarePlanInfrequentProperties.Intent,
                                    new HideableDetails(
                                        new NameValuePair(
                                            new LocalizedLabel("care-plan.intent"),
                                            new EnumLabel(".", "http://hl7.org/fhir/ValueSet/care-plan-intent"),
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary
                                        )
                                    )
                                ),
                                infrequentProperties.Optional(CarePlanInfrequentProperties.Category,
                                    new HideableDetails(
                                        new NameValuePair(
                                            new LocalizedLabel("care-plan.category"),
                                            new ItemListBuilder(".", ItemListType.Unordered, _ => [new CodeableConcept()]),
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary
                                        )
                                    )
                                ),
                                infrequentProperties.Optional(CarePlanInfrequentProperties.Title,
                                    new NameValuePair(
                                        new EhdsiDisplayLabel(LabelCodes.Name),
                                        new Text("@value"),
                                        direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                ),
                                infrequentProperties.Optional(CarePlanInfrequentProperties.Description,
                                    new NameValuePair(
                                        new EhdsiDisplayLabel(LabelCodes.Description),
                                        new Text("@value"),
                                        direction: FlexDirection.Column,
                                        style: NameValuePair.NameValuePairStyle.Primary
                                    )
                                ),
                                // ignore subject
                                // ignore encounter
                                infrequentProperties.Optional(CarePlanInfrequentProperties.Period,
                                    new HideableDetails(
                                        new NameValuePair(
                                            new LocalizedLabel("care-plan.period"),
                                            new ShowPeriod(),
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary
                                        )
                                    )
                                ),
                                infrequentProperties.Optional(CarePlanInfrequentProperties.Created,
                                    new HideableDetails(
                                        new NameValuePair(
                                            new LocalizedLabel("care-plan.created"),
                                            new ShowDateTime(),
                                            direction: FlexDirection.Column,
                                            style: NameValuePair.NameValuePairStyle.Primary
                                        )
                                    )
                                ),
                                // ignore author
                                // ignore contributor
                                // ignore careTeam
                                infrequentProperties.Optional(CarePlanInfrequentProperties.Encounter,
                                    new HideableDetails(
                                        new AnyReferenceNamingWidget(
                                            widgetModel: new ReferenceNamingWidgetModel
                                            {
                                                Type = ReferenceNamingWidgetType.NameValuePair,
                                                Direction = FlexDirection.Column,
                                                Style = NameValuePair.NameValuePairStyle.Primary,
                                                LabelOverride = new LocalizedLabel("node-names.Encounter"),
                                            }
                                        )
                                    )
                                ),
                            ],
                            flexContainerClasses: "column-gap-6 row-gap-1"
                        ),
                        new Condition("f:text", new NarrativeCollapser()),
                    ],
                    flexContainerClasses: "px-2 gap-1"
                ),
            ]
        );

        // Render the card using the original navigator context
        return resultWidget.Render(item, renderer, context);
    }

    public enum CarePlanInfrequentProperties
    {
        Intent,
        Category,
        Title,
        Description,
        Period,
        Created,
        Encounter,
    }
}