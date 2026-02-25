using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class AbsentData(string path) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        List<Widget> widgets =
        [
            new TextContainer(TextStyle.Muted, [
                new EhdsiDisplayLabel(LabelCodes.MissingInfo),
                new ConstantText(", "),
                new LocalizedLabel("general.reason"),
                new ConstantText(": "),
                new Choose([
                    new When($"{path}/f:extension/f:valueCode/@value", // TODO - maybe extension condition is missing? - see IsDataAbsent
                        new EnumLabel($"{path}/f:extension/f:valueCode/@value",
                            "http://hl7.org/fhir/ValueSet/data-absent-reason")),
                    new When($"{path}/f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/data-absent-reason']/f:code/@value",
                        new EnumLabel(
                            $"{path}/f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/data-absent-reason']/f:code/@value",
                            "http://terminology.hl7.org/CodeSystem/data-absent-reason")),
                ]),
            ])
        ];

        return widgets.RenderConcatenatedResult(navigator, renderer, context);
    }
}