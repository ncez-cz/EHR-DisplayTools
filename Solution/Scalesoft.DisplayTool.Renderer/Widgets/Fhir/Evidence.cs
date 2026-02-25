using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Evidence(
    Widget? collapserTitle = null,
    List<Widget>? variableAdditions = null
) : ColumnResourceBase<Evidence>, IResourceWidget
{
    public static string ResourceType => "Evidence";

    public static bool HasBorderedContainer(Widget widget) => true;

    public Evidence() : this(null)
    {
    }

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties = InfrequentProperties.Evaluate<EvidenceInfrequentProperties>(navigator);

        var headerInfo = new Container([
            collapserTitle ?? new LocalizedLabel("evidence"),
            new Optional("f:title",
                new ConstantText(" ("),
                new Text("@value"),
                new ConstantText(")")
            )
        ], ContainerType.Span);

        var badge = new PlainBadge(new LocalizedLabel("general.basic-information"));

        var basicInfo = new Container([
            new Optional("f:title|f:shortTitle|f:name",
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.Name),
                    new Text("@value")
                )
            ),
            infrequentProperties.Optional(EvidenceInfrequentProperties.Subtitle,
                new NameValuePair(
                    new LocalizedLabel("evidence.subtitle"),
                    new Text("@value")
                )
            ),
            new NameValuePair(
                new LocalizedLabel("evidence.status"),
                new EnumLabel("f:status", "http://hl7.org/fhir/ValueSet/publication-status")
            ),
            infrequentProperties.Optional(EvidenceInfrequentProperties.Date,
                new NameValuePair(
                    new LocalizedLabel("evidence.date"),
                    new ShowDateTime()
                )
            ),
            infrequentProperties.Optional(EvidenceInfrequentProperties.Description,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.Description),
                    new Text("@value")
                )
            ),
            infrequentProperties.Optional(EvidenceInfrequentProperties.Copyright,
                new NameValuePair(
                    new LocalizedLabel("evidence.copyright"),
                    new Markdown("@value")
                )
            ),
            infrequentProperties.Condition(EvidenceInfrequentProperties.Jurisdiction,
                new NameValuePair(
                    new LocalizedLabel("evidence.jurisdiction"),
                    new CommaSeparatedBuilder("f:jurisdiction", _ => [new CodeableConcept()])
                )
            ),
            infrequentProperties.Optional(EvidenceInfrequentProperties.ApprovalDate,
                new NameValuePair(
                    new LocalizedLabel("evidence.approvalDate"),
                    new ShowDateTime()
                )
            ),
            infrequentProperties.Optional(EvidenceInfrequentProperties.LastReviewDate,
                new NameValuePair(
                    new LocalizedLabel("evidence.lastReviewDate"),
                    new ShowDateTime()
                )
            ),
            infrequentProperties.Optional(EvidenceInfrequentProperties.EffectivePeriod,
                new NameValuePair(
                    new LocalizedLabel("evidence.effectivePeriod"),
                    new ShowPeriod()
                )
            ),
            infrequentProperties.Condition(EvidenceInfrequentProperties.Topic,
                new NameValuePair(
                    new LocalizedLabel("evidence.topic"),
                    new CommaSeparatedBuilder("f:topic", _ => [new CodeableConcept()])
                )
            ),
        ], optionalClass: "name-value-pair-wrapper w-fit-content");

        var actorInfo = new Container([
            new Condition("f:contact|f:publisher",
                new Container([
                    new PlainBadge(new LocalizedLabel("evidence.publisher")),
                    new Optional("f:publisher",
                        new NameValuePair(
                            new EhdsiDisplayLabel(LabelCodes.Name),
                            new Text("@value")
                        )),
                    new Container([
                        new Condition("f:contact",
                            new TextContainer(TextStyle.Bold, new EhdsiDisplayLabel(LabelCodes.Telecom)),
                            new Row([new ShowContactDetail("f:contact")])
                        )
                    ], ContainerType.Div, "mt-2"),
                ])
            ),
            infrequentProperties.Optional(EvidenceInfrequentProperties.Author,
                new Container([
                    new PlainBadge(new LocalizedLabel("evidence.author")),
                    new Row([new ShowContactDetail(".")]),
                ], optionalClass: "mt-2")
            ),
            infrequentProperties.Optional(EvidenceInfrequentProperties.Editor,
                new Container([
                    new PlainBadge(new LocalizedLabel("evidence.editor")),
                    new Row([new ShowContactDetail(".")]),
                ], optionalClass: "mt-2")
            ),
            infrequentProperties.Optional(EvidenceInfrequentProperties.Reviewer,
                new Container([
                    new PlainBadge(new LocalizedLabel("evidence.reviewer")),
                    new Row([new ShowContactDetail(".")]),
                ], optionalClass: "mt-2")
            ),
            infrequentProperties.Optional(EvidenceInfrequentProperties.Endoser,
                new Container([
                    new PlainBadge(new LocalizedLabel("evidence.endorser")),
                    new Row([new ShowContactDetail(".")]),
                ], optionalClass: "mt-2")
            ),
        ]);

        var componentBadge = new PlainBadge(new LocalizedLabel("evidence.evidence-components"));
        var componentInfo = new Container([
            new Container([
                infrequentProperties.Optional(EvidenceInfrequentProperties.ExposureBackground,
                    new AnyReferenceNamingWidget(
                        widgetModel: new ReferenceNamingWidgetModel
                        {
                            Type = ReferenceNamingWidgetType.NameValuePair,
                            LabelOverride = new LocalizedLabel("evidence.exposureBackground"),
                        }
                    )
                ),
                infrequentProperties.Optional(EvidenceInfrequentProperties.ExposureVariant,
                    new AnyReferenceNamingWidget(
                        widgetModel: new ReferenceNamingWidgetModel
                        {
                            Type = ReferenceNamingWidgetType.NameValuePair,
                            LabelOverride = new LocalizedLabel("evidence.exposureVariant"),
                        }
                    )
                ),
                infrequentProperties.Optional(EvidenceInfrequentProperties.Outcome,
                    new AnyReferenceNamingWidget(
                        widgetModel: new ReferenceNamingWidgetModel
                        {
                            Type = ReferenceNamingWidgetType.NameValuePair,
                            LabelOverride = new EhdsiDisplayLabel(LabelCodes.Outcome),
                        }
                    )
                ),
            ], optionalClass: "name-value-pair-wrapper w-fit-content"),
            new Condition("f:relatedArtifact",
                new TextContainer(
                    TextStyle.Bold,
                    [new LocalizedLabel("evidence.relatedArtifact"), new ConstantText(":")]
                ),
                new ListBuilder("f:relatedArtifact", FlexDirection.Row, _ =>
                    [new RelatedArtifact()]
                )
            ),
        ]);

        var complete =
            new Collapser([headerInfo], [
                    badge,
                    basicInfo,
                    new Condition("f:contact or f:publisher or f:author or f:editor or f:reviewer or f:endorser",
                        new ThematicBreak(),
                        actorInfo
                    ),
                    new Condition("f:exposureBackground or f:exposureVariant or f:outcome",
                        new ThematicBreak(),
                        componentBadge,
                        componentInfo
                    ),
                    variableAdditions != null
                        ? new Concat(variableAdditions)
                        : new NullWidget(),
                    new Condition("f:note",
                        new ThematicBreak(),
                        new PlainBadge(new LocalizedLabel("evidence.note")),
                        new ListBuilder("f:note", FlexDirection.Column, _ =>
                            [new ShowAnnotationCompact()]
                        )
                    ),
                ], footer: navigator.EvaluateCondition("f:text")
                    ?
                    [
                        new NarrativeCollapser()
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()]
            );


        return complete.Render(navigator, renderer, context);
    }

    public enum EvidenceInfrequentProperties
    {
        Subtitle,
        Date,
        Description,
        Copyright,
        ApprovalDate,
        LastReviewDate,
        EffectivePeriod,
        ExposureBackground,
        ExposureVariant,
        Outcome,
        Jurisdiction,
        Topic,
        Author,
        Editor,
        Reviewer,
        Endoser,
    }
}