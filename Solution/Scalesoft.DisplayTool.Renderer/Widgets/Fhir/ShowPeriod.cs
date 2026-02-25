using Scalesoft.DisplayTool.Renderer.Constants;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class ShowPeriod(string path = ".") : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (IsDataAbsent(navigator, path))
        {
            return new AbsentData(path).Render(navigator, renderer, context);
        }

        if (context.RenderMode == RenderMode.Documentation)
        {
            return Task.FromResult<RenderResult>(navigator.SelectSingleNode(path).GetFullPath());
        }

        var tree = new ChangeContext(path,
            new Optional("f:start", new EhdsiDisplayLabel(LabelCodes.From), new ConstantText(" "), new ShowDateTime()),
            new Condition("f:start and f:end", new ConstantText(" ")),
            new Optional("f:end",  new EhdsiDisplayLabel(LabelCodes.To), new ConstantText(" "), new ShowDateTime()));

        return tree.Render(navigator, renderer, context);
    }
}