using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert;

public class FlagResourceCard(bool skipIdPopulation = true)
    : AlternatingBackgroundColumnResourceBase<FlagResourceCard>, IResourceWidget
{
    public static string ResourceType => "Flag";
    public static bool HasBorderedContainer(Widget widget) => false;

    public FlagResourceCard() : this(true)
    {
    }

    public static ResourceSummaryModel? RenderSummary(XmlDocumentNavigator item)
    {
        if (item.EvaluateCondition("f:code"))
        {
            return new ResourceSummaryModel
            {
                Value = new ChangeContext(item, "f:code", new CodeableConcept()),
            };
        }

        return null;
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<FlagInfrequentPropertiesPaths>(navigator);

        var resultWidget = new Concat(
            [
                new Row(
                    [
                        new Heading(
                            [
                                new Container([
                                    new TextContainer(TextStyle.Bold,
                                        [new ChangeContext("f:code", new CodeableConcept())]),
                                ], optionalClass: "blue-color d-flex align-items-center"),
                            ],
                            HeadingSize.H5,
                            "m-0"
                        ),
                        new EnumIconTooltip("f:status", "http://hl7.org/fhir/flag-status",
                            new EhdsiDisplayLabel(LabelCodes.Status)),
                        new NarrativeModal(alignRight: false),
                    ],
                    flexContainerClasses: "gap-1 align-items-center",
                    idSource: skipIdPopulation ? null : new IdentifierSource(navigator), flexWrap: false
                ),
                new Column(
                    [
                        new Row(
                            [
                                infrequentProperties.Optional(FlagInfrequentPropertiesPaths.Subject,
                                    new AnyReferenceNamingWidget(
                                        widgetModel: new ReferenceNamingWidgetModel
                                        {
                                            Type = ReferenceNamingWidgetType.NameValuePair,
                                            Direction = FlexDirection.Column,
                                            Style = NameValuePair.NameValuePairStyle.Primary,
                                            LabelOverride = new LocalizedLabel("flag.subject"),
                                        }
                                    )
                                ),
                                infrequentProperties.Condition(FlagInfrequentPropertiesPaths.Category,
                                    new HideableDetails(
                                        new NameValuePair(
                                            [new LocalizedLabel("flag.category")],
                                            [new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()])],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )
                                ),
                                infrequentProperties.Optional(FlagInfrequentPropertiesPaths.Period,
                                    new NameValuePair(
                                        [new EhdsiDisplayLabel(LabelCodes.Date)],
                                        [new ShowPeriod()],
                                        style: NameValuePair.NameValuePairStyle.Primary,
                                        direction: FlexDirection.Column
                                    )
                                ),
                                infrequentProperties.Optional(FlagInfrequentPropertiesPaths.Encounter,
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
                                    )),
                                infrequentProperties.Optional(FlagInfrequentPropertiesPaths.Author,
                                    new HideableDetails(
                                        new AnyReferenceNamingWidget(
                                            widgetModel: new ReferenceNamingWidgetModel
                                            {
                                                Type = ReferenceNamingWidgetType.NameValuePair,
                                                Direction = FlexDirection.Column,
                                                Style = NameValuePair.NameValuePairStyle.Primary,
                                                LabelOverride = new EhdsiDisplayLabel(LabelCodes.Author),
                                            }
                                        )
                                    )),
                            ],
                            flexContainerClasses: "column-gap-6 row-gap-1"
                        ),
                        new Condition("f:text", new NarrativeCollapser()),
                    ],
                    flexContainerClasses: "px-2 gap-1"
                ),
            ]
        );

        return await resultWidget.Render(navigator, renderer, context);
    }

    public enum FlagInfrequentPropertiesPaths
    {
        Category,
        Period,
        Encounter,
        Author,
        Subject,
    }
}