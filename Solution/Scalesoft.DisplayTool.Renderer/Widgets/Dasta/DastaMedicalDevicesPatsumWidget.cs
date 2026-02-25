using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaMedicalDevicesPatsumWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Variable("medDevicesPlaintext",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:uniz[@typ='SN']"),
            new Variable("medDevicesCoded",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:uniz[@typ='SF']"),
            new Variable("noMedDevices",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:uniz[@typ='N']"),
            new Variable("unknownMedDevices",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:uniz[@typ='U']"),
            new Section(
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:uniz[1]",
                null,
                [new ConstantText("Náhrady, implantáty, zařízení")],
                [
                    new Choose(
                    [
                        new When("$noMedDevices",
                            new ConstantText("Nejsou známé žádné implantáty, náhrady či zařízení")),
                        new When("$unknownMedDevices", new ConstantText("Není známo"))
                    ], new Collapser([new ConstantText("Volným textem")], [
                        new Table([
                            new TableCell([new ConstantText("Náhrada, implantát, zařízení")], TableCellType.Header),
                            new TableCell([new ConstantText("Důvod použití")], TableCellType.Header),
                            new TableCell([new ConstantText("Období")], TableCellType.Header),
                            new ConcatBuilder("$medDevicesPlaintext", i =>
                            [
                                new TableRow([
                                    new TableCell([new Text("dsip:u_niz")]),
                                    new TableCell([
                                        DastaWidgetUtils.TextOrTooltipWithText(
                                            [new TextContainer(TextStyle.Regular, [new Text("dsip:duvod_text")])], [
                                                new ItemListBuilder("dsip:duvod_kod", ItemListType.Unordered, _ =>
                                                [
                                                    new DastaMknOrphaCodedValue("."),
                                                    // TODO handle optional text with optional code #DT-175
                                                ]),
                                            ], "dsip:duvod_kod"),
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
                                    // ignore autor
                                    // ignore dat_ab
                                    // ignore iid
                                    // ignore @ind_oprav_sd, according to Hynek Kružík
                                ])
                            ]),
                        ]),
                    ]), new LineBreak(), new Collapser([new ConstantText("Formalizovaně")], [
                        new Table([
                            new TableCell([new ConstantText("Náhrada, implantát, zařízení")], TableCellType.Header),
                            new TableCell([new ConstantText("Důvod použití")], TableCellType.Header),
                            new TableCell([new ConstantText("Identifikátor prostředku")], TableCellType.Header),
                            new TableCell([new ConstantText("Období")], TableCellType.Header),
                            new TableCell([new ConstantText("Doplňující informace a poznámky")], TableCellType.Header),
                            new ConcatBuilder("$medDevicesCoded", i =>
                            [
                                new TableRow([
                                    new TableCell([
                                        new Tooltip(
                                            [new TextContainer(TextStyle.Regular, [new Text("dsip:unizf/@niz_text")])],
                                            [
                                                new DastaCodedValue("dsip:unizf/@niz_klic",
                                                    "1.3.6.1.4.1.12559.11.10.1.3.1.42.8"),
                                                // TODO handle optional text with required code #DT-175
                                            ], []),
                                    ]),
                                    new TableCell([
                                        DastaWidgetUtils.TextOrTooltipWithText(
                                            [new TextContainer(TextStyle.Regular, [new Text("dsip:duvod_text")])], [
                                                new ItemListBuilder("dsip:duvod_kod", ItemListType.Unordered, _ =>
                                                [
                                                    new DastaMknOrphaCodedValue("."),
                                                ]),
                                            ], "dsip:duvod_kod"),
                                    ]),
                                    new TableCell([new Text("dsip:unizf/@niz_ident")]),
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
                                    new TableCell([new Text("dsip:unizf/@info_text")]),
                                    // ignore autor
                                    // ignore dat_ab
                                    // ignore iid
                                    // ignore @ind_oprav_sd, according to Hynek Kružík
                                ])
                            ]),
                        ]),
                    ])),
                ], titleAbbreviations: ("IZ", "MD")),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}