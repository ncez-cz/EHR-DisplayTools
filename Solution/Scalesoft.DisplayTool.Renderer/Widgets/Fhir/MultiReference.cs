using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class MultiReference(
    Func<List<XmlDocumentNavigator>, Task<Widget>> contentBuilder,
    string elementName = "f:entry/f:reference"
)
    : Reference(elementName)
{
    public MultiReference(
        Func<List<XmlDocumentNavigator>, Widget> contentBuilder,
        string elementName = "f:entry/f:reference"
    ) : this(x => Task.FromResult(contentBuilder(x)), elementName)
    {
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        //This returns only references with existing content
        var referenceResult = GetReferences(navigator);

        var errors = referenceResult.Errors;
        var itemNavigators = referenceResult.Navigators;

        if (errors.MaxSeverity() >= ErrorSeverity.Fatal)
        {
            return errors;
        }

        // Render result instead?
        var contentWidget = await contentBuilder(itemNavigators);
        return await contentWidget.Render(navigator, renderer, context);
    }
}