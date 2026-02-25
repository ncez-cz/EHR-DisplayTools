using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DisplayTool.DocSignAuthority.Service.Models;
using Hl7.Fhir.Model;

namespace DisplayTool.DocSignAuthority.Service.FhirManipulation;

public interface IDocumentFormatManipulator
{
    public RootResourceResult? SelectRootElement(string documentContent);

    public Signature GetSignature(
        string payload,
        string canonicalizationUrl,
        RSA privateKey,
        X509Certificate2 certificate,
        DateTime signingTime
    );

    public DocumentType Type { get; }

    public string ReplaceRootBundlePlaceholder(string rootResourceContent, string bundleIdLookup, Bundle resultBundle);

    public string? GetEncapsulatedBundleSignature(string documentContent);

    public string? GetEncapsulatedBundleProvenanceTarget(string documentContent);

    public DocumentSignatureVerificationResult Validate(string signatureTarget, string signatureContent);

    public IntegratedSignaturePreprocessResult SelectIntegratedSignatureResourceParts(string rootResourceContent);

    public string AddMetaIdSignature(string payload, Provenance provenance, object? metaContent, object? idContent);
}