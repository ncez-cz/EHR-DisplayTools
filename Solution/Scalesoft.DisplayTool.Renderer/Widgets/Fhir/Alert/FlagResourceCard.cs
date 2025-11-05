using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Alert;

public class FlagResourceCard(bool skipIdPopulation = true)
    : AlternatingBackgroundColumnResourceBase<FlagResourceCard>, IResourceWidget
{
    public static string ResourceType => "Flag";

    public FlagResourceCard() : this(true)
    {
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<FlagInfrequentPropertiesPaths>([navigator]);

        var resultWidget = new Concat(
            [
                new Row(
                    [
                        new Heading(
                            [
                                new Container([
                                    new TextContainer(TextStyle.Bold,
                                        [new ChangeContext("f:code", new CodeableConcept())]),
                                    new EnumIconTooltip("f:status", "http://hl7.org/fhir/flag-status",
                                        new DisplayLabel(LabelCodes.Status)),
                                ], optionalClass: "blue-color d-flex align-items-center"),
                            ],
                            HeadingSize.H5,
                            "m-0"
                        ),
                        new NarrativeModal(alignRight: false),
                    ],
                    flexContainerClasses: "gap-1",
                    idSource: skipIdPopulation ? null : new IdentifierSource(navigator)
                ),
                new Column(
                    [
                        new Row(
                            [
                                InfrequentProperties.Optional(
                                    infrequentProperties,
                                    FlagInfrequentPropertiesPaths.Subject, new HideableDetails(
                                        new NameValuePair(
                                            [new ConstantText("Subjekt")],
                                            [new AnyReferenceNamingWidget()],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )),
                                new If(
                                    _ => infrequentProperties.Contains(FlagInfrequentPropertiesPaths.Category),
                                    new HideableDetails(
                                        new NameValuePair(
                                            [new ConstantText("Kategorie")],
                                            [new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()])],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
                                        )
                                    )
                                ),
                                new If(
                                    _ => infrequentProperties.Contains(FlagInfrequentPropertiesPaths.Period),
                                    new NameValuePair(
                                        [new DisplayLabel(LabelCodes.Date)],
                                        [new Optional("f:period", new ShowPeriod())],
                                        style: NameValuePair.NameValuePairStyle.Primary,
                                        direction: FlexDirection.Column
                                    )
                                ),
                                InfrequentProperties.Optional(
                                    infrequentProperties,
                                    FlagInfrequentPropertiesPaths.Encounter, new HideableDetails(
                                        new HideableDetails(
                                            new NameValuePair(
                                                [new ConstantText(Labels.Encounter)],
                                                [new AnyReferenceNamingWidget()],
                                                style: NameValuePair.NameValuePairStyle.Primary,
                                                direction: FlexDirection.Column
                                            )
                                        )
                                    )),
                                InfrequentProperties.Optional(
                                    infrequentProperties,
                                    FlagInfrequentPropertiesPaths.Author, new HideableDetails(
                                        new NameValuePair(
                                            [new DisplayLabel(LabelCodes.Author)],
                                            [new AnyReferenceNamingWidget()],
                                            style: NameValuePair.NameValuePairStyle.Primary,
                                            direction: FlexDirection.Column
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
        Identifier,
        Id,
        Category,
        Period,
        Encounter,
        Author,
        Text,

        [EnumValueSet("http://hl7.org/fhir/flag-status")]
        Status,
        Subject,
    }
}