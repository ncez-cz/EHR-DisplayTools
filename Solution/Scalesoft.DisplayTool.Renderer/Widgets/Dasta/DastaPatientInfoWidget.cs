using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaPatientInfoWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new ConcatBuilder("/ds:dasta/ds:is/dsip:ip", i =>
            [
                new Section($"/ds:dasta/ds:is/dsip:ip[{i + 1}]", null,
                    [
                        new ConstantText("Osobní údaje pacienta"),
                    ], // TODO no suitable eHDSI labels found, maybe tweak text #DT-175
                    [
                        new Row([
                            new Choose([
                                new When("dsip:titul_pred", new Container([
                                    new PlainBadge(new ConstantText("Titul před jménem"), Severity.Primary),
                                    new Heading([new Text("dsip:titul_pred")], HeadingSize.H3),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:jmeno", new Container([
                                    new PlainBadge(new ConstantText("Jméno"), Severity.Primary),
                                    new Heading([new Text("dsip:jmeno")], HeadingSize.H3),
                                ]))
                            ]),
                            new Container([
                                new PlainBadge(new ConstantText("Příjmení"), Severity.Primary),
                                new Heading([new Text("dsip:prijmeni")], HeadingSize.H3),
                            ]),
                            new Choose([
                                new When("dsip:titul_za", new Container([
                                    new PlainBadge(new ConstantText("Titul za jménem"), Severity.Primary),
                                    new Heading([new Text("dsip:titul_za")], HeadingSize.H3),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:dat_dn", new Container([
                                    new PlainBadge(new ConstantText("Datum a čas narození"), Severity.Primary),
                                    new Heading([new DastaDate("dsip:dat_dn"),], HeadingSize.H3),
                                ]))
                            ]),
                            new Container([
                                new PlainBadge(new ConstantText("Identifikátor pacienta"), Severity.Primary),
                                new Heading([
                                    new Text("@id_pac"),
                                    new Choose([
                                        new When("not(@typ_id_pac) or @typ_id_pac = '0'",
                                            new TextContainer(TextStyle.Muted,
                                                [new ConstantText(" (číslo pojištěnce)"),]))
                                    ]),
                                ], HeadingSize.H3),
                            ]),
                        ]),
                        new Row([
                            new Choose([
                                new When("dsip:rodcis", new Container([
                                    new PlainBadge(new ConstantText("Rodné číslo"), Severity.Primary),
                                    new Heading([new Text("dsip:rodcis"),], HeadingSize.H6),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:rip", new Container([
                                    new PlainBadge(new ConstantText("Resortní identifikátor pacienta"),
                                        Severity.Primary),
                                    new Heading([new Text("dsip:rip"),], HeadingSize.H6),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:id_card", new Container([
                                    new PlainBadge(new ConstantText("Číslo občanského průkazu"), Severity.Primary),
                                    new Heading([new Text("dsip:id_card/dsip:id_number"),], HeadingSize.H6),
                                    new Choose([
                                        new When("dsip:id_card/dsip:dat_pl",
                                            new PlainBadge(new ConstantText("Konec platnosti občanského průkazu"),
                                                Severity.Primary),
                                            new Heading([new DastaDate("dsip:id_card/dsip:dat_pl"),], HeadingSize.H6))
                                    ]),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:passport", new Container([
                                    new PlainBadge(new ConstantText("Číslo cestovního pasu"), Severity.Primary),
                                    new Heading([new Text("dsip:passport/dsip:id_number"),], HeadingSize.H6),
                                    new Choose([
                                        new When("dsip:passport/dsip:stat",
                                            new PlainBadge(new ConstantText("Stát, který cestovní pas vydal"),
                                                Severity.Primary), new Heading([
                                                new DastaCodedValue("dsip:passport/dsip:stat",
                                                    "ISO_3166-1_alpha-3"), // TODO finalize code list value - custom value is used #DT-176
                                            ], HeadingSize.H6))
                                    ]),
                                    new Choose([
                                        new When("dsip:passport/dsip:dat_pl",
                                            new PlainBadge(new ConstantText("Konec platnosti cestovního pasu"),
                                                Severity.Primary),
                                            new Heading([new DastaDate("dsip:passport/dsip:dat_pl"),], HeadingSize.H6))
                                    ]),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:misto_nar", new Container([
                                    new PlainBadge(new ConstantText("Místo narození"), Severity.Primary),
                                    new Heading([new Text("dsip:misto_nar"),], HeadingSize.H6),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:stat_pris", new Container([
                                    new PlainBadge(new ConstantText("Státní příslušnost"),
                                        Severity.Primary),
                                    new Heading([
                                        new DastaCodedValue("dsip:stat_pris",
                                            "ISO_3166-1_alpha-3"), // TODO finalize code list value - custom value is used #DT-176
                                    ], HeadingSize.H6),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:dat_de", new Container([
                                    new PlainBadge(new ConstantText("Datum a čas úmrtí"), Severity.Primary),
                                    new Heading([new DastaDate("dsip:dat_de"),], HeadingSize.H6),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:sex", new Container([
                                    new PlainBadge(new ConstantText("Pohlaví úřední"), Severity.Primary),
                                    new Heading(
                                    [
                                        new DastaMockCodedValue("dsip:sex",
                                            "https://www.dastacr.cz/dasta/hypertext/MZAUO.htm#sex_sezn"),
                                    ], HeadingSize.H6),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:sex_klin", new Container([
                                    new PlainBadge(new ConstantText("Pohlaví pro klinické použití"), Severity.Primary),
                                    new Heading([
                                        new DastaMockCodedValue("dsip:sex_klin",
                                            "https://www.dastacr.cz/dasta/hypertext/MZAUO.htm#sex_sezn"),
                                    ], HeadingSize.H6),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:rod_prijm", new Container([
                                    new PlainBadge(new ConstantText("Rodné příjmení"), Severity.Primary),
                                    new Heading([new Text("dsip:rod_prijm"),], HeadingSize.H6),
                                ]))
                            ]),
                            new Choose([
                                new When("dsip:jine_idu", new Container([
                                    new PlainBadge(new ConstantText("Jiné identifikační údaje"), Severity.Primary),
                                    new Heading([new Text("dsip:jine_idu"),], HeadingSize.H6),
                                ]))
                            ]),
                            // ignore typ_sdel
                            // ignore duvernost
                            new Container([
                                new PlainBadge(new ConstantText("Komunikační jazyk"), Severity.Primary),
                                new Choose([
                                    new When("dsip:jazyk", new ItemListBuilder("dsip:jazyk", ItemListType.Unordered,
                                        i =>
                                        [
                                            new Heading([
                                                new DastaCodedValue("@jazyk_klic",
                                                    "1.3.6.1.4.1.12559.11.10.1.3.1.42.6"),
                                            ], HeadingSize.H6),
                                            new Choose([new When("@pref = 'P'", new ConstantText(" (preferovaný)"))]),
                                            // ignore a - interpreter's contact
                                            // ignore pozn
                                            // ignore autor
                                            // ignore dat_od
                                            // ignore dat_do
                                            // ignore dat_ab
                                            // ignore @ind_oprav_sd
                                        ]))
                                ], new Heading([
                                    new DastaCodedValue(context.Language.Primary.Code,
                                        "1.3.6.1.4.1.12559.11.10.1.3.1.42.6"), // if communication language is not specified, cs-CZ is used
                                ], HeadingSize.H6)),
                            ]),
                            // ignore ipi_o
                            // ignore ipi_v
                            new Choose([
                                new When("dsip:povolani", new Container([
                                    new PlainBadge(new ConstantText("Povolání pacienta"), Severity.Primary),
                                    new ItemListBuilder("dsip:povolani", ItemListType.Unordered, i =>
                                    [
                                        new Heading([new Text("dsip:povolani_text"),], HeadingSize.H6),
                                        // ignore autor
                                        // ignore dat_od
                                        // ignore dat_do
                                        // ignore dat_ab
                                    ]),
                                ]))
                            ]),
                            // ignore h, fyznal, grav, pv_pac, p, n, u, an, anf, oc, dg, le, lek, chv, fzs, ld, fs, pn, ts, tps
                            // relevant parts of ku element are rendered elsewhere
                            //new ChangeContext("ds:a[@typ = '1' or @typ = '2']", DisplayAddressAndTelecom()),
                        ]),
                        new ListBuilder("ds:a[(@typ = '1' or @typ = '2')]", FlexDirection.Row, i =>
                        [
                            DisplayAddressAndTelecom(),
                        ], null, "patient-cards-layout"),
                        new ListBuilder("ds:a[not(@typ = '1' or @typ = '2')]", FlexDirection.Row, i =>
                        [
                            new Collapser([
                                new DastaCodedValue("@typ", "TAB_TA"), // TODO verify DASTA-specific code list #DT-176
                            ], DisplayRelatedPersonsAndCompanies()),
                        ], null, "patient-cards-layout"),
                    ], titleAbbreviations: ("OÚ", "PD")
                ),
            ]),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }

    private Widget AddressConnectionsWidget()
    {
        var connectionTextWidget = new TextContainer(TextStyle.Regular, [
            new Text("ds:obsah"),
            new Choose([
                new When("ds:vnitrni", new ConstantText(" - "), new Text("ds:vnitrni"))
            ]),
        ]);

        return new ConcatBuilder("ds:as", i =>
            [
                new Container([
                    new TextContainer(TextStyle.Bold | TextStyle.Small,
                    [
                        new DastaMockCodedValue("@typ", "https://www.dastacr.cz/dasta/hypertext/DSAAL.htm#typ_hodn"),
                    ]),
                    new LineBreak(),
                    new Choose([
                        new When("ds:sdeleni or ds:heslo", new Tooltip(
                            [connectionTextWidget],
                            [
                                new Choose([
                                    new When("ds:sdeleni",
                                        new NameValuePair([new ConstantText("Textová informace k adrese")],
                                            [new Text("ds:sdeleni")]))
                                ]),
                                new Choose([
                                    new When("ds:heslo",
                                        new NameValuePair([new ConstantText("Heslo pro sdělení informací")],
                                            [new Text("ds:heslo")]))
                                ]),
                            ],
                            []
                        ))
                    ], connectionTextWidget),
                    new LineBreak(),
                ]),
            ], "@poradi", orderAscending: false) // TODO what should be done if no order is provided #DT-175
            ;
    }

    private Widget DisplayAddressAndTelecom()
    {
        const string ownPersonDataTypeValue = "(@typ = '1' or @typ = '2')";
        return new Container([
            new PlainBadge(new TextContainer(TextStyle.Regular, [
                new EhdsiDisplayLabel(LabelCodes.ContactInformation),
                new Choose([
                    new When($"{ownPersonDataTypeValue} and @ind_kont = 'K'", new ConstantText(" (korespondenční)"))
                ]),
            ])),
            new LineBreak(),
            new Choose([new When(ownPersonDataTypeValue, [..DisplayDataValidity(EntityType.OwnPerson)])]),
            DisplayAddress(),
            AddressConnectionsWidget(),
            new Choose([new When(ownPersonDataTypeValue, [..DisplayDataNote(EntityType.OwnPerson)])]),
        ]);
    }

    private Widget DisplayAddress()
    {
        return new Container([
            new TextContainer(TextStyle.Bold | TextStyle.Small, [new ConstantText("Adresa")]),
            new LineBreak(),
            new Choose([
                new When("ds:adr", new Text("ds:adr"))
            ]),
            new Choose([
                new When("ds:dop1", new ConstantText(" "), new Text("ds:dop1"))
            ]),
            new Choose([
                new When("ds:dop2", new ConstantText(" "), new Text("ds:dop2"))
            ]),
            new Choose([
                new When("ds:mesto", new ConstantText(", "), new Text("ds:mesto"))
            ]),
            new Choose([
                new When("ds:psc", new ConstantText(" "), new Text("ds:psc"))
            ]),
            // ignore stat
            new Choose([
                new When("ds:stat_text", new ConstantText(" "), new Text("ds:stat_text"))
            ]),
            // ignore gps
            new LineBreak(),
        ]);
    }

    private IList<Widget> DisplayRelatedPersonsAndCompanies()
    {
        return
        [
            ..DisplayDataValidity(EntityType.Other),
            // ignore o_jmeno, o_prijmeni, o_titul_pred, o_titul_za, f_jmeno, f_ico - ignore structured name
            new Container([
                new PlainBadge(new ConstantText("Jméno / název"), Severity.Primary),
                new Heading([new Text("ds:jmeno"),], HeadingSize.H6),
            ]),
            new Choose([
                new When("@typ = '3' and ds:vztah", new Container([
                    new PlainBadge(new ConstantText("Příbuzenský vztah k pacientovi"), Severity.Primary),
                    new Heading([new Text("ds:vztah"),], HeadingSize.H6),
                ]))
            ]),
            new Choose([
                new When("@typ = 'K'", new Choose([
                    new When("ds:k_osoba_klic", new Container([
                        new PlainBadge(new ConstantText("Osobní vztah"), Severity.Primary),
                        new Heading([
                            new DastaCodedValue("ds:k_osoba_klic", "1.3.6.1.4.1.12559.11.10.1.3.1.42.38"),
                        ], HeadingSize.H6),
                        // ignore k_osoba_text
                    ]))
                ]), new Choose([
                    new When("ds:k_osoba_pozn", new Container([
                        new PlainBadge(new ConstantText("Poznámka"), Severity.Primary),
                        new Heading([new Text("ds:k_osoba_pozn"),], HeadingSize.H6),
                    ]))
                ]), new Choose([
                    new When("ds:k_osoba_typ", new Container([
                        new PlainBadge(new ConstantText("Typ kontaktu"), Severity.Primary),
                        new Heading([
                            new DastaMockCodedValue("ds:k_osoba_typ",
                                "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_typ_sezn"),
                        ], HeadingSize.H6),
                    ]))
                ]), new Choose([
                    new When("ds:k_osoba_pravo", new Container([
                        new PlainBadge(new ConstantText("Právo"), Severity.Primary),
                        new LineBreak(),
                        new Choose([
                            new When("ds:k_osoba_pravo = 'N'", new DastaMockCodedValue("'N'",
                                "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_pravo_sezn")),
                            new When("ds:k_osoba_pravo = 'A'", new DastaMockCodedValue("'A'",
                                "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_pravo_sezn")),
                            new When("ds:k_osoba_pravo = 'D'", new ItemList(ItemListType.Unordered, [
                                new DastaMockCodedValue("'A'",
                                    "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_pravo_sezn"),
                                new DastaMockCodedValue("'D'",
                                    "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_pravo_sezn"),
                            ])),
                            new When("ds:k_osoba_pravo = 'V'", new ItemList(ItemListType.Unordered, [
                                new DastaMockCodedValue("'A'",
                                    "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_pravo_sezn"),
                                new DastaMockCodedValue("'D'",
                                    "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_pravo_sezn"),
                                new DastaMockCodedValue("'V'",
                                    "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_pravo_sezn"),
                            ])),
                            new When("ds:k_osoba_pravo = 'S'", new ItemList(ItemListType.Unordered, [
                                new DastaMockCodedValue("'A'",
                                    "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_pravo_sezn"),
                                new DastaMockCodedValue("'D'",
                                    "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_pravo_sezn"),
                                new DastaMockCodedValue("'V'",
                                    "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_pravo_sezn"),
                                new DastaMockCodedValue("'S'",
                                    "https://www.dastacr.cz/dasta/hypertext/DSAAK.htm#k_osoba_pravo_sezn"),
                            ])),
                        ], new Text("ds:k_osoba_pravo")),
                    ]))
                ])),
            ]),
            new Choose([
                new When("@typ = '4' or @typ = 'G' or @typ = 'M' or @typ = 'C'", new Choose([
                    new When("ds:icl", new Container([
                        new PlainBadge(new ConstantText("Identifikační číslo praktického lékaře pacienta"),
                            Severity.Primary),
                        new Heading([new Text("ds:icl"),], HeadingSize.H6),
                    ]))
                ]), new Choose([
                    new When("ds:id_zp", new Container([
                        new PlainBadge(new ConstantText("Identifikace ZP"), Severity.Primary),
                        new Heading([new Text("ds:id_zp"),], HeadingSize.H6),
                    ]))
                ])),
            ]),
            DisplayAddressAndTelecom(),
            ..DisplayDataNote(EntityType.Other),
        ];
    }

    private IList<Widget> DisplayDataValidity(EntityType entityType)
    {
        var validFromLabel = new ConstantText("Platnost od");
        Widget[] validFromLabelWidget = entityType == EntityType.Other
            ? [new PlainBadge(validFromLabel, Severity.Primary)]
            : [new TextContainer(TextStyle.Bold | TextStyle.Small, [validFromLabel]), new LineBreak()];

        var validToLabel = new ConstantText("Platnost do");
        Widget[] validToLabelWidget = entityType == EntityType.Other
            ? [new PlainBadge(validToLabel, Severity.Primary)]
            : [new TextContainer(TextStyle.Bold | TextStyle.Small, [validToLabel]), new LineBreak()];

        var validFromValue = new DastaDate("ds:dat_od");
        Widget[] validFromValueWidget = entityType == EntityType.Other
            ? [new Heading([validFromValue,], HeadingSize.H6)]
            : [validFromValue, new LineBreak()];

        var validToValue = new DastaDate("ds:dat_do");
        Widget[] validToValueWidget = entityType == EntityType.Other
            ? [new Heading([validToValue,], HeadingSize.H6)]
            : [validToValue, new LineBreak()];

        return
        [
            new Choose([
                new When("ds:dat_od", new Container([
                    ..validFromLabelWidget,
                    ..validFromValueWidget,
                ]))
            ]),
            new Choose([
                new When("ds:dat_do", new Container([
                    ..validToLabelWidget,
                    ..validToValueWidget,
                ]))
            ]),
        ];
    }

    private IList<Widget> DisplayDataNote(EntityType entityType)
    {
        var noteLabel = new ConstantText("Poznámka k údajům");
        Widget[] noteLabelWidget = entityType == EntityType.Other
            ?
            [
                new PlainBadge(noteLabel, Severity.Primary),
            ]
            :
            [
                new TextContainer(TextStyle.Bold | TextStyle.Small, [noteLabel]), new LineBreak(),
            ];

        var noteValue = new Text("ds:pozn");
        Widget[] noteValueWidget = entityType == EntityType.Other
            ?
            [
                new Heading([noteValue,], HeadingSize.H6),
            ]
            :
            [
                noteValue, new LineBreak(),
            ];

        return
        [
            new Choose([
                new When("ds:pozn", new Container([
                    ..noteLabelWidget,
                    ..noteValueWidget,
                ]))
            ]),
        ];
    }

    private enum EntityType
    {
        OwnPerson,
        Other,
    }
}