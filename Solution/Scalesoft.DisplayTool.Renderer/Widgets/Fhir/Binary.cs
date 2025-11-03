using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class Binary(
    string? width = null,
    string? height = null,
    string? altText = null,
    string? mimeType = null,
    string? base64data = null,
    bool onlyContentOrUrl = false
) : ItemListResourceBase<Binary>, IResourceWidget
{
    public static string ResourceType => "Binary";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;

    public Binary() : this(null)
    {
    }
    
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (context.RenderMode == RenderMode.Documentation)
        {
            return Task.FromResult<RenderResult>(navigator.GetFullPath());
        }

        var mimeTypeResult = mimeType ?? navigator.SelectSingleNode("f:contentType/@value").Node?.Value;
        var dataResult = base64data ?? navigator.SelectSingleNode("f:data/@value").Node?.Value;

        if (mimeTypeResult == null || (dataResult == null && onlyContentOrUrl))
        {
            return Task.FromResult<RenderResult>(new ParseError
            {
                Kind = ErrorKind.MissingValue,
                Message = "Missing mimeType attribute in binary resource",
                Path = navigator.GetFullPath(), Severity = ErrorSeverity.Warning
            });
        }

        if (dataResult == null)
        {
            return new TextContainer(TextStyle.Muted, new ConstantText("Nejsou dostupná žádná data")).Render(navigator, renderer, context);
        }

        if (!mimeTypeResult.StartsWith("image"))
        {
            var parts = mimeTypeResult.Split('/');
            var type = parts.Length > 1 ? parts[1] : mimeTypeResult;
                
            var downloadLink = new Link(new ConstantText($"Příloha ({type})"), $"data:{mimeTypeResult};base64,{dataResult}", downloadInfo: "Příloha");
            return downloadLink.Render(navigator, renderer, context);
        }

        var result = new Image($"data:{mimeTypeResult};base64,{dataResult}", altText, width, height);

        return result.Render(navigator, renderer, context);

    }
}