using Hl7.Fhir.Model;

namespace DisplayTool.DocSignAuthority.Service.Models;

public class DocumentSignatureVerificationResult
{
    public required bool IsValid { get; set; }

    public DateTimeOffset? SignedAt { get; set; }

    public ResourceReference? Signor { get; set; }
}