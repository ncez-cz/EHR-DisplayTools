using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaRiskFactorsPatsumWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Variable("riskFactorsPlaintext",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:urf[@typ='RN' or @typ='RS']"), // RS is obsolete, but try to parse it anyway
            new Variable("riskFactorsCoded",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:urf[@typ='RF']"), // unused, code lists are not implemented in dasta
            new Variable("noRiskFactors",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:urf[@typ='N']"),
            new Variable("unknownRiskFactors",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:urf[@typ='U']"),
            new Section(
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:urf[1]",
                null,
                [new ConstantText("Rizikové faktory")],
                [
                    new Choose(
                    [
                        new When("$noRiskFactors", new ConstantText("Nejsou známé žádné rizikové faktory")),
                        new When("$unknownRiskFactors", new ConstantText("Není známo"))
                    ], new Collapser([new ConstantText("Volným textem")], [
                        new Table([
                            new TableCell([new ConstantText("Rizikový faktor")], TableCellType.Header),
                            new TableCell([new ConstantText("Období")], TableCellType.Header),
                            new ConcatBuilder("$riskFactorsPlaintext", i =>
                            [
                                new TableRow([
                                    new TableCell([new Text("dsip:u_rf")]),
                                    // ignore autor
                                    new TableCell([
                                        new Choose(
                                        [
                                            new When("dsip:dat_du", new ConstantText("od "),
                                                new DastaDate("dsip:dat_du"))
                                        ]),
                                        new Choose([
                                            new When("dsip:dat_up", new ConstantText(" do "),
                                                new DastaDate("dsip:dat_up"))
                                        ]),
                                    ]),
                                    // ignore dat_ab
                                    // ignore iid
                                    // ignore @ind_oprav_sd, according to Hynek Kružík
                                ])
                            ]),
                        ]),
                    ]), new LineBreak(), new Collapser([new ConstantText("Formalizovaně")], [
                        new Table([
                            new TableCell([new ConstantText("Rizikový faktor")], TableCellType.Header),
                            new TableCell([new ConstantText("Období")], TableCellType.Header),
                            new TableCell([new ConstantText("Stupeň závažnosti faktoru")], TableCellType.Header),
                            new TableCell([new ConstantText("Doplňující informace a poznámky")], TableCellType.Header),
                            new ConcatBuilder("$riskFactorsCoded", i =>
                            [
                                new TableRow([
                                    new TableCell([
                                        new Tooltip(
                                            [new TextContainer(TextStyle.Regular, [new Text("dsip:urff/@rf_text")])], [
                                                new DastaCodedValue("dsip:urff/@rf_klic",
                                                    null), // ERIZFAKT code list is currently empty
                                                // TODO handle optional text with required code #DT-175
                                            ], []),
                                    ]),
                                    new TableCell([
                                        new Choose(
                                        [
                                            new When("dsip:dat_du", new ConstantText("od "),
                                                new DastaDate("dsip:dat_du"))
                                        ]),
                                        new Choose([
                                            new When("dsip:dat_up", new ConstantText(" do "),
                                                new DastaDate("dsip:dat_up"))
                                        ]),
                                    ]),
                                    new TableCell([
                                        DastaWidgetUtils.TextOrTooltipWithText(
                                            [new TextContainer(TextStyle.Regular, [new Text("dsip:urff/@szrf_text")])],
                                            [
                                                new DastaCodedValue("dsip:urff/@szrf_klic",
                                                    "1.3.6.1.4.1.12559.11.10.1.3.1.42.13"),
                                                // TODO handle optional text with optional code #DT-175
                                            ], "dsip:urff/@szrf_klic"),
                                    ]),
                                    new TableCell([new Text("dsip:urff/@info_text")]),
                                    // ignore autor
                                    // ignore dat_ab
                                    // ignore iid
                                ])
                            ]),
                        ]),
                    ])),
                ], titleAbbreviations: ("RF", "RF")),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}