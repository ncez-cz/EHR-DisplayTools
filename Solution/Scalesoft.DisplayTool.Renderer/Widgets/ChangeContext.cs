using System.Diagnostics.CodeAnalysis;
using System.Text;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class ChangeContext : Widget
{
    private string? Path { get; }
    private XmlDocumentNavigator? Node { get; }
    private Widget[] Child { get; }

    [MemberNotNullWhen(true, nameof(Node))]
    [MemberNotNullWhen(false, nameof(Path))]
    private bool NodeProvidedDirectly { get; }

    public ChangeContext(string path, params Widget[] child)
    {
        Path = path;
        Child = child;
    }

    public ChangeContext(XmlDocumentNavigator? node, params Widget[] child)
    {
        Node = node;
        Child = child;
        NodeProvidedDirectly = true;
    }

    public ChangeContext(XmlDocumentNavigator? node, string path, params Widget[] child)
    {
        Node = node;
        Path = path;
        Child = child;
        NodeProvidedDirectly = true;
    }

    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (NodeProvidedDirectly && Node == null)
        {
            return RenderResult.NullResult;
        }

        List<XmlDocumentNavigator> elements;
        if (NodeProvidedDirectly)
        {
            elements = Path != null ? Node.SelectAllNodes(Path).ToList() : [Node];
        }
        else
        {
            elements = navigator.SelectAllNodes(Path).ToList();
        }

        if (elements.Count == 0)
        {
            return new ParseError
            {
                Kind = ErrorKind.MissingValue,
                Severity = ErrorSeverity.Warning,
                Path = navigator.GetFullPath() + '/' + Path,
            };
        }

        var resultBuilder = new StringBuilder();
        List<ParseError> errors = [];
        foreach (var element in elements)
        {
            var result = await Child.RenderConcatenatedResult(element, renderer, context);
            errors.AddRange(result.Errors);

            if (result.MaxSeverity >= ErrorSeverity.Fatal)
            {
                return errors;
            }

            resultBuilder.Append(result.Content);
        }

        return new RenderResult(resultBuilder.ToString(), errors);
    }
}