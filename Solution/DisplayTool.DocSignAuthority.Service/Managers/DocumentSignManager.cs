using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DisplayTool.DocSignAuthority.Service.Canonicalizers;
using DisplayTool.DocSignAuthority.Service.Exceptions;
using DisplayTool.DocSignAuthority.Service.FhirManipulation;
using DisplayTool.DocSignAuthority.Service.Models;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;

namespace DisplayTool.DocSignAuthority.Service.Managers;

public class DocumentSignManager
{
    private readonly Canonicalizer m_canonicalizer;
    private readonly DocumentFormatManipulator m_documentFormatManipulator;
    private readonly IOptions<CertificateConfiguration> m_certificateConfiguration;
    private readonly ILogger<DocumentSignManager> m_logger;

    public DocumentSignManager(
        Canonicalizer canonicalizer,
        DocumentFormatManipulator documentFormatManipulator,
        IOptions<CertificateConfiguration> certificateConfiguration,
        ILoggerFactory loggerFactory
    )
    {
        m_canonicalizer = canonicalizer;
        m_documentFormatManipulator = documentFormatManipulator;
        m_certificateConfiguration = certificateConfiguration;
        m_logger = loggerFactory.CreateLogger<DocumentSignManager>();
    }

    /// <summary>
    ///     Signs the document using a Bundle with a provenance resource encapsulated inside a root Bundle
    /// </summary>
    /// <param name="document">Document content</param>
    /// <param name="documentType">Document type</param>
    /// <returns>Signed document</returns>
    /// <exception cref="DocumentSignatureException">Thrown when signing fails</exception>
    public byte[] SignEncapsulated(byte[] document, DocumentType documentType)
    {
        var (privateKey, cert, rootResourceResult) = ValidateInputAndPrepareInputs(document, documentType);
        var rootResourceContent = rootResourceResult.Content;
        var now = DateTime.Now;
        var canonicalizationResult = m_canonicalizer.Canonicalize(rootResourceContent, documentType);

        var signature = m_documentFormatManipulator.GetSignature(documentType,
            canonicalizationResult.CanonicalizedDocument, canonicalizationResult.CanonicalizationMethod, privateKey,
            cert, now);

        var provenance = new Provenance
        {
            Signature = [signature],
            Target =
            [
                new ResourceReference { Reference = $"{rootResourceResult.ResourceName}/{rootResourceResult.Id}" }
            ],
            Agent =
            [
                new Provenance.AgentComponent
                {
                    Who = (ResourceReference)signature.Who!,
                }
            ],
        };
        var bundle = new Bundle
        {
            Entry =
            [
                new Bundle.EntryComponent { Resource = provenance },
                new Bundle.EntryComponent { Resource = new Bundle { Id = rootResourceResult.Id } }
            ]
        };

        var serialized =
            m_documentFormatManipulator.ReplaceRootBundlePlaceholder(documentType, rootResourceContent,
                rootResourceResult.Id, bundle);

        return Encoding.UTF8.GetBytes(serialized);
    }

    /// <summary>
    ///     Validates a FHIR Bundle with a provenance resource encapsulated inside a root Bundle
    /// </summary>
    /// <param name="document">Document content</param>
    /// <param name="documentType">Document type</param>
    /// <returns>Validation status as boolean - true when the signature is valid</returns>
    /// <exception cref="DocumentSignatureException">Thrown when validation fails</exception>
    public DocumentSignatureVerificationResult ValidateEncapsulated(byte[] document, DocumentType documentType)
    {
        if (document.Length == 0)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Unable to verify the document's signature. Document is empty.");
            }

