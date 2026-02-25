using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Dasta;

public class DastaHeader : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
        var widget = new Table([
            new TableBody([
                new TableRow([
                    new TableCell([
                            new ConstantText("Identifikátor dokumentu"),
                        ],
                        TableCellType.Header),
                    new TableCell([
                        new Text("@id_soubor")
                    ]),
                    new TableCell([
                            new EhdsiDisplayLabel(LabelCodes.DocumentCreationDate),
                        ],
                        TableCellType.Header),
                    new TableCell([
                        new DastaDate("@dat_vb"),
                    ]),
                ]),
            ]),
        ]);

        return widget.Render(navigator, renderer, context);
    }
}
