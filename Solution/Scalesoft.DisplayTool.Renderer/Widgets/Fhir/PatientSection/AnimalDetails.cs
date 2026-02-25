using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir.PatientSection;

public class AnimalDetails : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        Widget[] tree =
        [
            new ChangeContext("f:extension[@url='species']/f:valueCodeableConcept",
                new NameValuePair(
                    new PlainBadge(new LocalizedLabel("patient-animal.species")),
                    new Heading([new CodeableConcept()], HeadingSize.H6)
                )
            ),
            new Optional("f:extension[@url='breed']/f:valueCodeableConcept",
                new NameValuePair(
                    new PlainBadge(new LocalizedLabel("patient-animal.breed")),
                    new Heading([new CodeableConcept()], HeadingSize.H6)
                )
            ),
        ];

        return tree.RenderConcatenatedResult(navigator, renderer, context);
    }
}