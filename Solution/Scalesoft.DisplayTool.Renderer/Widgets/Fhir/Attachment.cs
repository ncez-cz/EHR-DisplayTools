using System.Text;
using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

/// <summary>
/// Equivalent of https://hl7.org/fhir/R4/datatypes.html#Attachment
/// </summary>
public class Attachment(
    string? maxWidth = null,
    string? maxHeight = null,
    string? altText = null,
    bool onlyContentOrUrl = false,
    string? imageOptionalClass = null
) : ItemListResourceBase<Attachment>, IResourceWidget
{
    public static string ResourceType => "Attachment";
    [UsedImplicitly]
    public static bool RequiresExternalTitle => true;

    public Attachment() : this(null)
    {
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        
        if (context.RenderMode == RenderMode.Documentation)
        {
            return navigator.GetFullPath();
        }

        var mimeType = navigator.SelectSingleNode("f:contentType/@value").Node?.Value;
        var title = navigator.SelectSingleNode("f:title/@value").Node?.Value ?? altText;
        var data = navigator.SelectSingleNode("f:data/@value").Node?.Value;

        if (data == null && mimeType == null && onlyContentOrUrl)
        {
            return new ParseError()
            {
                Message = "Data or MimeType is missing",
                Kind = ErrorKind.MissingValue,
                Severity = ErrorSeverity.Warning,
                Path = navigator.GetFullPath(),
            };
        }
        
        var maxWidthHeight = new StringBuilder();

        if (maxHeight != null)
        {
            maxWidthHeight.Append($"max-height: {maxHeight};");
        }
        
        if (maxWidth != null)
        {
            maxWidthHeight.Append($" max-width: {maxWidth};");
        }

        var render = new Choose([
            new When("not(f:data)",
                new Choose([
                        new When("not(f:url)",
                            new Choose([
                                new When("f:title",
                                    new Concat([
                                        new Text("f:title/@value"),
                                        new TextContainer(TextStyle.Muted,
                                            new ConstantText("(Nejsou dostupná žádná data)")),
                                    ])
                                )
                            ], new TextContainer(TextStyle.Muted, new ConstantText("Nejsou dostupná žádná data")))
                        )
                    ], new Link(new Choose([
                            new When("f:title", new Text("f:title/@value")
                            ),
                        ], new ConstantText("Odkaz")), new Text("f:url/@value")
                    )
                )
            ),
            new When("starts-with(f:contentType/@value, 'image/')",
                new Image($"data:{mimeType};base64,{data}", title, optionalClass: imageOptionalClass, optionalStyle: maxWidthHeight.ToString())
            ),
        ], new Link(
            new Choose([
                    new When("f:title", new Text("f:title/@value")),
                ], new ConstantText("Odkaz na stažení")
            ), $"data:{mimeType};base64,{data}", downloadInfo: title));


        return await render.Render(navigator, renderer, context);
    }
}