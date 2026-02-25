using Hl7.Fhir.Model;

namespace DisplayTool.DocSignAuthority.Service.Models.Contracts;

public class DocumentSignatureVerificationResponseContract
{
    public required bool IsValid { get; set; }

    public DateTimeOffset? SignedAt { get; set; }

    public ResourceReference? Signor { get; set; }
}