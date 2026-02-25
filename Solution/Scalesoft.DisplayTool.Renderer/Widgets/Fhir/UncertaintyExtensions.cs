using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class UncertaintyExtensions : Widget
{
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
        List<Widget> widgets = [];

        var uncertaintyExtension =
            navigator.SelectSingleNode(
                "f:extension[@url='http://hl7.org/fhir/StructureDefinition/iso21090-uncertainty']");
        var uncertaintyTypeExtension =
            navigator.SelectSingleNode(
                "f:extension[@url='http://hl7.org/fhir/StructureDefinition/iso21090-uncertaintyType']");

        if (uncertaintyExtension.Node != null || uncertaintyTypeExtension.Node != null)
        {
            widgets.Add(new ConstantText(" ("));
        }

        if (uncertaintyExtension.Node != null)
        {
            widgets.AddRange([
                new ConstantText("σ: "),
                new ChangeContext(uncertaintyExtension, new ShowDecimal("f:valueDecimal")),
            ]);
        }

        if (uncertaintyTypeExtension.Node != null)
        {
            if (uncertaintyExtension.Node != null)
            {
                widgets.Add(new ConstantText(", "));
            }

            widgets.AddRange([
                new LocalizedLabel("iso21090-uncertaintyType"),
                new ConstantText(": "),
                new ChangeContext(uncertaintyTypeExtension,
                    new EnumLabel("f:valueCode", "http://hl7.org/fhir/ValueSet/probability-distribution-type")),
            ]);
        }

        if (uncertaintyExtension.Node != null || uncertaintyTypeExtension.Node != null)
        {
            widgets.Add(new ConstantText(")"));
        }

        return widgets.RenderConcatenatedResult(navigator, renderer, context);
    }
}