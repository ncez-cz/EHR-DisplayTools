using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaSurgeriesPatsumWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Variable("surgeriesCoded",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:chv[@typ='A']"),
            new Variable("noSurgeries",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:chv[@typ='N']"),
            new Variable("unknownSurgeries",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:chv[@typ='U']"),
            new Section(
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:chv[1]", null,
                [new ConstantText("Chirurgické a jiné významné výkony")],
                [
                    new Choose(
                    [
                        new When("$noSurgeries", new ConstantText("Nebyly dosud žádné chirurgické či jiné výkony")),
                        new When("$unknownSurgeries", new ConstantText(
                            "Neznámá informace (anamnéza není známa, informace nedostupná - tj. mohou i nemusí být chirurgické výkony)"))
                    ], new Table([
                        new TableCell([new ConstantText("Chirurgický či jiný významný výkon")], TableCellType.Header),
                        new TableCell([new ConstantText("Anatomická lokalizace výkonu - volný text")],
                            TableCellType.Header),
                        new TableCell([new ConstantText("Anatomická lokalizace výkonu - formalizovaně")],
                            TableCellType.Header),
                        new TableCell([new ConstantText("Doplňující informace a poznámky k výkonu")],
                            TableCellType.Header),
                        new TableCell([new ConstantText("Důvod provedení výkonu")], TableCellType.Header),
                        new TableCell([new ConstantText("Výsledek výkonu")], TableCellType.Header),
                        new TableCell([new ConstantText("Komplikace výkonu")], TableCellType.Header),
                        new TableCell([new ConstantText("Zdravotnický prostředek")], TableCellType.Header),
                        new TableCell([new ConstantText("Datum výkonu")], TableCellType.Header),
                        new ConcatBuilder("$surgeriesCoded", i =>
                        [
                            new TableRow([
                                new TableCell([
                                    DastaWidgetUtils.TextOrTooltipWithText(
                                        [new TextContainer(TextStyle.Regular, [new Text("@chvps_text")])], [
                                            new DastaCodedValue("@chvps_klic", "1.3.6.1.4.1.12559.11.10.1.3.1.42.10"),
                                            // TODO handle optional text with optional code #DT-175
                                        ], "@chvps_klic"),
                                ]),
                                new TableCell([new Text("dsip:alv_text")]),
                                new TableCell([new DastaText("dsip:alv_form")]),
                                new TableCell([new Text("@info_text")]),
                                new TableCell([
                                    DastaWidgetUtils.TextOrTooltipWithText(
                                        [new TextContainer(TextStyle.Regular, [new Text("dsip:duvod_text")])], [
                                            new ItemListBuilder("dsip:duvod_kod", ItemListType.Unordered, i =>
                                            [
                                                new DastaMknOrphaCodedValue("."),
                                            ]),
                                            // TODO handle optional text with optional code #DT-175
                                        ], "dsip:duvod_kod"),
                                ]),
                                new TableCell([
                                    new DastaCodedValue("dsip:vysledek", null)
                                ]), // TODO code system is not defined, additionally enum does not have an anchor in documentation #DT-176
                                new TableCell([
                                    new ItemListBuilder("dsip:komplikace", ItemListType.Unordered, i =>
                                    [
                                        new DastaMknOrphaCodedValue("."),
                                    ]),
                                ]),
                                new TableCell([
                                    new ItemListBuilder("dsip:zdrav_prost", ItemListType.Unordered, i =>
                                    [
                                        new Text(),
                                    ]),
                                ]),
                                // ignore autor
                                new TableCell([new DastaDate("dsip:dat_vyk")]),
                                // ignore dat_ab
                                // ignore iid
                                // ignore @ind_oprav_sd
                            ]),
                        ]),
                    ])),
                ], titleAbbreviations: ("CH", "S")),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}