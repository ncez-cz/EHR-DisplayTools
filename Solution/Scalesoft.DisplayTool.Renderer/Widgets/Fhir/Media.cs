using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Media : ColumnResourceBase<Media>, IResourceWidget
{
    public static string ResourceType => "Media";

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var headerInfo = new Container([
            new ConstantText("Záznam média"),
            new Optional("f:name",
                new ConstantText(" ("),
                new Text("@value"),
                new ConstantText(")")
            ),
            new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/event-status",
                new DisplayLabel(LabelCodes.Status))
        ]);

        var badge = new PlainBadge(new ConstantText("Základní informace"));

        var basicInfo = new Container([
            new Condition("f:basedOn",
                new NameValuePair(
                    new ConstantText("Založeno na"),
                    new CommaSeparatedBuilder("f:basedOn", _ => new AnyReferenceNamingWidget())
                )
            ),
            new Condition("f:partOf",
                new NameValuePair(
                    new ConstantText("Součástí"),
                    new CommaSeparatedBuilder("f:partOf", _ => new AnyReferenceNamingWidget())
                )
            ),
            new Optional("f:type", new NameValuePair(
                new ConstantText("Typ"),
                new CodeableConcept()
            )),
            new Optional("f:modality", new NameValuePair(
                new ConstantText("Modalita"),
                new CodeableConcept()
            )),
            new Optional("f:view",
                new NameValuePair(
                    new ConstantText("Pohled"),
                    new CodeableConcept()
                )
            ),
            new Optional("f:subject",
                new NameValuePair(
                    new ConstantText("Předmět"),
                    new AnyReferenceNamingWidget()
                )
            ),
        ]);

        var infrequentProperties =
            Widgets.InfrequentProperties.Evaluate<InfrequentProperties>([navigator]);

        var detailBadge = new PlainBadge(new ConstantText("Detailní informace"));
        var detailInfo = new Container([
            new If(_ => infrequentProperties.Contains(InfrequentProperties.Created), new NameValuePair(
                new ConstantText("Vytvořeno"),
                new Chronometry("created")
            )),
            new Optional("f:issued", new NameValuePair(
                new ConstantText("Vydáno"),
                new ShowDateTime()
            )),
            new Optional("f:operator", new NameValuePair(
                new ConstantText("Operátor"),
                new AnyReferenceNamingWidget()
            )),
            new Condition("f:reasonCode", new NameValuePair(
                new ConstantText("Důvod"),
                new CommaSeparatedBuilder("f:reasonCode", _ => [new CodeableConcept()])
            )),
            new Optional("f:bodySite", new NameValuePair(
                new DisplayLabel(LabelCodes.BodySite),
                new CodeableConcept()
            )),
            new Optional("f:deviceName", new NameValuePair(
                new ConstantText("Název zařízení"),
                new Text("@value")
            )),
            new Optional("f:device", new NameValuePair(
                new ConstantText("Zařízení"),
                new AnyReferenceNamingWidget()
            )),
        ]);

        var contentBadge = new PlainBadge(new ConstantText("Obsah"));
        var contentInfo = new Container([
            new Container([
                new ChangeContext("f:content",
                    new Attachment(navigator.SelectSingleNode("f:width").Node?.Value,
                        navigator.SelectSingleNode("f:height").Node?.Value,
                        navigator.SelectSingleNode("f:title").Node?.Value)
                )
            ], ContainerType.Div, "media-image-container")
        ]);


        var complete =
            new Collapser([headerInfo], [], [
                    badge,
                    basicInfo,
                    new If(
                        _ => infrequentProperties.Contains(InfrequentProperties.Created) ||
                             navigator.EvaluateCondition(
                                 "f:issued or f:operator or f:reasonCode or f:bodySite or f:deviceName or f:device"),
                        new ThematicBreak(),
                        detailBadge,
                        detailInfo
                    ),
                    new ThematicBreak(),
                    contentBadge,
                    contentInfo,
                    new Condition("f:note",
                        new ThematicBreak(),
                        new PlainBadge(new ConstantText("Poznámky")),
                        new ListBuilder("f:note", FlexDirection.Column, _ =>
                            [new ShowAnnotationCompact()]
                        )
                    )
                ], footer: navigator.EvaluateCondition("f:text") || navigator.EvaluateCondition("f:encounter")
                    ?
                    [
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
                        new Optional("f:text",
                            new NarrativeCollapser()
                        )
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()]
            );


        return complete.Render(navigator, renderer, context);
    }

    private enum InfrequentProperties
    {
        [OpenType("created")] Created
    }
}