using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Encounter;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class VisionPrescription : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var widgetTree = new List<Widget>();

        // ignore id, meta, implicitRules, language, extension, modifierExtension
        // ignore identifier, status
        // ignore create - resource creation time?
        // ignore patient
        // ignore prescriber

        var lensPrescription = navigator
            .SelectAllNodes("f:lensSpecification[f:product/f:coding/f:code/@value = 'lens']").ToList();
        var contactLensPrescription = navigator
            .SelectAllNodes("f:lensSpecification[f:product/f:coding/f:code/@value = 'contact']").ToList();
        var otherPrescription = navigator
            .SelectAllNodes(
                "f:lensSpecification[not(f:product/f:coding/f:code/@value = 'lens' or f:product/f:coding/f:code/@value = 'contact')]")
            .ToList();

        if (lensPrescription.Count != 0)
        {
            var lensTableHeader = BuildLensPrescriptionTableHeader(lensPrescription);
            var lensTable = new Table(
                [
                    ..lensTableHeader,
                    ..BuildVisionPrescriptionBody(lensPrescription, VisionPrescriptionMode.Lens),
                ],
                true
            );
            widgetTree.Add(lensTable);
        }

        if (contactLensPrescription.Count != 0)
        {
            var contactLensTableHeader = BuildContactLensPrescriptionTableHeader(contactLensPrescription);
            var contactLensTable = new Table(
                [
                    ..contactLensTableHeader,
                    ..BuildVisionPrescriptionBody(contactLensPrescription, VisionPrescriptionMode.ContactLens),
                ],
                true
            );
            widgetTree.Add(contactLensTable);
        }

        if (otherPrescription.Count != 0)
        {
            var genericVisionPrescriptionTableHeader = BuildUnknownVisionPrescriptionTableHeader(otherPrescription);
            var genericLensTable = new Table(
                [
                    ..genericVisionPrescriptionTableHeader,
                    ..BuildVisionPrescriptionBody(otherPrescription, VisionPrescriptionMode.Generic),
                ],
                true
            );
            widgetTree.Add(genericLensTable);
        }

        widgetTree.Add(new NameValuePair([new LocalizedLabel("vision-prescription.dateWritten")],
            [new ShowDateTime("f:dateWritten")]));
        widgetTree.Add(new NameValuePair([new EhdsiDisplayLabel(LabelCodes.Status)],
            [new EnumLabel("f:status", "http://hl7.org/fhir/ValueSet/request-status")]));

        widgetTree.Add(new Optional("f:encounter",
            new ShowMultiReference(".",
                (items, _) => items.Select(Widget (x) => new EncounterCard(x)).ToList(),
                x =>
                [
                    new Collapser([new LocalizedLabel("node-names.Encounter")], x.ToList(),
                        isCollapsed: true),
                ]
            )
        ));
        widgetTree.Add(
            new Condition("f:text",
                new NarrativeCollapser()
            )
        );

        var container = new Container(widgetTree);

        return container.Render(navigator, renderer, context);
    }

    private Widget[] BuildLensPrescriptionTableHeader(IList<XmlDocumentNavigator> navs)
    {
        List<TableCell> prescriptionHeaderFirstRow =
        [
            new([new ConstantText(string.Empty)], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.sphere")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.axis")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.cylinder")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.prism")], TableCellType.Header, 2, 1),
            new([new LocalizedLabel("vision-prescription.add")], TableCellType.Header, 1, 2),
        ];
        ConditionallyAddInfrequentColumnNames(navs, prescriptionHeaderFirstRow);
        Widget[] tableHeader =
        [
            new TableCaption([new LocalizedLabel("vision-prescription.lens-prescription")]),
            new TableHead([
                new TableRow([..prescriptionHeaderFirstRow]),
                new TableRow([
                    new TableCell([new LocalizedLabel("vision-prescription.prism.amount")], TableCellType.Header),
                    new TableCell([new LocalizedLabel("vision-prescription.prism.base")], TableCellType.Header),
                ]),
            ]),
        ];

        return tableHeader;
    }

    private Widget[] BuildContactLensPrescriptionTableHeader(IList<XmlDocumentNavigator> navs)
    {
        List<TableCell> prescriptionHeaderFirstRow =
        [
            new([new ConstantText(string.Empty)], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.sphere")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.backCurve")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.power")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.diameter")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.axis")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.cylinder")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.prism")], TableCellType.Header, 2, 1),
            new([new LocalizedLabel("vision-prescription.add")], TableCellType.Header, 1, 2),
        ];
        ConditionallyAddInfrequentColumnNames(navs, prescriptionHeaderFirstRow);
        Widget[] tableHeader =
        [
            new TableCaption([new LocalizedLabel("vision-prescription.contact-lense-prescription")]),
            new TableHead([
                new TableRow([..prescriptionHeaderFirstRow]),
                new TableRow([
                    new TableCell([new LocalizedLabel("vision-prescription.prism.amount")], TableCellType.Header),
                    new TableCell([new LocalizedLabel("vision-prescription.prism.base")], TableCellType.Header),
                ]),
            ]),
        ];

        return tableHeader;
    }

    private Widget[] BuildUnknownVisionPrescriptionTableHeader(IList<XmlDocumentNavigator> navs)
    {
        List<TableCell> prescriptionHeaderFirstRow =
        [
            new([new ConstantText(string.Empty)], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.sphere")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.backCurve")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.power")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.diameter")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.axis")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.cylinder")], TableCellType.Header, 1, 2),
            new([new LocalizedLabel("vision-prescription.prism")], TableCellType.Header, 2, 1),
            new([new LocalizedLabel("vision-prescription.add")], TableCellType.Header, 1, 2),
        ];
        ConditionallyAddInfrequentColumnNames(navs, prescriptionHeaderFirstRow);
        Widget[] tableHeader =
        [
            new TableCaption([new LocalizedLabel("vision-prescription.generic-lense")]),
            new TableHead([
                new TableRow([..prescriptionHeaderFirstRow]),
                new TableRow([
                    new TableCell([new LocalizedLabel("vision-prescription.prism.amount")], TableCellType.Header),
                    new TableCell([new LocalizedLabel("vision-prescription.prism.base")], TableCellType.Header),
                ]),
            ]),
        ];

        return tableHeader;
    }

    private void ConditionallyAddInfrequentColumnNames(
        IList<XmlDocumentNavigator> navs,
        IList<TableCell> headerRowCells
    )
    {
        var anyPrescriptionHasDuration = navs.Any(x => x.EvaluateCondition("f:duration"));
        var anyPrescriptionHasColor = navs.Any(x => x.EvaluateCondition("f:color"));
        var anyPrescriptionHasBrand = navs.Any(x => x.EvaluateCondition("f:brand"));
        var anyPrescriptionHasNote = navs.Any(x => x.EvaluateCondition("f:note"));

        if (anyPrescriptionHasDuration)
        {
            headerRowCells.Add(new TableCell([new LocalizedLabel("vision-prescription.duration")], TableCellType.Header,
                1, 2));
        }

        if (anyPrescriptionHasColor)
        {
            headerRowCells.Add(new TableCell([new LocalizedLabel("vision-prescription.color")], TableCellType.Header, 1,
                2));
        }

        if (anyPrescriptionHasBrand)
        {
            headerRowCells.Add(new TableCell([new LocalizedLabel("vision-prescription.brand")], TableCellType.Header, 1,
                2));
        }

        if (anyPrescriptionHasNote)
        {
            headerRowCells.Add(new TableCell([new LocalizedLabel("vision-prescription.note")], TableCellType.Header, 1,
                2));
        }
    }

    private List<Widget> BuildVisionPrescriptionBody(IList<XmlDocumentNavigator> navs, VisionPrescriptionMode mode)
    {
        var prescriptionBodies = new List<Widget>();
        foreach (var nav in navs)
        {
            prescriptionBodies.Add(new LensPrescriptionRow(nav, mode));
        }

        return prescriptionBodies;
    }

    private class LensPrescriptionRow(XmlDocumentNavigator item, VisionPrescriptionMode mode) : Widget
    {
        public override Task<RenderResult> Render(
            XmlDocumentNavigator navigator,
            IWidgetRenderer renderer,
            RenderContext context
        )
        {
            var lensRows = new List<Widget>();
            var lensPrismRows = new List<Widget>();
            var prismNavs = item.SelectAllNodes("f:prism").ToList();
            var nonPrismRowSpan = Math.Max(prismNavs.Count, 1);
            var cells = new List<Widget>
            {
                new TableCell([
                    new EnumLabel("f:eye", "http://hl7.org/fhir/ValueSet/vision-eye-codes"),
                ], rowspan: nonPrismRowSpan),
                new TableCell([
                    new ShowDecimal("f:sphere"),
                ], rowspan: nonPrismRowSpan),
            };
            if (mode is VisionPrescriptionMode.ContactLens or VisionPrescriptionMode.Generic)
            {
                cells.Add(new TableCell([
                    new ShowDecimal("f:backCurve"),
                ], rowspan: nonPrismRowSpan));
                cells.Add(new TableCell([
                    new ShowDecimal("f:power"),
                ], rowspan: nonPrismRowSpan));
                cells.Add(new TableCell([
                    new ShowDecimal("f:diameter"),
                ], rowspan: nonPrismRowSpan));
            }

            cells.Add(new TableCell([
                new Text("f:axis/@value"),
            ], rowspan: nonPrismRowSpan));
            cells.Add(new TableCell([
                new ShowDecimal("f:cylinder"),
            ], rowspan: nonPrismRowSpan));
            if (prismNavs.Count == 0)
            {
                cells.Add(new TableCell([
                    new ConstantText(string.Empty),
                ]));
                cells.Add(new TableCell([
                    new ConstantText(string.Empty),
                ]));
            }
            else
            {
                for (var index = 0; index < prismNavs.Count; index++)
                {
                    var prismNav = prismNavs[index];
                    Widget[] prismCells =
                    [
                        new ChangeContext(prismNav, new TableCell([
                            new ShowDecimal("f:amount"),
                        ]), new TableCell([
                            new EnumLabel("f:base", "http://hl7.org/fhir/ValueSet/vision-base-codes"),
                        ])),
                    ];
                    if (index == 0)
                    {
                        cells.AddRange(prismCells);
                    }
                    else
                    {
                        lensPrismRows.Add(new TableRow(prismCells));
                    }
                }
            }

            cells.Add(new TableCell([
                new ShowDecimal("f:add"),
            ], rowspan: nonPrismRowSpan));
            if (item.EvaluateCondition("f:duration"))
            {
                cells.Add(new TableCell([
                    new ShowQuantity("f:duration"),
                ], rowspan: nonPrismRowSpan));
            }

            if (item.EvaluateCondition("f:color"))
            {
                cells.Add(new TableCell([
                    new Text("f:color/@value"),
                ], rowspan: nonPrismRowSpan));
            }

            if (item.EvaluateCondition("f:brand"))
            {
                cells.Add(new TableCell([
                    new Text("f:brand/@value"),
                ], rowspan: nonPrismRowSpan));
            }

            if (item.EvaluateCondition("f:note"))
            {
                cells.Add(new TableCell([
                    new ChangeContext("f:note", new ShowAnnotationCompact()),
                ], rowspan: nonPrismRowSpan));
            }

            lensRows.Add(new TableRow([..cells]));
            lensRows.AddRange(lensPrismRows);
            var prescriptionBody = new TableBody(lensRows);

            return prescriptionBody.Render(item, renderer, context);
        }
    }

    private enum VisionPrescriptionMode
    {
        Lens,
        ContactLens,
        Generic,
    }
}