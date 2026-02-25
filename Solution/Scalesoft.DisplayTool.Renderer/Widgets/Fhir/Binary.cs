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
) : Widget, IResourceWidget
{
    public static string ResourceType => "Binary";
    [UsedImplicitly] public static bool RequiresExternalTitle => true;
    public static bool HasBorderedContainer(Widget widget) => false;

    public Binary() : this(null)
    {
    }

    public static List<Widget> InstantiateMultiple(List<XmlDocumentNavigator> items)
    {
        var imagesNavs = new List<XmlDocumentNavigator>();
        var otherNavs = new List<XmlDocumentNavigator>();
        foreach (var item in items)
        {
            var mimeTypeResult = item.SelectSingleNode("f:contentType/@value").Node?.Value;
            if (mimeTypeResult?.StartsWith("image") == true)
            {
                imagesNavs.Add(item);
            }
            else
            {
                otherNavs.Add(item);
            }
        }

        var filesContent = new List<Widget>
            { new Container(new LocalizedLabel("binary.files"), ContainerType.Div, "text-info-600") };
        foreach (var widget in otherNavs)
        {
            filesContent.Add(new ChangeContext(widget,
                new Container(new Binary(), idSource: widget, optionalClass: "fw-bold")));
        }

        var imagesContent = new List<Widget>();
        foreach (var widget in imagesNavs)
        {
            imagesContent.Add(new ChangeContext(widget,
                new Container(new Binary(), idSource: widget, optionalClass: "m-1 binary-image")));
        }

        return
        [
            new Container([
                new Container(filesContent, ContainerType.Div, imagesNavs.Count == 0 ? "col-12" : "col-6"),
                new Container(new FlexList(imagesContent, FlexDirection.Row), ContainerType.Div,
                    otherNavs.Count == 0 ? "col-12" : "col-6"),
            ], ContainerType.Div, "row")
        ];
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
            return new TextContainer(TextStyle.Muted, new LocalizedLabel("general.data-unavailable")).Render(navigator,
                renderer, context);
        }

        if (!mimeTypeResult.StartsWith("image"))
        {
            var parts = mimeTypeResult.Split('/');
            var type = parts.Length > 1 ? parts[1] : mimeTypeResult;

            var downloadLink = new Link(
                new Concat([new LocalizedLabel("node-names.Attachment-sgl"), new ConstantText($" ({type})")]),
                $"data:{mimeTypeResult};base64,{dataResult}", downloadInfo: "Příloha");
            return downloadLink.Render(navigator, renderer, context);
        }

        var result = new Image($"data:{mimeTypeResult};base64,{dataResult}", altText, width, height);

        return result.Render(navigator, renderer, context);
    }
}