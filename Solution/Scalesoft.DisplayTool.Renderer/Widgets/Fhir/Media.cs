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

    public static bool HasBorderedContainer(Widget widget) => true;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<MediaInfrequentProperties>(navigator);

        var headerInfo = new Container([
            new LocalizedLabel("media"),
            infrequentProperties.Optional(MediaInfrequentProperties.Name,
                new ConstantText(" ("),
                new Text("@value"),
                new ConstantText(")")
            ),
            new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/event-status",
                new EhdsiDisplayLabel(LabelCodes.Status))
        ]);

        var badge = new PlainBadge(new LocalizedLabel("general.basic-information"));

        var basicInfo = new Container([
            new Condition("f:basedOn",
                new NameValuePair(
                    new LocalizedLabel("media.basedOn"),
                    new CommaSeparatedBuilder("f:basedOn", _ => new AnyReferenceNamingWidget())
                )
            ),
            new Condition("f:partOf",
                new NameValuePair(
                    new LocalizedLabel("media.partOf"),
                    new CommaSeparatedBuilder("f:partOf", _ => new AnyReferenceNamingWidget())
                )
            ),
            infrequentProperties.Optional(MediaInfrequentProperties.Type,
                new NameValuePair(
                    new LocalizedLabel("media.type"),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Optional(MediaInfrequentProperties.Modality,
                new NameValuePair(
                    new LocalizedLabel("media.modality"),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Optional(MediaInfrequentProperties.View,
                new NameValuePair(
                    new LocalizedLabel("media.view"),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Optional(MediaInfrequentProperties.Subject,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("media.subject"),
                    }
                )),
        ]);

        var detailBadge = new PlainBadge(new LocalizedLabel("general.detailed-information"));
        var detailInfo = new Container([
            infrequentProperties.Condition(MediaInfrequentProperties.Created,
                new NameValuePair(
                    new LocalizedLabel("media.created"),
                    new Chronometry("created")
                )
            ),
            infrequentProperties.Optional(MediaInfrequentProperties.Issued,
                new NameValuePair(
                    new LocalizedLabel("media.issued"),
                    new ShowDateTime()
                )
            ),
            infrequentProperties.Optional(MediaInfrequentProperties.Operator,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("media.operator"),
                    }
                )
            ),
            infrequentProperties.Condition(MediaInfrequentProperties.ReasonCode,
                new NameValuePair(
                    new LocalizedLabel("media.reasonCode"),
                    new CommaSeparatedBuilder("f:reasonCode", _ => [new CodeableConcept()])
                )
            ),
            infrequentProperties.Optional(MediaInfrequentProperties.BodySite,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.BodySite),
                    new CodeableConcept()
                )
            ),
            infrequentProperties.Optional(MediaInfrequentProperties.DeviceName,
                new NameValuePair(
                    new LocalizedLabel("media.deviceName"),
                    new Text("@value")
                )
            ),
            infrequentProperties.Optional(MediaInfrequentProperties.Device,
                new AnyReferenceNamingWidget(
                    widgetModel: new ReferenceNamingWidgetModel
                    {
                        Type = ReferenceNamingWidgetType.NameValuePair,
                        LabelOverride = new LocalizedLabel("media.device"),
                    }
                )
            ),
        ]);

        var contentBadge = new PlainBadge(new LocalizedLabel("media.content"));
        var contentInfo = new Container([
            new Container([
                new ChangeContext("f:content",
                    new Attachment(navigator.SelectSingleNode("f:width").Node?.Value,
                        navigator.SelectSingleNode("f:height").Node?.Value,
                        navigator.SelectSingleNode("f:title").Node?.Value)
                ),
            ], ContainerType.Div, "media-image-container")
        ]);


        var complete =
            new Collapser([headerInfo], [
                    badge,
                    basicInfo,
                    new If(
                        _ => infrequentProperties.Contains(MediaInfrequentProperties.Created) ||
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
                        new PlainBadge(new LocalizedLabel("media.note")),
                        new ListBuilder("f:note", FlexDirection.Column, _ =>
                            [new ShowAnnotationCompact()]
                        )
                    ),
                ], footer: navigator.EvaluateCondition("f:text") || navigator.EvaluateCondition("f:encounter")
                    ?
                    [
                        infrequentProperties.Optional(MediaInfrequentProperties.Encounter,
                            new ShowMultiReference(".",
                                (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                                x =>
                                [
                                    new Collapser([new LocalizedLabel("node-names.Encounter")], x.ToList(),
                                        isCollapsed: true),
                                ]
                            )
                        ),
                        new Condition("f:text",
                            new NarrativeCollapser()
                        ),
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()]
            );


        return complete.Render(navigator, renderer, context);
    }

    private enum MediaInfrequentProperties
    {
        [OpenType("created")] Created,
        Subject,
        Type,
        Modality,
        View,
        Name,
        Issued,
        Operator,
        BodySite,
        DeviceName,
        Device,
        Encounter,
        ReasonCode,
    }
}