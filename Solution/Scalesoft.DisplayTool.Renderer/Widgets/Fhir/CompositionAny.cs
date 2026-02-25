using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PatientSection;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.Sections;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class CompositionAny : Widget, IResourceWidget
{
    public static string ResourceType => "Composition";
    
    public static bool HasBorderedContainer(Widget widget) => false;
    
    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        return items.Select(x => new ChangeContext(x, new CompositionAny())).ToList<Widget>();
    }
    
    public override Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer, RenderContext context)
    {
        List<Widget> widgets =
        [
            ShowSingleReference.WithDefaultDisplayHandler(x => [new Patient(x)], "f:subject"),

            new ConcatBuilder("f:section", _ => [new FhirSection()]),
        ];
        return widgets.RenderConcatenatedResult(navigator, renderer, context);
    }
}