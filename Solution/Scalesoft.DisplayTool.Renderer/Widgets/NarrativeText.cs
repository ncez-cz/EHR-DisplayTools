using System.Xml.XPath;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Models.Enums;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class NarrativeText(string? select, string? paramName) : Widget
{
    public override async Task<RenderResult> Render(XmlDocumentNavigator navigator, IWidgetRenderer renderer,
        RenderContext context)
    {
        XPathNavigator? el = null;
        if (!string.IsNullOrEmpty(select))
        {
            el = navigator.SelectSingleNode(select).Node;
        }
        else if (!string.IsNullOrEmpty(paramName))
        {
            if (navigator.Variables.TryGetValue(paramName, out var val) && val is XPathNodeIterator iter)
            {
                el = iter.Current;
            }
        }
        else
        {
            throw new InvalidOperationException("Incorrect widget creation params");
        }

        if (el == null)
        {
            return new ParseError
            {
                Kind = ErrorKind.MissingValue,
                Severity = ErrorSeverity.Warning,
                Path = navigator.GetFullPath(),
            };
        }

        var elNav = new XmlDocumentNavigator(el, parent: navigator, namespaces: navigator.Namespaces);

        var widgetsWithErrors = ParseElementChildren(elNav);

        var rr = await widgetsWithErrors.Widgets.RenderConcatenatedResult(navigator, renderer, context);
        rr.Errors.AddRange(widgetsWithErrors.Errors);
        
        return rr;
    }

    private WidgetsWithErrors ParseElementChildren(XmlDocumentNavigator el)
    {
        var widgets = new List<Widget>();
        var errors = new List<ParseError>();
        var children = el.SelectChildren(XPathNodeType.All).ToList();

        IterateChildren(children, widgets, errors);

        return new WidgetsWithErrors(widgets, errors);
    }

    private void IterateChildren(List<XmlDocumentNavigator> nodes, List<Widget> result, List<ParseError> errors)
    {
        foreach (var child in nodes)
        {
            ParseElement(child, result, errors);
        }
    }

    private void ParseElement(XmlDocumentNavigator el, List<Widget> result, List<ParseError> errors)
    {
        if (el.Node == null)
        {
            throw new InvalidOperationException();
        }
        
        switch (el.Node.NodeType)
        {
            case XPathNodeType.Whitespace: // assume output type is html, so preserve whitespaces as-is
            case XPathNodeType.Text:
                var text = el.Node.OuterXml;
                result.Add(new ConstantText(text));
                break;
            case XPathNodeType.Element:
                var elName = el.Node.LocalName;
                if (elName == "br")
                {
                    result.Add(new LineBreak());
                    break;
                }

                var innerWidgetsWithErrors = ParseElementChildren(el);
                errors.AddRange(innerWidgetsWithErrors.Errors);
                switch (elName)
                {
                    case "content":
                        var revisedAttrVal = el.SelectSingleNode("@revised").Node?.Value;
                        if (revisedAttrVal == "delete")
                        {
                            break;
                        }

                        result.AddRange(innerWidgetsWithErrors.Widgets);
                        break;
                    case "paragraph":
                        result.Add(new Container(innerWidgetsWithErrors.Widgets, ContainerType.Paragraph));
                        break;
                    case "pre":
                        result.Add(new TextContainer(TextStyle.Preformatted, innerWidgetsWithErrors.Widgets));
                        break;
                    case "list":
                        var listTypeAttrVal = el.SelectSingleNode("@listType").Node?.Value;
                        var listType = listTypeAttrVal == "ordered" ? ItemListType.Ordered : ItemListType.Unordered;
                        var captions = el.SelectAllNodes("n1:caption").ToList();
                        var items = el.SelectAllNodes("n1:item").ToList();
                        if (captions.Count != 0)
                        {
                            var listCaptions = new List<Widget>();
                            IterateChildren(captions, listCaptions, errors);
                            result.Add(new TextContainer(TextStyle.Bold, listCaptions));
                        }

                        var itemWidgets = new List<Widget>();
                        IterateChildren(items, itemWidgets, errors);
                        result.Add(new ItemList(listType, itemWidgets));
                        break;
                    case "item":
                        result.Add(new Concat(innerWidgetsWithErrors.Widgets));
                        break;
                    case "caption":
                        if (el.Parent?.Node?.LocalName == "table")
                        {
                            result.Add(new TableCaption(innerWidgetsWithErrors.Widgets));
                        }
                        else
                        {
                            result.AddRange(innerWidgetsWithErrors.Widgets);
                            result.Add(new ConstantText(": "));
                        }
                        
                        break;
                    case "table":
                        result.Add(new Table(innerWidgetsWithErrors.Widgets, true));
                        break;
                    case "thead":
                        result.Add(new TableHead([new TableRow(innerWidgetsWithErrors.Widgets)]));
                        break;
                    case "tfoot":
                        result.Add(new TableFooter(innerWidgetsWithErrors.Widgets));
                        break;
                    case "tbody":
                        result.Add(new TableBody(innerWidgetsWithErrors.Widgets));
                        break;
                    case "colgroup":
                        // TODO mostly used for styling, consider rendering #DT-111
                        break;
                    case "col":
                        // TODO mostly used for styling, consider rendering #DT-111
                        break;
                    case "tr":
                        result.Add(new TableRow(innerWidgetsWithErrors.Widgets));
                        break;
                    case "th":
                        result.Add(new TableCell(innerWidgetsWithErrors.Widgets.ToArray(), TableCellType.Header));
                        break;
                    case "td":
                        result.Add(new TableCell(innerWidgetsWithErrors.Widgets.ToArray()));
                        break;
                    default:
                        // Unknown element, render as-is. Consider displaying a warning
                        result.Add(new RawText(el.Node.OuterXml));
                        errors.Add(new ParseError
                        {
                            Kind = ErrorKind.MissingHandlerUsedFallback,
                            Severity = ErrorSeverity.Informational,
                            Path = el.GetFullPath(),
                            Message = $"Missing special handling for structured document element {elName}, displaying as-is",
                        });
                        break;
                }

                break;
            default:
                throw new InvalidOperationException();
        }
    }

    private class WidgetsWithErrors
    {
        public WidgetsWithErrors(List<Widget> widgets, List<ParseError> errors)
        {
            Widgets = widgets;
            Errors = errors;
        }

        public List<Widget> Widgets { get; set; }
        
        public List<ParseError> Errors { get; set; }
    }
}
