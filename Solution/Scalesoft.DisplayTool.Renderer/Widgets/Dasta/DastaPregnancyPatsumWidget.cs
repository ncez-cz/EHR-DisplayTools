using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaPregnancyPatsumWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Variable("pregnancy",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:grav[@typ='A' or @typ='G']"),
            new Variable("noPregnancy",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:grav[@typ='N']"),
            new Variable("unknownPregnancy",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:grav[@typ='U']"),
            new Variable("confidentialPregnancy",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:grav[@typ='T']"),
            new Section(
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:grav[1]", null,
                [new ConstantText("Gravidita")],
                [
                    new Choose(
                    [
                        new When("$noPregnancy", new ConstantText("není gravidní")), // no suitable eHDSI labels found
                        new When("$unknownPregnancy",
                            new ConstantText(
                                "neznámá informace (anamnéza není známa, informace nedostupná - tj. pacientka může a nemusí být gravidní)")), // no suitable eHDSI labels found
                        new When("$confidentialPregnancy",
                            new ConstantText("utajená informace")), // no suitable eHDSI labels found
                    ], new Table([
                        new TableCell([new ConstantText("Předpokládaný termín porodu a způsob jeho určení")],
                            TableCellType.Header),
                        new TableCell([new ConstantText("Den početí")], TableCellType.Header),
                        new TableCell([new ConstantText("Doplňující informace a poznámky")], TableCellType.Header),
                        new TableCell([new ConstantText("Datum posledního vyšetření")], TableCellType.Header),
                        new TableRow([
                            new TableCell([
                                new ItemListBuilder("$pregnancy/dsip:grav_tp", ItemListType.Unordered, i =>
                                [
                                    new DastaDate("dsip:dat_por"),
                                    new Choose([
                                        new When("@zutp_klic or @zutp_text", new ConstantText(" - "),
                                            DastaWidgetUtils.TextOrTooltipWithText(
                                                [new TextContainer(TextStyle.Regular, [new Text("@zutp_text")])], [
                                                    new DastaCodedValue("@zutp_klic",
                                                        "1.3.6.1.4.1.12559.11.10.1.3.1.42.9"),
                                                ], "@zutp_klic"))
                                    ]),
                                ]),
                            ]),
                            new TableCell([
                                new DastaDate("$pregnancy/dsip:dat_poc"),
                            ]),
                            new TableCell([
                                new Text("$pregnancy/@info_text"),
                            ]),
                            // ignore autor
                            new TableCell([
                                new DastaDate("$pregnancy/dsip:dat_vys"),
                            ]),
                            // ignore @dat_ab
                            // ignore iid
                            // ignore @ind_oprav_sd 
                        ]),
                    ])),
                ], titleAbbreviations: ("T", "P")),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}