using System.Text;
using JetBrains.Annotations;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.Fhir.ResourceResolving;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

/// <summary>
///     Equivalent of https://hl7.org/fhir/R4/datatypes.html#Attachment
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
    [UsedImplicitly] public static bool RequiresExternalTitle => true;
    public static bool HasBorderedContainer(Widget widget) => false;

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
            return new ParseError
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

        var url = navigator.SelectSingleNode("f:url/@value").Node?.Value ?? "";

        // ignore f:language - it is not defined as a codeableConcept, but rather as a string bindable to a code system - no realistic way to display it
        // ignore f:hash - it is not obvious where to display it and it has little value in a user-friendly reader
        Widget[] tree =
        [
            new Choose([
                new When("not(f:data)",
                    new Choose([
                            new When("not(f:url)",
                                new Choose([
                                    new When("f:title",
                                        new Concat([
                                                new Text("f:title/@value"),
                                                new TextContainer(
                                                    TextStyle.Muted,
                                                    [
                                                        new ConstantText("("),
                                                        new LocalizedLabel("general.data-unavailable"),
                                                        new ConstantText(")"),
                                                    ]
                                                )
                                            ]
                                        )
                                    ),
                                ], new TextContainer(TextStyle.Muted, new LocalizedLabel("general.data-unavailable")))
                            ),
                        ], new Link(new Concat([
                                new Choose([
                                    new When("f:title", new Text("f:title/@value")
                                    ),
                                ], new LocalizedLabel("attachment.url")),
                                new Condition(
                                    "f:size",
                                    new ConstantText(" ("),
                                    new LocalizedLabel("attachment.size"),
                                    new ConstantText(": "),
                                    new Text("f:size/@value"),
                                    new ConstantText(")")
                                ),
                            ], string.Empty), url
                        )
                    )
                ),
                new When("starts-with(f:contentType/@value, 'image/')",
                    new Image($"data:{mimeType};base64,{data}", title, optionalClass: imageOptionalClass,
                        optionalStyle: maxWidthHeight.ToString())
                ),
            ], new Link(
                new Choose([
                        new When("f:title", new Text("f:title/@value")),
                    ], new LocalizedLabel("general.download-link")
                ), $"data:{mimeType};base64,{data}", downloadInfo: title)),
            new Condition("f:creation",
                new TextContainer(TextStyle.Muted, [
                    new ConstantText(" ("),
                    new LocalizedLabel("attachment.creation"),
                    new ConstantText(": "),
                    new ShowDateTime("f:creation"),
                    new ConstantText(")"),
                ])
            ),
        ];


        return await tree.RenderConcatenatedResult(navigator, renderer, context);
    }
}