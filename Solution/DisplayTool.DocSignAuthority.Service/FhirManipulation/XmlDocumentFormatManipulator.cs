using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using DisplayTool.DocSignAuthority.Service.Exceptions;
using DisplayTool.DocSignAuthority.Service.Models;
using Hl7.Fhir.Model;

namespace DisplayTool.DocSignAuthority.Service.FhirManipulation;

public class XmlDocumentFormatManipulator : IDocumentFormatManipulator
{
    private readonly ILogger<XmlDocumentFormatManipulator> m_logger;

    public XmlDocumentFormatManipulator(ILoggerFactory loggerFactory)
    {
        m_logger = loggerFactory.CreateLogger<XmlDocumentFormatManipulator>();
    }

    public DocumentType Type => DocumentType.FhirXml;

    public RootResourceResult? SelectRootElement(string documentContent)
    {
        var doc = new XmlDocument();
        try
        {
            doc.LoadXml(documentContent);
        }
        catch (XmlException e)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(e, "Failed to select FHIR XML root element. Invalid XML document.");
            }

            throw new DocumentSignatureException("Invalid XML document.");
        }

        var rootBundles = doc.SelectNodes("/");
        if (rootBundles == null || rootBundles.Count != 1)
        {
            return null;
        }

        var rootResource = rootBundles[0];

        if (rootResource == null)
        {
            return null;
        }

        var id = rootResource.SelectSingleNode("*[local-name()='id']/@value")?.Value;

        return new RootResourceResult
        {
            ResourceName = rootResource.Name,
            Content = rootResource.OuterXml,
            Id = id,
        };
    }

    public Signature GetSignature(
        string payload,
        string canonicalizationUrl,
        RSA privateKey,
        X509Certificate2 certificate,
        DateTime signingTime
    )
    {
        throw new NotImplementedException();
    }

    public string ReplaceRootBundlePlaceholder(string rootResourceContent, string bundleIdLookup, Bundle resultBundle)
    {
        throw new NotImplementedException();
    }

    public string? GetEncapsulatedBundleSignature(string documentContent)
    {
        throw new NotImplementedException();
    }

    public string? GetEncapsulatedBundleProvenanceTarget(string documentContent)
    {
        throw new NotImplementedException();
    }

    public DocumentSignatureVerificationResult Validate(string signatureTarget, string signatureContent)
    {
        throw new NotImplementedException();
    }

    public IntegratedSignaturePreprocessResult SelectIntegratedSignatureResourceParts(string rootResourceContent)
    {
        throw new NotImplementedException();
    }

    public string AddMetaIdSignature(string payload, Provenance provenance, object? metaContent, object? idContent)
    {
        throw new NotImplementedException();
    }
}