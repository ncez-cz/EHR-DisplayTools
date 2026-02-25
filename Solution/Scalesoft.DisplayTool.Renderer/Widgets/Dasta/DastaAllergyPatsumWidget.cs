using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaAllergyPatsumWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Variable("allergiesPlaintext",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:ua[@typ='AN' or @typ='AS']"), // AS type is probihited from DASTA v4.25.01, but try to parse it anyway
            new Variable("allergiesCoded",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:ua[@typ='AF']"),
            new Variable("noAllergies",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:ua[@typ='N']"),
            new Variable("unknownAllergies",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:ua[@typ='U']"),
            new Section(
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:ua[1]",
                null,
                [new ConstantText("Alergie")],
                [
                    new Choose(
                    [
                        new When("$noAllergies",
                            new EhdsiDisplayLabel(LabelCodes.TheSubjectHasNoKnownAllergyConditions)),
                        new When("$unknownAllergies", new EhdsiDisplayLabel(LabelCodes.AllergyNoInformationAvailable))
                    ], new Collapser([new ConstantText("Volným textem")], [
                        new Table([
                            new TableCell([new ConstantText("Alergie")], TableCellType.Header),
                            new TableCell([new ConstantText("Stav")], TableCellType.Header),
                            new TableCell([new ConstantText("Období")], TableCellType.Header),
                            new TableCell([new ConstantText("Míra jistoty")], TableCellType.Header),
                            new ConcatBuilder("$allergiesPlaintext", i =>
                            [
                                new TableRow([
                                    new TableCell([new Text("dsip:u_al")]),
                                    new TableCell([
                                        new DastaMockCodedValue("dsip:stav",
                                            "https://www.dastacr.cz/dasta/hypertext/DSAAR.htm#stav_sezn")
                                    ]),
                                    DisplayPeriod(),
                                    new TableCell([new Text("dsip:jistota_text")]),
                                    // jistota_kod is unused - not implemented in dasta itself
                                    // ignore autor
                                    // ignore dat_ab
                                    // ignore iid
                                    // ignore @ind_oprav_sd, according to Hynek Kružík
                                ])
                            ]),
                        ]),
                    ]), new LineBreak(), new Collapser([new ConstantText("Formalizovaně")], [
                        new Table([
                            new TableCell([new ConstantText("Alergen")], TableCellType.Header),
                            new TableCell([new ConstantText("Typ agens")], TableCellType.Header),
                            new TableCell([new ConstantText("Typ nežádoucí reakce")], TableCellType.Header),
                            new TableCell([new ConstantText("Alergen - další neformalizované informace")],
                                TableCellType.Header),
                            new TableCell([new ConstantText("Stav")], TableCellType.Header),
                            new TableCell([new ConstantText("Období")], TableCellType.Header),
                            new TableCell([new ConstantText("Popis alergické reakce")], TableCellType.Header),
                            new TableCell([new ConstantText("Stupeň závažnosti projevu")], TableCellType.Header),
                            new TableCell([new ConstantText("Míra jistoty")], TableCellType.Header),
                            new TableCell([new ConstantText("Doplňující informace a poznámky")], TableCellType.Header),
                            new ConcatBuilder("$allergiesCoded", i =>
                            [
                                new TableRow([
                                    new TableCell([
                                        new Choose([
                                            new When(
                                                "dsip:uaf/dsip:alerg_text or dsip:uaf/dsip:alerg_lek_klic or dsip:uaf/dsip:alerg_nelek_klic",
                                                new Tooltip(
                                                [
                                                    new TextContainer(TextStyle.Regular,
                                                        [new Text("dsip:uaf/dsip:alerg_text")])
                                                ], [
                                                    new Choose(
                                                    [
                                                        new When("dsip:uaf/dsip:alerg_lek_klic", new DastaCodedValue(
                                                            "dsip:uaf/dsip:alerg_lek_klic",
                                                            "1.3.6.1.4.1.12559.11.10.1.3.1.42.24"))
                                                    ], new DastaCodedValue("dsip:uaf/dsip:alerg_nelek_klic",
                                                        "1.3.6.1.4.1.12559.11.10.1.3.1.42.19")),
                                                ], []))
                                        ]),
                                    ]),
                                    new TableCell([
                                        new DastaMockCodedValue("dsip:uaf/@typ_agens",
                                            "https://www.dastacr.cz/dasta/hypertext/DSBFX.htm#typ_agens_sezn")
                                    ]),
                                    new TableCell([
                                        new DastaMockCodedValue("dsip:uaf/@typ_reakce",
                                            "https://www.dastacr.cz/dasta/hypertext/DSBFX.htm#typ_reakce_sezn")
                                    ]),
                                    new TableCell([
                                        new Text("dsip:uaf/@alerg_info")
                                    ]), // TODO maybe display in tooltip only #DT-175
                                    new TableCell([
                                        new DastaMockCodedValue("dsip:stav",
                                            "https://www.dastacr.cz/dasta/hypertext/DSAAR.htm#stav_sezn")
                                    ]),
                                    DisplayPeriod(),
                                    new TableCell([
                                        DastaWidgetUtils.TextOrTooltipWithText(
                                            [new TextContainer(TextStyle.Regular, [new Text("dsip:uaf/@ar_text")])], [
                                                new DastaCodedValue("dsip:uaf/@ar_klic",
                                                    "1.3.6.1.4.1.12559.11.10.1.3.1.42.11")
                                            ], "dsip:uaf/@ar_klic"),
                                    ]),
                                    new TableCell([
                                        DastaWidgetUtils.TextOrTooltipWithText(
                                            [new TextContainer(TextStyle.Regular, [new Text("dsip:uaf/@szp_text")])], [
                                                new DastaCodedValue("dsip:uaf/@szp_klic",
                                                    "1.3.6.1.4.1.12559.11.10.1.3.1.42.13")
                                            ], "dsip:uaf/@szp_klic"),
                                    ]),
                                    new TableCell([new Text("dsip:jistota_text")]),
                                    new TableCell([new Text("dsip:uaf/@info_text")]),
                                    // jistota_kod is unused - not implemented in dasta itself
                                    // ignore autor
                                    // ignore dat_ab
                                    // ignore iid
                                    // ignore @ind_oprav_sd, according to Hynek Kružík
                                ])
                            ]),
                        ]),
                    ])),
                ], titleAbbreviations: ("A", "A")),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }

    private TableCell DisplayPeriod()
    {
        return new TableCell([
            new Choose([
                new When("dsip:dat_du or dsip:dat_up", new Choose(
                    [new When("dsip:dat_du", new ConstantText("od "), new DastaDate("dsip:dat_du"))]), new Choose([
                    new When("dsip:dat_up", new ConstantText(" do "), new DastaDate("dsip:dat_up"))
                ]))
            ], DastaWidgetUtils.TextOrTooltipWithText(
                [new TextContainer(TextStyle.Regular, [new Text("dsip:obdobi_text")])], [
                    new DastaCodedValue("dsip:obdobi_kod", "Z_OBDOBI"), // TODO verify DASTA-specific code list #DT-176
                ], "dsip:obdobi_kod")),
        ]);
    }
}