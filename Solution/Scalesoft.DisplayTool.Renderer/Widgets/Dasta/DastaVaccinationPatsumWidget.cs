using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaVaccinationPatsumWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Variable("unknownVaccination",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:oc[@typ='U']"),
            new Variable("vaccinations",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:oc[not(@typ='U')]"),
            new Section("/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:oc[1]",
                null,
                [new ConstantText("Očkování - souhrn")],
                [
                    new Choose(
                    [
                        new When("$unknownVaccination", new ConstantText("Informace o očkování nejsou známé"))
                    ], new Table([
                        new TableRow([
                            new TableCell([new ConstantText("Typ očkování")], TableCellType.Header, rowspan: 2),
                            new TableCell([new ConstantText("Agens, proti kterým očkování působí")],
                                TableCellType.Header, rowspan: 2),
                            new TableCell([new ConstantText("Datum příštího očkování")], TableCellType.Header,
                                rowspan: 2),
                            new TableCell([new ConstantText("Poznámka")], TableCellType.Header, rowspan: 2),
                            new TableCell([new ConstantText("Očkovací dávka")], TableCellType.Header, colspan: 10),
                        ]),
                        new TableRow([
                            new TableCell([new ConstantText("Číslo dávky")], TableCellType.Header),
                            new TableCell([new ConstantText("Název očkovací látky")], TableCellType.Header),
                            new TableCell([new ConstantText("Název výrobce nebo držitele obchodního rozhodnutí")],
                                TableCellType.Header),
                            new TableCell([new ConstantText("Generický název očkovací látky")], TableCellType.Header),
                            new TableCell([new ConstantText("Číslo šarže")], TableCellType.Header),
                            new TableCell([new ConstantText("Aplikační cesta")], TableCellType.Header),
                            new TableCell([new ConstantText("Místo aplikace")], TableCellType.Header),
                            new TableCell([new ConstantText("Poznámky k očkovací dávce")], TableCellType.Header),
                            new TableCell([new ConstantText("Pracoviště")], TableCellType.Header),
                            new TableCell([new ConstantText("Datum a čas provedení očkování")], TableCellType.Header),
                        ]),
                        new ConcatBuilder("$vaccinations/dsip:ocz", i =>
                        [
                            // ignore garant_dat
                            // ignore dat_ab
                            // ignore iid
                            new TableBody([
                                new TableRow([
                                    new TableCell([
                                        new Tooltip([new TextContainer(TextStyle.Regular, [new Text("@typ_oc_text")])],
                                        [
                                            new DastaCodedValue("@typ_oc_kod",
                                                "LOCTO"), // TODO verify DASTA-specific code list #DT-176
                                        ], [])
                                    ]),
                                    // ignore @id_ockovani
                                    // ignore ISIN values (id_ockovani_isin,indikace,indikace_text)
                                    new TableCell([
                                        new ItemListBuilder("dsip:agens", ItemListType.Unordered, i =>
                                        [
                                            new Tooltip(
                                                [new TextContainer(TextStyle.Regular, [new Text("@agens_text")])], [
                                                    new DastaCodedValue("@agens_klic",
                                                        "NCMPAG"), // TODO verify DASTA-specific code list #DT-176
                                                    // TODO handle optional text with required code #DT-175
                                                ], []),
                                        ]),
                                    ]),
                                    // davka is rendered in separate rows
                                    new TableCell([new DastaDate("dsip:dat_po")]),
                                    new TableCell([new Text("dsip:pozn")]),
                                    new TableCell([], colspan: 10), // davka
                                    // ignore iid
                                ]),
                                new ConcatBuilder("dsip:davka", _ =>
                                [
                                    new TableRow([
                                        new TableCell([],
                                            colspan: 4), // vaccination type, vaccination agent, vaccination next date, vaccination note
                                        new TableCell([
                                            new Choose(
                                            [
                                                new When("@poc_davek and @cis_davky", new Text("@cis_davky"),
                                                    new ConstantText("/"), new DastaDate("@poc_davek"))
                                            ], new Text("@cis_davky")),
                                        ]),
                                        // ignore @ind_oprav_sd, according to Hynek Kružík
                                        // ignore id_davka
                                        // ignore id_davka_isin
                                        new TableCell([
                                            new Tooltip([new TextContainer(TextStyle.Regular, [new Text("@nazev_ol")])],
                                            [
                                                new DastaCodedValue("@kod_ol",
                                                    "SUKL"), // TODO verify SUKL code list is valid name #DT-176
                                            ], []),
                                        ]),
                                        new TableCell([new Text("@nazev_vyrobce"),]),
                                        new TableCell([new Text("@gene_ol"),]),
                                        new TableCell([new Text("@cis_sarze"),]),
                                        // ignore dat_expirace
                                        new TableCell([
                                            DastaWidgetUtils.TextOrTooltipWithText(
                                                [new TextContainer(TextStyle.Regular, [new Text("@apl_cesta_text")])], [
                                                    new DastaCodedValue("@apl_cesta_klic",
                                                        "LAPLC"), // TODO verify DASTA-specific code list #DT-176
                                                ], "@apl_cesta_klic"),
                                        ]),
                                        new TableCell([
                                            DastaWidgetUtils.TextOrTooltipWithText(
                                                [new TextContainer(TextStyle.Regular, [new Text("@misto_apl_text")])], [
                                                    new DastaCodedValue("@misto_apl_klic",
                                                        "LAPLM"), // TODO verify DASTA-specific code list #DT-176
                                                ], "@misto_apl_klic"),
                                        ]),
                                        new TableCell([new Text("@pozn"),]),
                                        // ignore autor
                                        new TableCell([
                                            new Choose([
                                                new When("dsip:pracoviste", new Tooltip(
                                                    [
                                                        new TextContainer(TextStyle.Regular,
                                                            [new Text("dsip:pracoviste/dsip:nazev")])
                                                    ],
                                                    [
                                                        NameValuePairIfPresent("Kód pracoviště lokální",
                                                            "dsip:pracoviste/@kod_lok"), // TODO investigate - maybe there is a coded system defined for this #DT-176
                                                        NameValuePairIfPresent("IČO", "dsip:pracoviste/@ico"),
                                                        NameValuePairIfPresent("IČZ", "dsip:pracoviste/@icz"),
                                                        NameValuePairIfPresent("IČP", "dsip:pracoviste/@icp"),
                                                        NameValuePairIfPresent("Odbornost pracoviště",
                                                            "dsip:pracoviste/@odb"),
                                                        // ignore nesmluv_p
                                                        NameValuePairIfPresent("Nákladové středisko",
                                                            "dsip:pracoviste/@ns"),
                                                        NameValuePairIfPresent("Oddělení", "dsip:pracoviste/@oddel"),
                                                        NameValuePairIfPresent("Pořadové číslo zařízení",
                                                            "dsip:pracoviste/@pcz"),
                                                        NameValuePairIfPresent("Poznámky", "dsip:pracoviste/dsip:pozn"),
                                                        new Choose([
                                                            new When("dsip:pracoviste/ds:a", new NameValuePair(
                                                                [new ConstantText("Adresa")],
                                                                [
                                                                    new Choose([
                                                                        new When("dsip:pracoviste/ds:a/ds:jmeno",
                                                                            new Text("dsip:pracoviste/ds:a/ds:jmeno"),
                                                                            new LineBreak())
                                                                    ]),
                                                                    new Choose([
                                                                        new When("dsip:pracoviste/ds:a/ds:adr",
                                                                            new Text("dsip:pracoviste/ds:a/ds:adr"),
                                                                            new LineBreak())
                                                                    ]),
                                                                    new Choose([
                                                                        new When("dsip:pracoviste/ds:a/ds:dop1",
                                                                            new Text("dsip:pracoviste/ds:a/ds:dop1"),
                                                                            new LineBreak())
                                                                    ]),
                                                                    new Choose([
                                                                        new When("dsip:pracoviste/ds:a/ds:dop2",
                                                                            new Text("dsip:pracoviste/ds:a/ds:dop2"),
                                                                            new LineBreak())
                                                                    ]),
                                                                    new Choose([
                                                                        new When("dsip:pracoviste/ds:a/ds:psc",
                                                                            new Text("dsip:pracoviste/ds:a/ds:psc"),
                                                                            new LineBreak())
                                                                    ]),
                                                                    new Choose([
                                                                        new When("dsip:pracoviste/ds:a/ds:mesto",
                                                                            new Text("dsip:pracoviste/ds:a/ds:mesto"),
                                                                            new LineBreak())
                                                                    ]),
                                                                    // ignore stat
                                                                    new Choose([
                                                                        new When("dsip:pracoviste/ds:a/ds:stat_text",
                                                                            new Text(
                                                                                "dsip:pracoviste/ds:a/ds:stat_text"))
                                                                    ]),
                                                                ]))
                                                        ]),
                                                    ], []))
                                            ]),
                                        ]),
                                        new TableCell([new DastaDate("dsip:dat_du"),]),
                                        // ignore reakce
                                    ]),
                                ]),
                            ]),
                        ]),
                    ])),
                ], titleAbbreviations: ("OS", "VS")),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }

    private Widget NameValuePairIfPresent(string label, string select)
    {
        return new Choose([
            new When(select, new NameValuePair([new ConstantText(label)],
            [
                new Text(select)
            ]))
        ]);
    }
}