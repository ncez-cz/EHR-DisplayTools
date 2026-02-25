using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;

public class DiagnosticReport(bool skipIdPopulation = true)
    : AlternatingBackgroundColumnResourceBase<DiagnosticReport>, IResourceWidget
{
    public DiagnosticReport() : this(true)
    {
    }

    public static string ResourceType => "DiagnosticReport";

    public static bool RequiresExternalTitle => true;

    public static bool HasBorderedContainer(Widget widget) => false;

    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var infrequentProperties =
            InfrequentProperties.Evaluate<DiagnosticReportInfrequentProperties>(navigator);

        var nameValuePairs = new FlexList([
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.Category,
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("diagnostic-report.category"),
                    new CommaSeparatedBuilder("f:category", _ => [new CodeableConcept()]),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                ))
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.Effective,
                new NameValuePair(
                    new LocalizedLabel("diagnostic-report.effective"),
                    new Chronometry("effective"),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.Subject,
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("diagnostic-report.subject"),
                    new AnyReferenceNamingWidget("f:subject"),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                ))
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.BasedOn,
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("diagnostic-report.basedOn"),
                    new ConcatBuilder("f:basedOn",
                        _ => [new AnyReferenceNamingWidget()], new LineBreak()),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                ))
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.Issued,
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("diagnostic-report.issued"),
                    new ShowDateTime("f:issued"),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                ))
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.Media,
                new ConcatBuilder("f:media", _ =>
                [
                    new NameValuePair([new LocalizedLabel("diagnostic-report.media")],
                        [
                            new NameValuePair(
                                new LocalizedLabel("diagnostic-report.media.link"),
                                new AnyReferenceNamingWidget("f:link"),
                                style: NameValuePair.NameValuePairStyle.Secondary,
                                direction: FlexDirection.Row
                            ),
                            new Optional("f:comment",
                                new NameValuePair(
                                    new LocalizedLabel("diagnostic-report.media.comment"),
                                    new Text("@value"),
                                    style: NameValuePair.NameValuePairStyle.Secondary,
                                    direction: FlexDirection.Row
                                )
                            ),
                        ],
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    ),
                ])
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.Result,
                new NameValuePair(
                    new LocalizedLabel("diagnostic-report.result"),
                    new CommaSeparatedBuilder("f:result", _ => [new AnyReferenceNamingWidget()]),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.Performer,
                new NameValuePair(
                    new EhdsiDisplayLabel(LabelCodes.Performer),
                    new CommaSeparatedBuilder("f:performer", _ => [new AnyReferenceNamingWidget()]),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.ResultsInterpreter,
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("diagnostic-report.resultsInterpreter"),
                    new CommaSeparatedBuilder("f:resultsInterpreter",
                        _ => [new AnyReferenceNamingWidget()]),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                ))
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.Specimen,
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("diagnostic-report.specimen"),
                    new AnyReferenceNamingWidget("f:specimen"),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                ))
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties
                    .ImagingStudy,
                new HideableDetails(new NameValuePair(
                    new LocalizedLabel("diagnostic-report.imagingStudy"),
                    new CommaSeparatedBuilder("f:imagingStudy", _ => [new AnyReferenceNamingWidget()]),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                ))
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.Conclusion,
                new NameValuePair(
                    new LocalizedLabel("diagnostic-report.conclusion"),
                    new Text("f:conclusion/@value"),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.ConclusionCode,
                new NameValuePair(
                    new LocalizedLabel("diagnostic-report.conclusion"),
                    new CommaSeparatedBuilder("f:conclusionCode", _ => [new CodeableConcept()]),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Condition(DiagnosticReportInfrequentProperties.PresentedForm,
                new NameValuePair(
                    new LocalizedLabel("diagnostic-report.presentedForm"),
                    new CommaSeparatedBuilder("f:presentedForm", _ => [new Attachment()]),
                    style: NameValuePair.NameValuePairStyle.Primary,
                    direction: FlexDirection.Column
                )
            ),
            infrequentProperties.Optional(DiagnosticReportInfrequentProperties.Encounter,
                new HideableDetails(
                    new NameValuePair(
                        [new LocalizedLabel("node-names.Encounter")],
                        [new AnyReferenceNamingWidget()],
                        style: NameValuePair.NameValuePairStyle.Primary,
                        direction: FlexDirection.Column
                    )
                )),
        ], FlexDirection.Row, flexContainerClasses: "column-gap-6 row-gap-1");

        var resultWidget = new Concat([
            new Row([
                    new Container([
                        new TextContainer(TextStyle.Bold,
                            [new ChangeContext("f:code", new CodeableConcept())]),
                    ], optionalClass: "h5 m-0 blue-color"),
                    new EnumIconTooltip("f:status", "http://hl7.org/fhir/diagnostic-report-status",
                        new EhdsiDisplayLabel(LabelCodes.Status)),
                    new NarrativeModal(alignRight: false),
                ], flexContainerClasses: "gap-1 align-items-center",
                idSource: skipIdPopulation ? null : new IdentifierSource(navigator), flexWrap: false),
            new FlexList([
                nameValuePairs,
                ThematicBreak.SurroundedThematicBreak(
                    infrequentProperties, [
                        DiagnosticReportInfrequentProperties.Category,
                        DiagnosticReportInfrequentProperties.Effective,
                        DiagnosticReportInfrequentProperties.Subject,
                        DiagnosticReportInfrequentProperties.BasedOn,
                        DiagnosticReportInfrequentProperties.Issued,
                        DiagnosticReportInfrequentProperties.Media,
                        DiagnosticReportInfrequentProperties.Encounter,
                        DiagnosticReportInfrequentProperties.Result,
                        DiagnosticReportInfrequentProperties.Performer,
                        DiagnosticReportInfrequentProperties.ResultsInterpreter,
                        DiagnosticReportInfrequentProperties.Specimen,
                        DiagnosticReportInfrequentProperties.ImagingStudy,
                        DiagnosticReportInfrequentProperties.Conclusion,
                        DiagnosticReportInfrequentProperties.ConclusionCode,
                        DiagnosticReportInfrequentProperties.PresentedForm,
                    ], [
                        DiagnosticReportInfrequentProperties.Text,
                    ]
                ),
                new Condition("f:text", new NarrativeCollapser()),
            ], FlexDirection.Column, flexContainerClasses: "px-2 gap-1")
        ]);

        return resultWidget.Render(navigator, renderer, context);
    }

    private enum DiagnosticReportInfrequentProperties
    {
        Text,
        [HiddenInSimpleMode] BasedOn,
        [HiddenInSimpleMode] Category,
        [HiddenInSimpleMode] Subject,
        Encounter,
        [OpenType("effective")] Effective,
        [HiddenInSimpleMode] Issued,
        Performer,
        [HiddenInSimpleMode] ResultsInterpreter,
        [HiddenInSimpleMode] Specimen,
        Result,
        [HiddenInSimpleMode] ImagingStudy,
        Media,
        Conclusion,
        ConclusionCode,
        PresentedForm,
    }
}