using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;

public class DiagnosticReportCard(bool skipIdPopulation = false) : ColumnResourceBase<DiagnosticReportCard>, IResourceWidget
{
    public static string ResourceType => "DiagnosticReport";

    public DiagnosticReportCard() : this(false)
    {
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<DiagnosticInfrequentProperties>([navigator]);

        var headerInfo = new Container([
            new ConstantText("Diagnostická zpráva"),
            new HideableDetails(new ConstantText(" ("),
                new ChangeContext("f:code", new CodeableConcept()),
                new ConstantText(")"),
                new EnumIconTooltip("f:status", "http://hl7.org/fhir/ValueSet/observation-status",
                    new DisplayLabel(LabelCodes.Status)))
        ]);

        var badge = new PlainBadge(new ConstantText("Základní informace"));
        var basicInfo = new Container([
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.Category),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Druh pozorování"),
                    new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()])
                ))
            ),
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.Effective),
                new NameValuePair(
                    new ConstantText("Datum"),
                    new Chronometry("effective")
                )
            ),
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.Subject),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Subjekt"),
                    new AnyReferenceNamingWidget("f:subject")
                ))
            ),
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.BasedOn),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Založeno na"),
                    new ConcatBuilder("f:basedOn",
                        _ => [new AnyReferenceNamingWidget()], new LineBreak())
                ))
            ),
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.Issued),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Vydáno"),
                    new ShowDateTime("f:issued")
                ))
            ),
        ]);

        var resultBadge = new PlainBadge(new ConstantText("Výsledek"));
        var resultInfo = new Container([
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.Result),
                new NameValuePair(
                    new DisplayLabel(LabelCodes.Result),
                    new CommaSeparatedBuilder("f:result", _ => [new AnyReferenceNamingWidget()])
                )
            ),
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.Performer),
                new NameValuePair(
                    new DisplayLabel(LabelCodes.Performer),
                    new CommaSeparatedBuilder("f:performer", _ => [new AnyReferenceNamingWidget()])
                )
            ),
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.ResultsInterpreter),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Intrepret výsledků"),
                    new CommaSeparatedBuilder("f:resultsInterpreter", _ => [new AnyReferenceNamingWidget()])
                ))
            ),
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.Specimen),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Vzorek"),
                    new AnyReferenceNamingWidget("f:specimen")
                ))
            ),
            new If(
                _ => infrequentProperties.Contains(DiagnosticInfrequentProperties
                    .ImagingStudy),
                new HideableDetails(new NameValuePair(
                    new ConstantText("Obrazová studie"),
                    new ConcatBuilder("f:imagingStudy",
                        _ => [new AnyReferenceNamingWidget()], new LineBreak()
                    )
                ))
            ),
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.Media),
                new TextContainer(TextStyle.Bold, new ConstantText("Záznamy:")),
                new ListBuilder("f:media", FlexDirection.Row, _ =>
                [
                    new Card(null, new Container([
                            new NameValuePair(
                                new ConstantText("Média"),
                                new AnyReferenceNamingWidget("f:link")
                            ),
                            new Optional("f:comment",
                                new NameValuePair(
                                    new ConstantText("Popis"),
                                    new Text("@value")
                                )
                            ),
                        ])
                    )
                ])
            ),
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.Conclusion),
                new NameValuePair(
                    new ConstantText("Závěr"),
                    new Text("f:conclusion/@value")
                )
            ),
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.ConclusionCode),
                new NameValuePair(
                    new ConstantText("Kód závěru"),
                    new CommaSeparatedBuilder("f:conclusionCode", _ => [new CodeableConcept()])
                )
            ),
            new If(_ => infrequentProperties.Contains(DiagnosticInfrequentProperties.PresentedForm),
                new NameValuePair(
                    new ConstantText("Předložené formuláře"),
                    new CommaSeparatedBuilder("f:presentedForm", _ => [new Attachment()])
                )
            ),
        ]);

        var complete =
            new Collapser([headerInfo], [], [
                    badge,
                    basicInfo,
                    new If(
                        _ => infrequentProperties.ContainsAnyOf(DiagnosticInfrequentProperties.Result,
                            DiagnosticInfrequentProperties.Performer, DiagnosticInfrequentProperties.ResultsInterpreter,
                            DiagnosticInfrequentProperties.Specimen, DiagnosticInfrequentProperties.ImagingStudy,
                            DiagnosticInfrequentProperties.Media, DiagnosticInfrequentProperties.Conclusion,
                            DiagnosticInfrequentProperties.ConclusionCode,
                            DiagnosticInfrequentProperties.PresentedForm),
                        new ThematicBreak(),
                        resultBadge,
                        resultInfo
                    ),
                ],
                footer: infrequentProperties.ContainsAnyOf(DiagnosticInfrequentProperties.Encounter,
                    DiagnosticInfrequentProperties.Text)
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
                            new NarrativeCollapser(".")
                        ),
                    ]
                    : null,
                iconPrefix: [new NarrativeModal()],
                idSource: skipIdPopulation ? null : new IdentifierSource(navigator));
        return await complete.Render(navigator, renderer, context);
    }
}

public enum DiagnosticInfrequentProperties
{
    Text,
    BasedOn,
    Category,
    Subject,
    Encounter,
    [OpenType("effective")] Effective,
    Issued,
    Performer,
    ResultsInterpreter,
    Specimen,
    Result,
    ImagingStudy,
    Media,
    Conclusion,
    ConclusionCode,
    PresentedForm
}