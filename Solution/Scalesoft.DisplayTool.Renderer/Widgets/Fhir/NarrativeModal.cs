using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
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
            new Modal(
                new Heading(
                    [
                        new EhdsiDisplayLabel(LabelCodes.OriginalNarrative),
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
            );

        var showNarrativeByDefault = NarrativeUtils.ShowNarrativeByDefault(navigator.SelectSingleNode("f:text"));

        Widget output =
            showNarrativeByDefault
                ? new NullWidget()
                : new HideableDetails(
                    ContainerType.Div,
                    alignRight ? "ms-auto" : null,
                    modal
                );

        return output.Render(navigator, renderer, context);
    }
}