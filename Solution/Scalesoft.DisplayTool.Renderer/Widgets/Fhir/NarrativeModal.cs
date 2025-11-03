using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class NarrativeModal(string path = "f:text", bool alignRight = true, Widget? openButtonContent = null) : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var modal =
            new HideableDetails(
                ContainerType.Div,
                alignRight ? "ms-auto" : null,
                new Modal(
                    new Heading(
                        [
                            new DisplayLabel(LabelCodes.OriginalNarrative),
                            new Optional(
                                "f:code",
                                new ConstantText(" - "),
                                new CodeableConcept()
                            ),
                        ],
                        HeadingSize.H4,
                        "m-0"
                    ),
                    new Narrative(path),
                    SupportedIcons.FileLines,
                    openButtonContent,
                    openButtonCustomClass: "narrative-modal-button"
                )
            );

        return modal.Render(navigator, renderer, context);
    }
}