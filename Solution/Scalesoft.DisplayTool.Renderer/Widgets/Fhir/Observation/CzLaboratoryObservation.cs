using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Observation;

public class CzLaboratoryObservation(List<XmlDocumentNavigator> items) : Widget
{
    public const string XPathCondition =
        "f:category/f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/observation-category' and f:code/@value='laboratory']";

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var table = new Table(
            [
                new TableHead([
                    new TableRow([
                        new TableCell([new LocalizedLabel("observation.test")], TableCellType.Header),
                        new TableCell([new LocalizedLabel("observation.value")], TableCellType.Header,
                            containerClass: "border-end-0"),
                        new TableCell([new NullWidget()], TableCellType.Header, containerClass: "border-start-0"),
                        new TableCell([new LocalizedLabel("observation.value.unit")], TableCellType.Header),
                        new TableCell([new LocalizedLabel("observation.referenceRange-short")], TableCellType.Header),
                        new TableCell([new LocalizedLabel("observation.interpretation")], TableCellType.Header,
                            containerClass: "border-end-0"),
                        new TableCell([new NullWidget()], TableCellType.Header, containerClass: "border-start-0"),
                    ])
                ]),
                ..items.Select(x => new CzLaboratoryRowBuilder(x)),
            ]
        );

        return await table.Render(navigator, renderer, context);
    }

    private class CzLaboratoryRowBuilder(XmlDocumentNavigator item) : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var rowDetails = new StructuredDetails();

            Widget[] tableRowContent =
            [
                new LabResultsRow(rowDetails),
            ];

            var infrequentProperties =
                InfrequentProperties.Evaluate<CzLabObservationInfrequentProperties>(item);

            // first sub-row
            var rowDetailsChildren = new List<Widget>();
            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.CertifiedRefMaterialCodeableExtension,
                    out var certRefMatCodedPath))
            {
                rowDetailsChildren.Add(new Container([
                    new PlainBadge(new LocalizedLabel("observation.certifiedRefMaterialCodeable")), new LineBreak(),
                    new ListBuilder(
                        certRefMatCodedPath,
                        FlexDirection.Column,
                        _ =>
                        [
                            new Container(
                                new ChangeContext("f:valueCodeableConcept", new CodeableConcept()),
                                ContainerType.Span
                            )
                        ])
                ]));
            }

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.CertifiedRefMaterialIdentiferExtension,
                    out var certRefMatIdPath))
            {
                rowDetailsChildren.Add(new Container([
                    new PlainBadge(new LocalizedLabel("observation.certifiedRefMaterialIdentifier")), new LineBreak(),
                    new ListBuilder(
                        certRefMatIdPath,
                        FlexDirection.Column,
                        _ =>
                        [
                            new ShowIdentifier(),
                        ])
                ]));
            }

            if (item.EvaluateCondition("f:category"))
            {
                rowDetailsChildren.Add(new Container([
                    new PlainBadge(new LocalizedLabel("observation.category")), new LineBreak(),
                    new CommaSeparatedBuilder("f:category",
                        _ =>
                        [
                            new CodeableConcept(),
                        ])
                ]));
            }

            // ignore subject

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.Focus, out var focusPath))
            {
                rowDetailsChildren.Add(new Container([
                    new PlainBadge(new LocalizedLabel("observation.focus")), new LineBreak(),
                    new CommaSeparatedBuilder(focusPath,
                        _ => [new AnyReferenceNamingWidget()])
                ]));
            }

            rowDetailsChildren.Add(new Container([
                new PlainBadge(new LocalizedLabel("observation.effective")), new LineBreak(),
                new Chronometry("effective")
            ]));

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.Issued, out var issuedPath))
            {
                rowDetailsChildren.Add(new Container([
                    new PlainBadge(new LocalizedLabel("observation.issued")), new LineBreak(),
                    new ShowInstant(issuedPath)
                ]));
            }

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.BodySite, out var bodySiteXpath))
            {
                rowDetailsChildren.Add(new Container([
                    new PlainBadge(new EhdsiDisplayLabel(LabelCodes.BodySite)), new LineBreak(),
                    new ChangeContext(bodySiteXpath, new CodeableConcept())
                ]));
            }

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.Method, out var methodXpath))
            {
                rowDetailsChildren.Add(new Container([
                    new PlainBadge(new LocalizedLabel("observation.method")), new LineBreak(),
                    new ChangeContext(methodXpath, new CodeableConcept())
                ]));
            }

            // second sub-row

            var cardsRow = new List<Widget>();

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.TriggeredByR5Extension,
                    out var triggeredByXpath))
            {
                cardsRow.Add(new Container([
                    new PlainBadge(new LocalizedLabel("observation.triggeredBy")),
                    new ListBuilder(triggeredByXpath,
                        FlexDirection.Column,
                        (_, supportInfoNav) =>
                        {
                            var supportedSubExtensions = new List<Widget>();
                            if (supportInfoNav.EvaluateCondition("f:extension[@url='type']"))
                            {
                                supportedSubExtensions.Add(
                                    new NameValuePair(
                                        new LocalizedLabel("observation.triggeredBy.type"),
                                        new CommaSeparatedBuilder("f:extension[@url='type']", _ =>
                                        [
                                            new EnumLabel("f:valueCode",
                                                "http://hl7.org/fhir/ValueSet/observation-triggeredbytype"),
                                        ])
                                    )
                                );
                            }

                            if (supportInfoNav.EvaluateCondition("f:extension[@url='reason']"))
                            {
                                supportedSubExtensions.Add(
                                    new NameValuePair(
                                        new LocalizedLabel("observation.triggeredBy.reason"),
                                        new CommaSeparatedBuilder("f:extension[@url='reason']", _ =>
                                        [
                                            new Text("f:valueString/@value"),
                                        ])
                                    )
                                );
                            }

                            if (supportInfoNav.EvaluateCondition("f:extension[@url='observation']"))
                            {
                                supportedSubExtensions.Add(new ListBuilder("f:extension[@url='observation']",
                                    FlexDirection.Column, _ =>
                                    [
                                        new AnyReferenceNamingWidget("f:valueReference"),
                                    ]));
                            }

                            return supportedSubExtensions;
                        }, separator: new LineBreak(), flexContainerClasses: string.Empty), // Overrides the default class
                ]));
            }

            // third sub-row

            var referenceLinksRow = new List<DetailItem>();
            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.LabTestKitExtension,
                    out var labTestKitPath))
            {
                referenceLinksRow.Add(new NameValuePairDetail(new LocalizedLabel("observation.labTestKit"),
                    new CommaSeparatedBuilder(
                        labTestKitPath,
                        _ =>
                        [
                            new AnyReferenceNamingWidget("f:valueReference"),
                        ]))
                );
            }

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.Specimen, out var specimenXpath))
            {
                referenceLinksRow.Add(new NameValuePairDetail(new LocalizedLabel("observation.specimen"),
                    new AnyReferenceNamingWidget(specimenXpath))
                );
            }

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.PartOf, out var partOfPath))
            {
                referenceLinksRow.Add(new NameValuePairDetail(new LocalizedLabel("observation.partOf"),
                    new CommaSeparatedBuilder(
                        partOfPath,
                        _ =>
                        [
                            new AnyReferenceNamingWidget(),
                        ]))
                );
            }

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.Device, out var deviceXpath))
            {
                referenceLinksRow.Add(new NameValuePairDetail(new LocalizedLabel("observation.device"),
                    new AnyReferenceNamingWidget(deviceXpath))
                );
            }

            if (item.EvaluateCondition("f:encounter"))
            {
                referenceLinksRow.Add(new NameValuePairDetail(new LocalizedLabel("node-names.Encounter"),
                    new AnyReferenceNamingWidget("f:encounter"))
                );
            }

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.BasedOn, out var basedOnPath))
            {
                referenceLinksRow.Add(new NameValuePairDetail(new LocalizedLabel("observation.basedOn"),
                    new CommaSeparatedBuilder(
                        basedOnPath,
                        _ =>
                        [
                            new AnyReferenceNamingWidget(),
                        ]))
                );
            }


            referenceLinksRow.Add(new NameValuePairDetail(new LocalizedLabel("observation.performer"),
                new CommaSeparatedBuilder(
                    "f:performer",
                    _ =>
                    [
                        new AnyReferenceNamingWidget(),
                        new Optional(
                            "f:extension[@url='http://hl7.org/fhir/StructureDefinition/event-performerFunction']",
                            new HideableDetails(
                                new ConstantText(" ( "),
                                new LocalizedLabel("observation.performer.function"),
                                new ChangeContext("f:valueCodeableConcept", new CodeableConcept()),
                                new ConstantText(")"))
                        ),
                    ])));

            // ignore note

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.SupportingInfoExtension,
                    out var supportInfoXpath))
            {
                referenceLinksRow.Add(new NameValuePairDetail(new LocalizedLabel("observation.supportingInfo"),
                    new CommaSeparatedBuilder(
                        supportInfoXpath,
                        _ =>
                        [
                            new AnyReferenceNamingWidget("f:valueReference"),
                        ])));
            }

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.HasMember, out var hasMemberXpath))
            {
                referenceLinksRow.Add(
                    new NameValuePairDetail(
                        new LocalizedLabel("observation.hasMember"),
                        new CommaSeparatedBuilder(hasMemberXpath, _ =>
                            [new AnyReferenceNamingWidget()])
                    )
                );
            }

            if (infrequentProperties.TryGet(CzLabObservationInfrequentProperties.DerivedFrom, out var derivedFromXpath))
            {
                referenceLinksRow.Add(
                    new NameValuePairDetail(
                        new LocalizedLabel("observation.derivedFrom"),
                        new CommaSeparatedBuilder(derivedFromXpath, _ =>
                            [new AnyReferenceNamingWidget()])
                    )
                );
            }


            if (item.EvaluateCondition("f:text"))
            {
                rowDetails.Add(
                    new CollapsibleDetail(
                        new EhdsiDisplayLabel(LabelCodes.OriginalNarrative),
                        new Narrative("f:text")
                    )
                );
            }

            if (rowDetailsChildren.Count != 0)
            {
                rowDetails.Add(
                    new RawDetail(new Row(rowDetailsChildren, flexContainerClasses: "gap-5", wrapChildren: true)));
                if (cardsRow.Count != 0 || referenceLinksRow.Count != 0)
                {
                    rowDetails.Add(new RawDetail(new ThematicBreak()));
                }
            }

            if (cardsRow.Count != 0)
            {
                rowDetails.Add(
                    new RawDetail(new Row(cardsRow, flexContainerClasses: "gap-5", wrapChildren: true)));
                if (referenceLinksRow.Count != 0)
                {
                    rowDetails.Add(new RawDetail(new ThematicBreak()));
                }
            }

            rowDetails.AddRange(referenceLinksRow);

            List<Widget> rowWidgets =
            [
                new TableRow(tableRowContent, rowDetails, idSource: item),
            ];
            if (item.EvaluateCondition("f:component"))
            {
                foreach (var componentNav in item.SelectAllNodes("f:component"))
                {
                    var componentRowDetails = new StructuredDetails();
                    var rowContent = new ChangeContext(componentNav, new LabResultsRow(componentRowDetails));
                    var componentRow = new TableRow([rowContent], componentRowDetails,
                        optionalClass: "sub-component-row");
                    rowWidgets.Add(componentRow);
                }
            }

            return new TableBody(rowWidgets).Render(item, renderer, context);
        }
    }

    private class LabResultsRow(StructuredDetails rowDetails) : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var interpretationCodingVal =
                navigator.SelectAllNodes("f:interpretation/f:coding/f:code/@value").Select(x => x.Node).ToList()
                    .WhereNotNull()
                    .ToList();

            var trendIconNavigator = navigator
                .SelectAllNodes("f:interpretation")
                .FirstOrDefault(interpNav => interpNav
                    .SelectAllNodes("f:coding/f:code/@value")
                    .Any(node =>
                    {
                        var code = node.Node?.Value.ToLower().ToEnum<SupportedCodes>();
                        return code != null && EnumIconTooltip.TryGetIcon(code.Value, "", out _);
                    }));

            var normalityClasses = interpretationCodingVal.Select(x => x.Value)
                .Select(GetNormalityClass).WhereNotNull()
                .ToList();

            var statusIcon = new EnumIconTooltip("f:status", "http://hl7.org/fhir/observation-status",
                new EhdsiDisplayLabel(LabelCodes.Status));

            Widget[] widgetTree =
            [
                new TableCell([new ChangeContext("f:code", new CodeableConcept())]),
                new TableCell([
                    // displaying multiple normality background colors makes no sense
                    new If(_ => normalityClasses.Count == 1, new LazyWidget(() =>
                    [
                        new Container(
                            new OpenTypeElement(rowDetails, hints: OpenTypeElementRenderingHints.HideQuantityUnit),
                            ContainerType.Span, optionalClass: "lab-abnormal-value " + normalityClasses.First()),
                    ])).Else(
                        new OpenTypeElement(rowDetails, hints: OpenTypeElementRenderingHints.HideQuantityUnit)
                    )
                ], optionalClass: "text-center", containerClass: "border-end-0"),
                new TableCell([
                    new If(_ => trendIconNavigator != null,
                        new ChangeContext(trendIconNavigator!,
                            new CodeableConceptIconTooltip(new LocalizedLabel("observation.interpretation")))
                    )
                ], containerClass: "border-start-0"),
                new TableCell([new ShowQuantityUnit("f:valueQuantity")]),
                new TableCell([
                    new ConcatBuilder("f:referenceRange", _ => [new ShowReferenceRange()], new LineBreak())
                ]),
                new TableCell([
                        new ConcatBuilder("f:interpretation", _ =>
                        [
                            new Choose([
                                new When(
                                    "f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation' and (f:code/@value='LL' or f:code/@value='LU' or f:code/@value='L' or f:code/@value='N' or f:code/@value='H' or f:code/@value='HU' or f:code/@value='HH')]",
                                    new Container([new ShowInterpretationScale()],
                                        optionalClass: context.RenderMode != RenderMode.Documentation
                                            ? "fixed-aligned-content justify-content-center"
                                            : "")),
                                new When(
                                    "f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation' and (f:code/@value='B' or f:code/@value='D' or f:code/@value='U' or f:code/@value='W')]", // trends are being displayed in a neighboring cell
                                    new NullWidget()),
                            ], new CodeableConcept()),
                        ], new LineBreak()),
                    ],
                    containerClass: "border-end-0"),
                new TableCell([new Container([statusIcon], optionalClass: "fixed-aligned-content")],
                    containerClass: "border-start-0"),
            ];

            return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
        }

        private string? GetNormalityClass(string? value)
        {
            switch (value)
            {
                case null:
                case "N":
                    return null;
                case "A":
                case "H":
                case "L":
                    return "abnormal";
                case "AA":
                case "HH":
                case "LL":
                case "HU":
                case "LU":
                    return "critical";
                default:
                    return null;
            }
        }
    }

    public enum CzLabObservationInfrequentProperties
    {
        [Extension("http://hl7.org/fhir/StructureDefinition/workflow-supportingInfo")]
        SupportingInfoExtension,

        [Extension("http://hl7.org/fhir/5.0/StructureDefinition/extension-Observation.triggeredBy")]
        TriggeredByR5Extension,

        [Extension("http://hl7.eu/fhir/laboratory/StructureDefinition/observation-certifiedRefMaterialCodeable")]
        CertifiedRefMaterialCodeableExtension,

        [Extension("http://hl7.eu/fhir/laboratory/StructureDefinition/observation-certifiedRefMaterialIdentifer")]
        CertifiedRefMaterialIdentiferExtension,

        [Extension("http://hl7.eu/fhir/laboratory/StructureDefinition/observation-deviceLabTestKit")]
        LabTestKitExtension,

        BasedOn,

        PartOf,

        Focus,

        Issued,

        [OpenType("value")] Value,

        Interpretation,

        Note,

        BodySite,

        Method,

        Specimen,

        Device,

        HasMember,

        DerivedFrom,
    }

    private class ShowReferenceRange : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            Widget[] widgetTree =
            [
                new Condition("f:low and not(f:high)", new ConstantText(" ≥ ")),
                new Optional("f:low", new ShowQuantity(showUnit: false)),
                new Condition("f:low and f:high", new ConstantText(" - ")),
                new Condition("not(f:low) and f:high", new ConstantText(" ≤ ")),
                new Optional("f:high", new ShowQuantity(showUnit: false)),
                new Condition("f:low or f:high", new LineBreak()),
                new Optional("f:type",
                    new NameValuePair([new LocalizedLabel("observation.referenceRange.type")],
                        [new CodeableConcept()])),
                new Condition("f:appliesTo",
                    new NameValuePair([new LocalizedLabel("observation.referenceRange.appliesTo")],
                        [new ConcatBuilder("f:appliesTo", _ => [new CodeableConcept()], ", ")])),
                new Optional("f:age",
                    new NameValuePair([new LocalizedLabel("observation.referenceRange.age")], [new ShowRange()])),
                new Optional("f:text",
                    new NameValuePair([new LocalizedLabel("observation.referenceRange.text")], [new Text("@value")])),
            ];

            return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
        }
    }

    private class ShowInterpretationScale : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var value = navigator.SelectSingleNode("f:coding/f:code/@value").Node?.Value;

            if (context.RenderMode == RenderMode.Documentation)
            {
                return new ConstantText(navigator.GetFullPath()).Render(navigator, renderer, context);
            }

            const string template = "•|••|•|••|•";
            var targetIndex = -1;
            var targetClass = string.Empty;
            switch (value)
            {
                case "LL":
                    targetIndex = 0;
                    targetClass = "critical-bullet";
                    break;
                case "LU":
                    targetIndex = 2;
                    targetClass = "abnormal-bullet-significant";
                    break;
                case "L":
                    targetIndex = 3;
                    targetClass = "abnormal-bullet";
                    break;
                case "N":
                    targetIndex = 5;
                    targetClass = "normal-bullet";
                    break;
                case "H":
                    targetIndex = 7;
                    targetClass = "abnormal-bullet";
                    break;
                case "HU":
                    targetIndex = 8;
                    targetClass = "abnormal-bullet-significant";
                    break;
                case "HH":
                    targetIndex = 10;
                    targetClass = "critical-bullet";
                    break;
            }

            if (targetIndex >= template.Length || targetIndex < 0)
            {
                // invalid value, fall back to generic codeable concept handling 
                return new CodeableConcept().Render(navigator, renderer, context);
            }

            var preBulletText = template.Substring(0, targetIndex);
            var postBulletText = template.Substring(targetIndex + 1);
            var preBulletWidget = new Container(new ConstantText(preBulletText), ContainerType.Span);
            var postBulletWidget = new Container(new ConstantText(postBulletText), ContainerType.Span);
            var bulletWidget = new Container([
                    new ConstantText("⬤"),
                ], ContainerType.Span,
                optionalClass: "interpretation-bullet " + targetClass);

            return new Container([preBulletWidget, bulletWidget, postBulletWidget], ContainerType.Span,
                optionalClass: "interpretation-scale h-100").Render(navigator,
                renderer, context);
        }
    }
}