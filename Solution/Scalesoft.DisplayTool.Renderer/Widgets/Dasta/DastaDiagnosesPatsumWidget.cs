using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaDiagnosesPatsumWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Variable("diagnoses",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:dg/dsip:dgz"),
            new Section(
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:dg/dsip:dgz[1]",
                null,
                [new ConstantText("Diagnózy")],
                [
                    new Table([
                        new TableCell([new ConstantText("Kód")], TableCellType.Header),
                        new TableCell([new ConstantText("Pořadí")], TableCellType.Header),
                        new TableCell([new ConstantText("Stav")], TableCellType.Header),
                        new TableCell([new ConstantText("Závažnost")], TableCellType.Header),
                        new TableCell([new ConstantText("Datum zjištění diagnózy")], TableCellType.Header),
                        new TableCell([new ConstantText("Datum ukončení platnosti diagnózy")], TableCellType.Header),
                        new TableCell([new ConstantText("Specifikace diagnózy")], TableCellType.Header),
                        new TableCell([new ConstantText("Poznámka k diagnóze")], TableCellType.Header),
                        new ConcatBuilder("$diagnoses", i =>
                        [
                            new TableRow([
                                // ignore typ_dg
                                // ignore @ind_oprav_sd
                                new TableCell([
                                    DastaWidgetUtils.TextOrTooltipWithText(
                                        [new TextContainer(TextStyle.Regular, [new Text("dsip:diag")])], [
                                            new DastaCodedValue("dsip:diag/@nazev",
                                                "MKN10"), // TODO verify DASTA-specific code list, verify code list is valid #DT-176
                                        ], "dsip:diag/@nazev"),
                                ]),
                                // ignore diag/@mkn_verze
                                new TableCell([
                                    new Text("dsip:diag/@poradi"),
                                ]),
                                new TableCell([
                                    new ItemListBuilder("dsip:stav_dg", ItemListType.Unordered, i =>
                                    [
                                        new DastaCodedValue(".", "1.3.6.1.4.1.12559.11.10.1.3.1.42.15"),
                                    ]),
                                ]),
                                new TableCell([
                                    new DastaCodedValue("dsip:zavaz_dg", "1.3.6.1.4.1.12559.11.10.1.3.1.42.13"),
                                ]),
                                // ignore dzs_dg - unimplemented is dasta
                                // ignore duvernost, duverne - assume documents that are being displayed are prefiltered by provider
                                new TableCell([new DastaDate("dsip:dat_du"),]),
                                new TableCell([new DastaDate("dsip:dat_up"),]),
                                new TableCell([new DastaDate("dsip:spec_dg"),]),
                                new TableCell([new DastaDate("dsip:pozn"),]),
                                // ignore autor
                                // ignore dat_vb
                                // ignore iid
                            ])
                        ]),
                    ]),
                ], titleAbbreviations: ("D", "D")),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}