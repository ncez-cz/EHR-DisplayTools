using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class HospitalLogo(string imageOptionalClass = "header-image") : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var brandLogo =
            ShowSingleReference.WithDefaultDisplayHandler(
                documentReferenceNav =>
                [
                    new Container(
                        new ChangeContext("f:content[1]/f:attachment",
                            new Binary()), // why does logo DocumentReference contain multiple content?
                        optionalClass: imageOptionalClass, idSource: documentReferenceNav),
                ],
                "f:valueReference");

        return brandLogo.Render(navigator, renderer, context);
    }
}