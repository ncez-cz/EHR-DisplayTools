using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class ConstantText(string value) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        var viewModel = new ViewModel
        {
            Text = value,
        };

        var view = await renderer.RenderConstantText(viewModel);

        return new RenderResult(view, []);
    }

    public class ViewModel : ViewModelBase
    {
        public required string Text { get; set; }
    }
}