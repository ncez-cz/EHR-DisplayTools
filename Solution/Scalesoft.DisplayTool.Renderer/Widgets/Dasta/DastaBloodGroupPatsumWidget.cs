using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaBloodGroupPatsumWidget : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
        List<Widget> widgetTree =
        [
            new Variable("bloodGroup",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:uks"),
            new Section("/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:uks[1]", null,
                [new EhdsiDisplayLabel(LabelCodes.BloodGroup)],
                [
                    new Table([
                        new TableCell([new EhdsiDisplayLabel(LabelCodes.BloodGroup)], TableCellType.Header),
                        new TableCell([new ConstantText("Rhesus faktor")], TableCellType.Header),
                        new TableCell([new ConstantText("Datum věrohodného zjištění")], TableCellType.Header),
                        new ChangeContext("$bloodGroup",
                        [
                            new TableRow([
                                new TableCell([
                                    new Choose(
                                    [
                                        new When("dsip:ks_rh_text or dsip:ks_rh", [
                                            new Tooltip([new TextContainer(TextStyle.Regular, [new Text("dsip:ks_rh_text")])], [
                                                new DastaCodedValue("dsip:ks_rh", "LUKSAB0"), // TODO verify DASTA-specific code list #DT-176
                                                // TODO handle optional text with required code #DT-175
                                            ], [])
                                        ])
                                    ],[
                                        new Text("dsip:krevskup")
                                    ]),
                                ]),
                                new TableCell([
                                    new Text("dsip:rh")
                                ]),
                                // ignore autor
                                new TableCell([
                                    new DastaDate("dsip:dat_du")
                                ]),
                                // ignore dat_ab
                                // ignore iid
                                // ignore @ind_oprav_sd, according to Hynek Kružík
                            ])
                        ]),
                    ]),
                ], titleAbbreviations: ("KS", "BG")),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}
