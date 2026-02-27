using System.ComponentModel.DataAnnotations;

namespace Scalesoft.DisplayTool.Shared.Configuration;

public class ExternalServicesConfiguration
{
    public required DocumentConverterConfiguration DocumentConverter { get; init; }
    public required DocumentValidationConfiguration DocumentValidation { get; init; }
    public required DocumentSignatureConfiguration DocumentSignature { get; init; }
}

public class DocumentConverterConfiguration
{
    public required string BaseUrl { get; init; }
    public bool UseConverterForPatientSummary { get; init; }
}

public class DocumentValidationConfiguration
{
    // If any validator url is not specified, an internal validator is used instead.
    public string? FhirBaseUrl { get; init; }
    public string? CdaBaseUrl { get; init; }
}

public class DocumentSignatureConfiguration
{
    public required SignatureProvider PdfSigningProvider { get; init; }
    public required SignatureProvider FhirDocumentProvider { get; init; }
    public EZCAIIDocumentSignatureConfiguration? EZCAIIConfiguration { get; init; }
    public PoCSigningAuthorityDocumentSignatureConfiguration? PoCSigningAuthorityConfiguration { get; init; }
}

public class EZCAIIDocumentSignatureConfiguration
{
    [Required] public required string BaseUrl { get; init; }
    [Required] public required string ApiCallerName { get; init; }
    [Required] public Guid CertificateId { get; init; }
    public string? EncryptedCertificatePassword { get; init; }
    [Required] public Guid StorageId { get; init; }
}

public class PoCSigningAuthorityDocumentSignatureConfiguration
{
    [Required] public required string BaseUrl { get; init; }
}