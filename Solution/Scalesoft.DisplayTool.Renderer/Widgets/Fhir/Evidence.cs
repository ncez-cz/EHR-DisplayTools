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

    public Evidence() : this(null)
    {
    }

    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer, RenderContext context)
    {
        var headerInfo = new Container([
            collapserTitle ?? new ConstantText("Důkaz"),
            new Optional("f:title",
                new ConstantText(" ("),
                new Text("@value"),
                new ConstantText(")")
            )
        ], ContainerType.Span);

        var badge = new PlainBadge(new ConstantText("Základní informace"));

        var basicInfo = new Container([
            new Optional("f:title|f:shortTitle|f:name", new NameValuePair(
                new DisplayLabel(LabelCodes.Name),
                new Text("@value")
            )),
            new Optional("f:subtitle", new NameValuePair(
                new ConstantText("Podnázev"),
                new Text("@value")
            )),
            new NameValuePair(
                new ConstantText("Stav publikace"),
                new EnumLabel("f:status", "http://hl7.org/fhir/ValueSet/publication-status")
            ),
            new Optional("f:date", new NameValuePair(
                new ConstantText("Datum publikace"),
                new ShowDateTime()
            )),
            new Optional("f:description", new NameValuePair(
                new DisplayLabel(LabelCodes.Description),
                new Text("@value")
            )),
            new Optional("f:copyright", new NameValuePair(
                new ConstantText("Autorská práva"),
                new Markdown("@value")
            )),
            new Condition("f:jurisdiction", new NameValuePair(
                new ConstantText("Působnost"),
                new CommaSeparatedBuilder("f:jurisdiction", _ => [new CodeableConcept()])
            )),
            new Optional("f:approvalDate", new NameValuePair(
                new ConstantText("Datum schválení"),
                new ShowDateTime()
            )),
            new Optional("f:lastReviewDate", new NameValuePair(
                new ConstantText("Datum posledního přezkoumání"),
                new ShowDateTime()
            )),
            new Optional("f:effectivePeriod", new NameValuePair(
                new ConstantText("Platnost"),
                new ShowPeriod()
            )),
            new Condition("f:topic", new NameValuePair(
                new ConstantText("Témata"),
                new CommaSeparatedBuilder("f:topic", _ => [new CodeableConcept()])
            )),
        ], optionalClass: "name-value-pair-wrapper w-max-content");

        var actorInfo = new Container([
            new Condition("f:contact|f:publisher",
                new Container([
                    new PlainBadge(new ConstantText("Vydavatel")),
                    new Optional("f:publisher",
                        new NameValuePair(
                            new DisplayLabel(LabelCodes.Name),
                            new Text("@value")
                        )),
                    new Container([
                        new Condition("f:contact",
                            new TextContainer(TextStyle.Bold, new DisplayLabel(LabelCodes.Telecom)),
                            new Row([new ShowContactDetail("f:contact")])
                        )
                    ], ContainerType.Div, "mt-2")
                ])
            ),
            new Condition("f:author",
                new Container([
                    new PlainBadge(new ConstantText("Autor")),
                    new Row([
                        new ShowContactDetail("f:author")
                    ])
                ], optionalClass: "mt-2")
            ),
            new Condition("f:editor",
                new Container([
                    new PlainBadge(new ConstantText("Editor")),
                    new Row([new ShowContactDetail("f:editor")])
                ], optionalClass: "mt-2")
            ),
            new Condition("f:reviewer",
                new Container([
                    new PlainBadge(new ConstantText("Recenzent")),
                    new Row([new ShowContactDetail("f:reviewer")])
                ], optionalClass: "mt-2")
            ),
            new Condition("f:endorser",
                new Container([
                    new PlainBadge(new ConstantText("Podpora")),
                    new Row([new ShowContactDetail("f:endorser")])
                ], optionalClass: "mt-2")
            ),
        ]);

        var componentBadge = new PlainBadge(new ConstantText("Komponenty důkazu"));
        var componentInfo = new Container([
            new Container([
                new Condition("f:exposureBackground",
                    new NameValuePair(
                        new ConstantText("Zkoumaná populace"),
                        new AnyReferenceNamingWidget("f:exposureBackground")
                    )
                ),
                new Condition("f:exposureVariant",
                    new NameValuePair(
                        new ConstantText("Zkoumaná intervence"),
                        new AnyReferenceNamingWidget("f:exposureBackground")
                    )
                ),
                new Condition("f:outcome",
                    new NameValuePair(
                        new DisplayLabel(LabelCodes.Outcome),
                        new AnyReferenceNamingWidget("f:exposureBackground")
                    )
                ),
            ], optionalClass: "name-value-pair-wrapper w-max-content"),
            new Condition("f:relatedArtifact",
                new TextContainer(TextStyle.Bold, new ConstantText("Související artefakty:")),
                new ListBuilder("f:relatedArtifact", FlexDirection.Row, _ =>
                    [new RelatedArtifact()]
                )
            )
        ]);

        var complete =
            new Collapser([headerInfo], [], [
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
                        new PlainBadge(new ConstantText("Poznámky")),
                        new ListBuilder("f:note", FlexDirection.Column, _ =>
                            [new ShowAnnotationCompact()]
                        )
                    )
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
}