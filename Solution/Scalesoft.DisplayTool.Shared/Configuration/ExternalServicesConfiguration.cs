using System.ComponentModel.DataAnnotations;

namespace Scalesoft.DisplayTool.Shared.Configuration;

public class ExternalServicesConfiguration
{
    public required TranslationSourceConfiguration TranslationSource { get; init; }
    public required DocumentConverterConfiguration DocumentConverter { get; init; }
    public required DocumentValidationConfiguration DocumentValidation { get; init; }
    public required DocumentSignatureValidationConfiguration DocumentSignatureValidation { get; init; }
}

public class TranslationSourceConfiguration
{
    public string? BaseUrl { get; init; }
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

public class DocumentSignatureValidationConfiguration
{
    // If no url is specified, disable signing / signature validation
    [Required] public string? EZCAIIBaseUrl { get; init; }
    [Required] public string? ApiCallerName { get; init; }
    [Required] public Guid? CertificateId { get; init; }
    public string? EncryptedCertificatePassword { get; init; }
    [Required] public Guid? StorageId { get; init; }
}