using System.Xml.XPath;
using Scalesoft.DisplayTool.Shared.Signature;

namespace Scalesoft.DisplayTool.Shared.DocumentNavigation;

public class XmlDocumentNavigator
{
    private const string HideableContextName = "hideable-context";

    private const string CompositionSubjectIdName = "composition-subject-id";

    private const string SignatureValidationResultVariableName = "signature-validation-result";

    public XmlDocumentNavigator? Parent { get; }

    public XPathNavigator? Node { get; }

    public string PathFromParent { get; }

    public bool IsInHideableContext => Variables.ContainsKey(HideableContextName);

    public string? CompositionSubjectId
    {
        get => Variables.TryGetValue(CompositionSubjectIdName, out var value) ? value as string : null;
        set
        {
            if (value != null)
            {
                Variables[CompositionSubjectIdName] = value;
            }
            else
            {
                Variables.Remove(CompositionSubjectIdName);
            }
        }
    }

    public DocumentSignatureValidationOperationResult? SignatureValidationResult
    {
        get => Variables.TryGetValue(SignatureValidationResultVariableName, out var value)
            ? value as DocumentSignatureValidationOperationResult
            : null;
        set
        {
            if (value != null)
            {
                Variables[SignatureValidationResultVariableName] = value;
            }
            else
            {
                Variables.Remove(SignatureValidationResultVariableName);
            }
        }
    }

    public bool IsSubjectFromComposition()
    {
        if (Node?.Name != "subject" || CompositionSubjectId == null)
        {
            return false;
        }

        var expression = CreateExpression("f:reference/@value");
        return CompositionSubjectId == Node.SelectSingleNode(expression)?.InnerXml;
    }

    public Dictionary<string, object> Variables { get; }

    public Dictionary<string, string> Namespaces { get; }

    public XmlDocumentNavigator(
        XPathNavigator? node,
        XmlDocumentNavigator? parent = null,
        Dictionary<string, object>? parentVariables = null,
        Dictionary<string, string>? namespaces = null,
        string pathFromParent = "/"
    )
    {
        Parent = parent;
        Node = node;
        PathFromParent = pathFromParent;

        // Save a copy of the parent's variable list, so we don't modify the parent's list.
        Variables = new Dictionary<string, object>(parentVariables ?? new Dictionary<string, object>());
        Namespaces = namespaces ?? new Dictionary<string, string>();
    }

    public XmlDocumentNavigator SelectSingleNode(string xpath)
    {
        if (Node == null)
        {
            throw new InvalidOperationException();
        }

        var expression = CreateExpression(xpath);
        var node = Node.SelectSingleNode(expression);

        return new XmlDocumentNavigator(node, this, Variables, Namespaces, xpath);
    }

    public IEnumerable<XmlDocumentNavigator> SelectAllNodes(string xpath)
    {
        if (Node == null)
        {
            throw new InvalidOperationException($"Node is null, path: {GetFullPath()}");
        }

        var expression = CreateExpression(xpath);
        var nodes = Node.SelectAllNodes(expression);

        return nodes.Select(x => new XmlDocumentNavigator(x, this, Variables, Namespaces, xpath));
    }

    public IEnumerable<XmlDocumentNavigator> SelectChildren(XPathNodeType type)
    {
        if (Node == null)
        {
            throw new InvalidOperationException();
        }

        var nodes = Node.SelectChildren(type);

        return nodes.OfType<XPathNavigator>().Select(x =>
            new XmlDocumentNavigator(x, this, Variables, Namespaces, "child::node()")
        );
    }

    public bool EvaluateCondition(string test)
    {
        var expression = CreateExpression($"boolean({test})");
        return Node?.Evaluate(expression) is true;
    }

    public string Evaluate(string path)
    {
        var expression = CreateExpression(path);
        return Node?.Evaluate(expression).ToString() ?? string.Empty;
    }

    public double? EvaluateNumber(string path)
    {
        var expression = CreateExpression($"number({path})");
        var val = Node?.Evaluate(expression);
        if (val is double doubleVal && !double.IsNaN(doubleVal))
        {
            return doubleVal;
        }

        return null;
    }

    public string EvaluateString(string path)
    {
        var expression = CreateExpression($"string({path})");
        return Node?.Evaluate(expression).ToString() ?? string.Empty;
    }

    public string GetFullPath()
    {
        if (IsAbsolutePath(PathFromParent) || Parent == null)
        {
            return PathFromParent;
        }

        return Parent.GetFullPath() + '/' + PathFromParent;
    }

    public void SetVariable(string name, string? select)
    {
        if (string.IsNullOrWhiteSpace(select))
        {
            if (Variables.ContainsKey(name))
            {
                return;
            }

            Variables[name] = string.Empty;
            return;
        }

        if (Node == null)
        {
            throw new InvalidOperationException();
        }

        var expression = CreateExpression(select);
        var value = Node.Evaluate(expression);

        if (value is XPathNodeIterator iterator)
        {
            var copy = iterator.Clone();
            if (copy.MoveNext() && copy.Current?.NodeType == XPathNodeType.Attribute)
            {
                value = copy.Current.Value;
            }
        }

        Variables[name] = value;
    }

    public void SetVariableValue(string name, object? value)
    {
        if (value == null)
        {
            if (Variables.ContainsKey(name))
            {
                return;
            }

            Variables[name] = string.Empty;
            return;
        }

        Variables[name] = value;
    }

    public void SetParameter(string name, string? select)
    {
        // xsl:parameter element shouldn't override an already existing value - it should only provide a default value
        if (Variables.ContainsKey(name))
        {
            return;
        }

        if (string.IsNullOrEmpty(select))
        {
            Variables.Add(name, string.Empty);
            return;
        }

        if (Node == null)
        {
            throw new InvalidOperationException();
        }

        var expression = CreateExpression(select);
        var value = Node.Evaluate(expression);
        Variables.Add(name, value);
    }

    public void SetParameterValue(string name, object? value)
    {
        // xsl:parameter element shouldn't override an already existing value - it should only provide a default value
        if (Variables.ContainsKey(name))
        {
            return;
        }

        if (value == null)
        {
            Variables.Add(name, string.Empty);
            return;
        }

        Variables.Add(name, value);
    }

    public void AddNamespace(string prefix, string uri)
    {
        Namespaces.Add(prefix, uri);
    }

    private static bool IsAbsolutePath(string path)
    {
        return path.StartsWith('/');
    }

    /// <summary>
    ///     Compiles an XPath expression using the provided XPath string and the configured XML namespace manager.
    /// </summary>
    /// <param name="xpath">The XPath string to compile into an XPathExpression.</param>
    /// <returns>
    ///     An <see cref="XPathExpression" /> object compiled from the provided XPath string,
    ///     configured with a custom context that includes user-defined variables.
    /// </returns>
    private XPathExpression CreateExpression(string xpath)
    {
        var context = new CustomContext(Variables);

        foreach (var (prefix, uri) in Namespaces)
        {
            context.AddNamespace(prefix, uri);
        }

        var expression = XPathExpression.Compile(xpath, context);

        return expression;
    }

    public XmlDocumentNavigator Clone()
    {
        return new XmlDocumentNavigator(Node, Parent, Variables, Namespaces, PathFromParent);
    }

    public XmlDocumentNavigator WithHideableContext()
    {
        var result = new XmlDocumentNavigator(Node, Parent, Variables, Namespaces, PathFromParent);
        result.Variables.TryAdd(HideableContextName, true);

        return result;
    }
}