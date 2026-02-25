using System.Xml.XPath;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

/// <summary>
///     Fhir narrative widget.
/// </summary>
/// <param name="path">Path to the narrative element.</param>
public class Narrative(string path = ".") : Widget
{
    private static readonly List<string> m_tagWhitelist =
    [
        "p", "br", "div", "span", "b", "i", "strong", "em", "small", "big", "tt", "sub", "sup", "blockquote", "q",
        "cite", "abbr", "acronym", "dfn", "code", "var", "samp", "kbd", "pre", "address", "hr", "ul", "ol", "li", "dl",
        "dt", "dd", "table", "caption", "thead", "tfoot", "tbody", "tr", "th", "td", "colgroup", "col", "h1", "h2",
        "h3", "h4", "h5", "h6", "a", "img",
    ];

    private static readonly List<string> m_attributeWhitelist =
    [
        "id", "class", "title", "style", "href", "name", "src", "alt", "height", "width", "colspan", "rowspan", "type",
        "value", "cite", "abbr", "align", "axis", "border", "cellpadding", "cellspacing", "char", "charoff", "dir",
        "frame", "headers", "lang", "title", "valign",
    ];


    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        if (context.RenderMode == RenderMode.Documentation)
        {
            return navigator.SelectSingleNode(path).GetFullPath();
        }

        var div = navigator.SelectSingleNode($"{path}/xhtml:div");
        var parseResult = ParseNode(div);
        if (parseResult.Errors.MaxSeverity() >= ErrorSeverity.Fatal)
        {
            return parseResult.Errors;
        }

        var narrativeNode = navigator.SelectSingleNode(path);
        var tree = new NarrativeContainer(parseResult.Results, idSource: narrativeNode);

        return await tree.Render(div, renderer, context);
    }

    private ParseResult<Widget> ParseNode(XmlDocumentNavigator node)
    {
        if (node.Node == null)
        {
            return new ParseError
            {
                Kind = ErrorKind.MissingValue, Path = node.GetFullPath(), Message = "Missing node",
                Severity = ErrorSeverity.Warning
            };
        }

        return node.Node.NodeType switch
        {
            XPathNodeType.Whitespace or XPathNodeType.SignificantWhitespace or XPathNodeType.Text
                => new ConstantText(node.Node.OuterXml),
            XPathNodeType.Element
                => ParseHtmlElement(node),
            XPathNodeType.Comment or XPathNodeType.Namespace => new ParseResult<Widget>(),
            _ => throw new InvalidOperationException(),
        };
    }

    private ParseResult<Widget> ParseHtmlElement(XmlDocumentNavigator element)
    {
        if (element.Node == null)
        {
            throw new InvalidOperationException();
        }

        var name = element.Node.LocalName.ToLower();
        if (!m_tagWhitelist.Contains(name))
        {
            return new ParseError
            {
                Kind = ErrorKind.InvalidValue, Message = $"Unsupported tag {name}", Path = element.GetFullPath(),
                Severity = ErrorSeverity.Warning
            };
        }

        var attributes = GetAttributes(element);
        if (attributes.Errors.MaxSeverity() >= ErrorSeverity.Fatal)
        {
            return attributes.Errors;
        }

        var children = element.SelectAllNodes("node()").Select(ParseNode).ToList();
        var childWidgets = children.SelectMany(x => x.Results).ToList();
        var errors = children.SelectMany(x => x.Errors).ToList();

        if (errors.MaxSeverity() >= ErrorSeverity.Fatal)
        {
            return errors;
        }

        Widget widget = name switch
        {
            "img" => new Container([new NarrativeImage(attributes.Results)], idSource: element),
            _ => new HtmlElement(name, childWidgets, attributes.Results),
        };

        return new ParseResult<Widget> { Results = [widget], Errors = errors };
    }

    ParseResult<KeyValuePair<string, string>> GetAttributes(XmlDocumentNavigator node)
    {
        var attributes = node.SelectAllNodes("@*").ToList();
        List<KeyValuePair<string, string>> results = [];
        List<ParseError> errors = [];

        foreach (var attribute in attributes)
        {
            if (attribute.Node == null)
            {
                continue;
            }

            if (!m_attributeWhitelist.Contains(attribute.Node.Name))
            {
                errors.Add(new ParseError
                {
                    Kind = ErrorKind.InvalidValue, Message = $"Attribute {attribute.Node.Name} is not allowed",
                    Path = attribute.GetFullPath(), Severity = ErrorSeverity.Warning
                });

                continue;
            }

            results.Add(new KeyValuePair<string, string>(attribute.Node.Name, attribute.Node.Value));
        }

        return new ParseResult<KeyValuePair<string, string>> { Results = results, Errors = errors };
    }
}