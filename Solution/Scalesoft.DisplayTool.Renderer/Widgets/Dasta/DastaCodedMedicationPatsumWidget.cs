using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaCodedMedicationPatsumWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Variable("medicationCoded",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:le[@typ='A']"),
            new Variable("noOrUnknownMedication",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:le[@typ='N' or @typ='U' or @typ='M']"),
            new Section("/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:le[1]",
                null,
                [new ConstantText("Podávané léky")],
                [
                    new Choose(
                    [
                        new When("$noOrUnknownMedication",
                            new DastaMockCodedValue("$noOrUnknownMedication/@typ",
                                "https://www.dastacr.cz/dasta/hypertext/DSABD.htm#typ_sezn")), // no suitable eHDSI labels found
                    ], new Table([
                        new TableCell([new ConstantText("Typ")], TableCellType.Header),
                        new TableCell([new ConstantText("Název léku")], TableCellType.Header),
                        new TableCell([new ConstantText("Důvod medikace")], TableCellType.Header),
                        new TableCell([new ConstantText("Důvod změny medikace")], TableCellType.Header),
                        new TableCell([new ConstantText("Generický název léku")], TableCellType.Header),
                        new TableCell([new ConstantText("ATC skupina léku")], TableCellType.Header),
                        new TableCell([new ConstantText("Aktivní látka")], TableCellType.Header),
                        new TableCell([new ConstantText("Aplikační cesta")], TableCellType.Header),
                        new TableCell([new ConstantText("Místo aplikace")], TableCellType.Header),
                        new TableCell([new ConstantText("Forma léku")], TableCellType.Header),
                        new TableCell([new ConstantText("Balení léku")], TableCellType.Header),
                        new TableCell([new ConstantText("Síla léku")], TableCellType.Header),
                        new TableCell([new ConstantText("Velikost balení")], TableCellType.Header),
                        new TableCell([new ConstantText("Datum podávání léku")], TableCellType.Header),
                        new TableCell([new ConstantText("Dávkování")], TableCellType.Header),
                        new TableCell([new ConstantText("Rozpis magistraliter")], TableCellType.Header),
                        new TableCell([new ConstantText("Poznámka k výdeji léku")], TableCellType.Header),
                        new TableCell([new ConstantText("Instrukce pro pacienta")], TableCellType.Header),
                        new TableCell([new ConstantText("Délka vybavení [den]")], TableCellType.Header),
                        new ConcatBuilder("$medicationCoded/dsip:lez", i =>
                        [
                            new TableRow([
                                new TableCell([
                                    new DastaMockCodedValue("@typ_med",
                                        "https://www.dastacr.cz/dasta/hypertext/DSABE.htm#typ_med_sezn")
                                ]),
                                // ignore @ind_oprav_sd
                                new TableCell([new Text("@nazev_lek")]),
                                new TableCell([
                                    DastaWidgetUtils.TextOrTooltipWithText([
                                        new TextContainer(TextStyle.Regular, [
                                            new Text("dsip:ind_m_text")
                                        ])
                                    ], [
                                        new ItemListBuilder("dsip:ind_m_kod", ItemListType.Unordered, i =>
                                        [
                                            new DastaMknOrphaCodedValue("."),
                                        ]),
                                    ], "dsip:ind_m_kod"),
                                ]),
                                new TableCell([
                                    DastaWidgetUtils.TextOrTooltipWithText(
                                        [new TextContainer(TextStyle.Regular, [new Text("dsip:zmena_m_text")])], [
                                            new DastaMockCodedValue("dsip:zmena_m_kod",
                                                "https://www.dastacr.cz/dasta/hypertext/DSABE.htm#zmena_m_kod_sezn"),
                                        ], "dsip:zmena_m_kod"),
                                ]),
                                new TableCell([
                                    DastaWidgetUtils.TextOrTooltipWithText(
                                        [new TextContainer(TextStyle.Regular, [new Text("@gene_lek")])], [
                                            new DastaCodedValue("@kod_lek", null, codeSystemSelect: "@cis_kod_lek"),
                                            // ignore @cis_kod_lek_v TODO maybe use @cis_kod_lek_v #DT-175
                                        ], "@kod_lek"),
                                ]),
                                new TableCell([new Text("@kod_atc")]),
                                // ignore kod_atb
                                new TableCell([
                                    new ItemListBuilder("dsip:aktivni_latka", ItemListType.Unordered, i =>
                                    [
                                        DastaWidgetUtils.TextOrTooltipWithText(
                                            [new TextContainer(TextStyle.Regular, [new Text("@nazev")])], [
                                                new Text("@kod_atc"),
                                                // TODo maybe use coded value widget #DT-175
                                            ], "@kod_atc"),
                                    ]),
                                ]),
                                new TableCell([
                                    DastaWidgetUtils.TextOrTooltipWithText(
                                        [new TextContainer(TextStyle.Regular, [new Text("@apl_cesta_text")])], [
                                            new DastaCodedValue("@apl_cesta_klic", "LAPLC"),
                                        ], "@apl_cesta_klic"),
                                ]),
                                new TableCell([
                                    DastaWidgetUtils.TextOrTooltipWithText(
                                        [new TextContainer(TextStyle.Regular, [new Text("@misto_apl_text")])], [
                                            new DastaCodedValue("@misto_apl_klic", "LAPLM"),
                                        ], "@misto_apl_klic"),
                                ]),
                                new TableCell([
                                    DastaWidgetUtils.TextOrTooltipWithText(
                                        [new TextContainer(TextStyle.Regular, [new Text("@forma_text")])], [
                                            new DastaCodedValue("@forma_klic", "1.3.6.1.4.1.12559.11.10.1.3.1.42.2"),
                                        ], "@forma_klic"),
                                ]),
                                new TableCell([
                                    DastaWidgetUtils.TextOrTooltipWithText(
                                    [
                                        new TextContainer(TextStyle.Regular,
                                            [new Text("dsip:lez_obal_leku/@obal_text")])
                                    ], [
                                        new DastaCodedValue("dsip:lez_obal_leku/@obal_klic",
                                            "1.3.6.1.4.1.12559.11.10.1.3.1.42.3"),
                                    ], "dsip:lez_obal_leku/@obal_klic"),
                                ]),
                                new TableCell([new Text("@sila_leku")]),
                                new TableCell([new Text("@velikost_baleni")]),
                                new TableCell([
                                    new Choose(
                                    [
                                        new When("dsip:dat_od", new ConstantText("od "), new DastaDate("dsip:dat_od"))
                                    ]),
                                    new Choose([
                                        new When("dsip:dat_do", new ConstantText(" do "), new DastaDate("dsip:dat_do"))
                                    ]),
                                ]),
                                new TableCell([new Text("dsip:rozpis_v")]),
                                new TableCell([new Text("dsip:magistraliter")]),
                                new TableCell([new Text("dsip:pozn")]),
                                new TableCell([new Text("dsip:instrukce_pac")]),
                                new TableCell([new Text("dsip:delka_vyb")]),
                                // ignore autor
                                // ignore dat_vb
                                // ignore iid
                            ]),
                        ]),
                    ])),
                ], titleAbbreviations: ("PL", "AM")),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}