using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using DisplayTool.DocSignAuthority.Service.Models;

namespace DisplayTool.DocSignAuthority.Service.Canonicalizers;

/// <summary>
///     Canonicalizes FHIR XML documents according to the FHIR signature specification.
/// </summary>
public class FhirXmlDocumentCanonicalizer : ICanonicalizer
{
    public DocumentType Type => DocumentType.FhirXml;
    public string MethodUrl => "http://hl7.org/fhir/canonicalization/xml";

    private const string FhirNamespace = "http://hl7.org/fhir";
    private const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";

    /// <summary>
    ///     Canonicalizes a FHIR XML document according to the FHIR signature specification.
    /// </summary>
    /// <param name="xmlContent">The XML content to canonicalize.</param>
    /// <returns>The canonicalized XML as a string.</returns>
    public string Canonicalize(string xmlContent)
    {
        // Step 1: Parse and normalize the document
        var doc = ParseAndNormalizeDocument(xmlContent);

        // Step 2: Remove comments
        // Comments are removed during C14 transform

        // Step 3: Normalize whitespace (except in XHTML narrative)
        NormalizeWhitespace(doc.DocumentElement!);

        // Step 4: Ensure default namespaces for FHIR and XHTML
        EnsureDefaultNamespaces(doc.DocumentElement!);

        // Step 5: Apply Canonical XML 1.1
        var canonicalXml = ApplyCanonicalXml11(doc);

        // Step 6: Convert XML entities to Unicode representation
        // Unicode conversion is done during C14 transform

        // Step 7: Add XML declaration
        return $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>{canonicalXml}";
    }

    /// <summary>
    ///     Canonicalizes a FHIR XML document from a stream.
    /// </summary>
    /// <param name="inputStream">The input stream containing XML.</param>
    /// <returns>The canonicalized XML as a string.</returns>
    public string Canonicalize(Stream inputStream)
    {
        using var reader = new StreamReader(inputStream, Encoding.UTF8);
        var content = reader.ReadToEnd();
        return Canonicalize(content);
    }

    /// <summary>
    ///     Canonicalizes a FHIR XML document and writes to an output stream.
    /// </summary>
    /// <param name="xmlContent">The XML content to canonicalize.</param>
    /// <param name="outputStream">The output stream to write to.</param>
    public void Canonicalize(string xmlContent, Stream outputStream)
    {
        var result = Canonicalize(xmlContent);
        var bytes = Encoding.UTF8.GetBytes(result);
        outputStream.Write(bytes, 0, bytes.Length);
    }

    private static XmlDocument ParseAndNormalizeDocument(string xmlContent)
    {
        var settings = new XmlReaderSettings
        {
            IgnoreComments = false, // We'll handle comments ourselves
            IgnoreWhitespace = false, // We'll handle whitespace ourselves
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        var doc = new XmlDocument
        {
            PreserveWhitespace = true
        };

        using var stringReader = new StringReader(xmlContent);
        using var xmlReader = XmlReader.Create(stringReader, settings);
        doc.Load(xmlReader);

        return doc;
    }

    private static void NormalizeWhitespace(XmlElement element)
    {
        // Skip XHTML content - preserve whitespace in narrative
        if (element.NamespaceURI == XhtmlNamespace)
        {
            return;
        }

        var nodesToRemove = new List<XmlNode>();
        var nodesToProcess = new List<XmlElement>();

        foreach (XmlNode child in element.ChildNodes)
        {
            switch (child)
            {
                // Remove whitespace-only text nodes outside XHTML
                case XmlWhitespace:
                case XmlSignificantWhitespace:
                case XmlText textNode when string.IsNullOrWhiteSpace(textNode.Value):
                    nodesToRemove.Add(child);
                    break;
                case XmlElement childElement:
                    nodesToProcess.Add(childElement);
                    break;
            }
        }

        foreach (var node in nodesToRemove)
        {
            element.RemoveChild(node);
        }

        foreach (var childElement in nodesToProcess)
        {
            NormalizeWhitespace(childElement);
        }

        // Normalize attribute values - collapse multiple spaces to single space
        foreach (XmlAttribute attr in element.Attributes)
        {
            attr.Value = NormalizeAttributeWhitespace(attr.Value);
        }
    }

    private static string NormalizeAttributeWhitespace(string value)
    {
        // Replace multiple whitespace characters with a single space
        return RegexDefinitions.MultipleWhitespaceRegex().Replace(value, " ").Trim();
    }

    private static void EnsureDefaultNamespaces(XmlElement element)
    {
        // Check if this is a FHIR root element
        if (element.NamespaceURI == FhirNamespace)
        {
            // Remove prefix if present and ensure default namespace
            if (!string.IsNullOrEmpty(element.Prefix))
            {
                var newElement = element.OwnerDocument.CreateElement(element.LocalName, FhirNamespace);
                CopyAttributes(element, newElement);
                CopyChildren(element, newElement);
                element.ParentNode?.ReplaceChild(newElement, element);
                element = newElement;
            }

            // Ensure xmlns is set as default
            if (element.GetAttribute("xmlns") != FhirNamespace)
            {
                element.SetAttribute("xmlns", FhirNamespace);
            }
        }

        // Process child elements
        var children = element.ChildNodes.OfType<XmlElement>().ToList();
        foreach (var child in children)
        {
            EnsureDefaultNamespaces(child);

            // Handle XHTML elements (typically in narrative div)
            if (child.NamespaceURI == XhtmlNamespace &&
                child.GetAttribute("xmlns") != XhtmlNamespace)
            {
                child.SetAttribute("xmlns", XhtmlNamespace);
            }
        }
    }

    private static void CopyAttributes(XmlElement source, XmlElement target)
    {
        foreach (XmlAttribute attr in source.Attributes)
        {
            if (attr.Name != "xmlns" && !attr.Name.StartsWith("xmlns:"))
            {
                target.SetAttribute(attr.Name, attr.Value);
            }
        }
    }

    private static void CopyChildren(XmlElement source, XmlElement target)
    {
        while (source.HasChildNodes)
        {
            target.AppendChild(source.FirstChild!);
        }
    }

    private static string ApplyCanonicalXml11(XmlDocument doc)
    {
        // Use Canonical XML 1.0 instead of 1.1 - main difference is in handling of inheritance of id attribute and xml:base in fragments, we do not support canonicalization on fragments
        var transform = new XmlDsigC14NTransform(false);
        transform.LoadInput(doc);

        using var output = (MemoryStream)transform.GetOutput(typeof(MemoryStream));
        return Encoding.UTF8.GetString(output.ToArray());
    }
}