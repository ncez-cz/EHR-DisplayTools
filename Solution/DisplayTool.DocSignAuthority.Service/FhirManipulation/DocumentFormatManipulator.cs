using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DisplayTool.DocSignAuthority.Service.Exceptions;
using DisplayTool.DocSignAuthority.Service.Models;
using Hl7.Fhir.Model;

namespace DisplayTool.DocSignAuthority.Service.FhirManipulation;

public class DocumentFormatManipulator
{
    private readonly Dictionary<DocumentType, IDocumentFormatManipulator> m_manipulators;
    private readonly ILogger<DocumentFormatManipulator> m_logger;

    public DocumentFormatManipulator(IEnumerable<IDocumentFormatManipulator> manipulators, ILoggerFactory loggerFactory)
    {
        m_manipulators = manipulators.ToDictionary(x => x.Type);
        m_logger = loggerFactory.CreateLogger<DocumentFormatManipulator>();
    }

    public RootResourceResult? SelectRootElement(DocumentType type, string documentContent)
    {
        var manipulator = GetManipulator(type);

        return manipulator.SelectRootElement(documentContent);
    }

    public Signature GetSignature(
        DocumentType type,
        string payload,
        string canonizationUrl,
        RSA privateKey,
        X509Certificate2 certificate,
        DateTime signingTime
    )
    {
        var manipulator = GetManipulator(type);

        return manipulator.GetSignature(payload, canonizationUrl, privateKey, certificate, signingTime);
    }

    public string ReplaceRootBundlePlaceholder(
        DocumentType type,
        string rootBundleContent,
        string bundleIdLookup,
        Bundle resultBundle
    )
    {
        var manipulator = GetManipulator(type);

        return manipulator.ReplaceRootBundlePlaceholder(rootBundleContent, bundleIdLookup, resultBundle);
    }

    public string? GetEncapsulatedBundleSignature(DocumentType type, string documentContent)
    {
        var manipulator = GetManipulator(type);

        return manipulator.GetEncapsulatedBundleSignature(documentContent);
    }

    public string? GetEncapsulatedBundleTarget(DocumentType type, string documentContent)
    {
        var manipulator = GetManipulator(type);

        return manipulator.GetEncapsulatedBundleProvenanceTarget(documentContent);
    }

    public DocumentSignatureVerificationResult Validate(
        DocumentType type,
        string signatureTarget,
        string signatureContent
    )
    {
        var manipulator = GetManipulator(type);

        return manipulator.Validate(signatureTarget, signatureContent);
    }

    public IntegratedSignaturePreprocessResult SelectIntegratedSignatureResourceParts(
        DocumentType type,
        string rootResourceContent
    )
    {
        var manipulator = GetManipulator(type);

        return manipulator.SelectIntegratedSignatureResourceParts(rootResourceContent);
    }

    public string AddMetaIdSignature(
        DocumentType type,
        string payload,
        Provenance signature,
        object? metaNode,
        object? idNode
    )
    {
        var manipulator = GetManipulator(type);

        return manipulator.AddMetaIdSignature(payload, signature, metaNode, idNode);
    }

    private IDocumentFormatManipulator GetManipulator(DocumentType type)
    {
        if (!m_manipulators.TryGetValue(type, out var manipulator))
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("No manipulator for type {type}.", type);
            }

            throw new DocumentSignatureException($"No manipulator for type {type}");
        }

        return manipulator;
    }
}