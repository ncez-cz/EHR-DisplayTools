using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.DisplayTool.Shared.Signature;

namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public interface IFhirDocumentSignatureManager
{
    public SignatureProvider SignatureProvider { get; }

    Task<DocumentSignatureValidationOperationResult?> ValidateSignatureAsync(
        byte[] document,
        FhirDocumentFormat fhirDocumentFormat
    );
}