            throw new DocumentSignatureException("Document is empty");
        }

        var documentContent = Encoding.UTF8.GetString(document);

        var signature = m_documentFormatManipulator.GetEncapsulatedBundleSignature(documentType, documentContent);

        if (string.IsNullOrEmpty(signature))
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Unable to verify the document's signature. Signature is missing.");
            }

            throw new DocumentSignatureException("Signature is missing", ErrorCode.NoSignature);
        }

        var signatureTarget = m_documentFormatManipulator.GetEncapsulatedBundleTarget(documentType, documentContent);

        if (string.IsNullOrEmpty(signatureTarget))
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Unable to verify the document's signature. Signed content was not found.");
            }

            throw new DocumentSignatureException("Signature target is missing");
        }

        var canonicalizationResult = m_canonicalizer.Canonicalize(signatureTarget, documentType);

        var isValid =
            m_documentFormatManipulator.Validate(documentType, canonicalizationResult.CanonicalizedDocument,
                signature);

        return isValid;
    }

    /// <summary>
    ///     Signs the document using a provenance resource placed alongside signed content
    /// </summary>
    /// <param name="document">Document content</param>
    /// <param name="documentType">Document type</param>
    /// <returns>Signed document</returns>
    /// <exception cref="DocumentSignatureException">Thrown when signing fails</exception>
    public byte[] SignIntegrated(byte[] document, DocumentType documentType)
    {
        var (privateKey, cert, rootResourceResult) = ValidateInputAndPrepareInputs(document, documentType);
        var rootResourceContent = rootResourceResult.Content;
        var now = DateTime.Now;
        var splitSignedResource =
            m_documentFormatManipulator.SelectIntegratedSignatureResourceParts(documentType, rootResourceContent);
        if (splitSignedResource.SignatureContent != null)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Failed to sign the document. The document already has a signature resource.");
            }

            throw new DocumentSignatureException("Document already has a signature resource");
        }

        var canonicalizationResult = m_canonicalizer.Canonicalize(splitSignedResource.ContentToSign, documentType);

        var signature = m_documentFormatManipulator.GetSignature(documentType,
            canonicalizationResult.CanonicalizedDocument, canonicalizationResult.CanonicalizationMethod, privateKey,
            cert, now);

        var provenance = new Provenance
        {
            Signature = [signature],
            Target =
            [
                new ResourceReference { Reference = $"{rootResourceResult.ResourceName}/{rootResourceResult.Id}" }
            ],
            Agent =
            [
                new Provenance.AgentComponent
                {
                    Who = (ResourceReference)signature.Who!,
                }
            ],
        };

        var res = m_documentFormatManipulator.AddMetaIdSignature(documentType,
            canonicalizationResult.CanonicalizedDocument, provenance, splitSignedResource.MetaNode,
            splitSignedResource.IdNode);

        return Encoding.UTF8.GetBytes(res);
    }

    /// <summary>
    ///     Validates a FHIR Bundle with a provenance resource encapsulated inside a root Bundle
    /// </summary>
    /// <param name="document">Document content</param>
    /// <param name="documentType">Document type</param>
    /// <returns>Validation status as boolean - true when the signature is valid</returns>
    /// <exception cref="DocumentSignatureException">Thrown when validation fails</exception>
    public DocumentSignatureVerificationResult ValidateIntegrated(byte[] document, DocumentType documentType)
    {
        if (document.Length == 0)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Unable to verify the document's signature. Document is empty.");
            }

            throw new DocumentSignatureException("Document is empty");
        }

        var documentContent = Encoding.UTF8.GetString(document);
        var rootResourceResult = m_documentFormatManipulator.SelectRootElement(documentType, documentContent);
        if (rootResourceResult == null)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Failed to sign the document. Unable to find the root resource to sign.");
            }

            throw new DocumentSignatureException("Document is not a FHIR bundle");
        }

        var rootResourceContent = rootResourceResult.Content;
        var splitSignedResource =
            m_documentFormatManipulator.SelectIntegratedSignatureResourceParts(documentType, rootResourceContent);

        if (splitSignedResource.SignatureContent == null)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Failed to verify the document's signature. Signature was not found.");
            }

            throw new DocumentSignatureException("Unable to verify the signature. Signature was not found",
                ErrorCode.NoSignature);
        }

        var canonicalizationResult = m_canonicalizer.Canonicalize(splitSignedResource.ContentToSign, documentType);

        var isValid =
            m_documentFormatManipulator.Validate(documentType, canonicalizationResult.CanonicalizedDocument,
                splitSignedResource.SignatureContent);

        return isValid;
    }

    private (RSA privateKey, X509Certificate2 cert, RootResourceValidatedResult rootResource)
        ValidateInputAndPrepareInputs(
            byte[] document,
            DocumentType documentType
        )
    {
        if (document.Length == 0)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Failed to sign the document. The document is empty.");
            }

            throw new DocumentSignatureException("Document is empty");
        }

        var certConfig = m_certificateConfiguration.Value;
        RSA privateKey;
        try
        {
            privateKey = LoadRsaPrivateKeyByPath(certConfig.PrivateKeyPath, certConfig.Password);
        }
        catch (DocumentSignatureException e)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(e, "Failed to sign the document. Unable to load the signing private key.");
            }

            throw new DocumentSignatureException("Signing private key not found");
        }

        X509Certificate2 cert;
        try
        {
            cert = LoadCertificateByPath(certConfig.CertificatePath);
        }
        catch (DocumentSignatureException e)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(e, "Failed to sign the document. Unable to load the public key.");
            }

            throw new DocumentSignatureException("Signing certificate not found");
        }

        var documentContent = Encoding.UTF8.GetString(document);
        var rootResourceResult = m_documentFormatManipulator.SelectRootElement(documentType, documentContent);
        if (rootResourceResult == null)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Failed to sign the document. Unable to find the root resource to sign.");
            }

            throw new DocumentSignatureException("Document is not a FHIR bundle");
        }

        if (!RootResourceValidatedResult.Validate(rootResourceResult, out var validatedRootResourceResult))
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError("Failed to sign the document. Root resource has no id.");
            }

            throw new DocumentSignatureException("Bundle has no id and cannot be processed");
        }


        return (privateKey, cert, validatedRootResourceResult);
    }

    private RSA LoadRsaPrivateKeyByPath(string path, string? password = null)
    {
        try
        {
            var content = File.ReadAllText(path);
            var key = RSA.Create();
            if (string.IsNullOrEmpty(password))
            {
                key.ImportFromPem(content);
            }
            else
            {
                key.ImportFromEncryptedPem(content, password);
            }

            return key;
        }
        catch (Exception e) when (e is CryptographicException or ArgumentException)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(e, "Failed to load signing key");
            }

            throw new DocumentSignatureException("Failed to load certificate", e);
        }
    }

    private X509Certificate2 LoadCertificateByPath(string path, string? password = null)
    {
        try
        {
            var cert = new X509Certificate2(path, password);

            return cert;
        }
        catch (CryptographicException e)
        {
            if (m_logger.IsEnabled(LogLevel.Error))
            {
                m_logger.LogError(e, "Failed to load certificate");
            }

            throw new DocumentSignatureException("Failed to load certificate", e);
        }
    }
}