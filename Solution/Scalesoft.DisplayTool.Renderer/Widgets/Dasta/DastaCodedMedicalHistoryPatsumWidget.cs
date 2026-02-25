using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaCodedMedicalHistoryPatsumWidget : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgetTree =
        [
            new Variable("medicalHistoryCoded",
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:anf"),
            new Section(
                "/ds:dasta/ds:is/dsip:ip/dsip:ku/dsip:ku_z[@typku='PATSUM.DAT']/dsip:ku_z_patsumdat/dsip:u/dsip:anf[1]",
                null,
                [new ConstantText("Anamnéza formalizovaná")],
                [
                    // TODO implement parsing logic #DT-145
                    new Collapser([new ConstantText("Osobní anamnéza")], [
                        new Table([]),
                        new Table([]),
                        new Table([]),
                        new Table([]),
                    ]),
                    new Collapser([new ConstantText("Epidemiologická anamnéza")], [
                        new Table([]),
                        new Table([]),
                    ]),
                    new Collapser([new ConstantText("Rodinná anamnéza")], [
                        new Table([]),
                        new Table([]),
                    ]),
                    new Collapser([new ConstantText("Abusus")], [
                        new Table([]),
                        new Table([]),
                        new Table([]),
                        new Table([]),
                    ]),
                ], titleAbbreviations: ("A", "H")),
        ];

        return widgetTree.RenderConcatenatedResult(navigator, renderer, context);
    }
}