using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.DisplayTool.Shared.Signature;

namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public class NullSignatureManager : IPdfSignatureManager, IFhirDocumentSignatureManager
{
    public SignatureProvider SignatureProvider => SignatureProvider.None;

    public Task<DocumentSigningOperationResult> SignPdfFileAsync(byte[] document)
    {
        return Task.FromResult(DocumentSigningOperationResult.Success(document));
    }

    public Task<DocumentSignatureValidationOperationResult?> ValidateSignatureAsync(
        byte[] document,
        FhirDocumentFormat fhirDocumentFormat
    )
    {
        return Task.FromResult<DocumentSignatureValidationOperationResult?>(null);
    }
}