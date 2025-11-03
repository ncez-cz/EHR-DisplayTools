namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public class NullDocumentSignatureValidationManager : IDocumentSignatureValidationManager
{
    public Task<DocumentSigningOperationResult> SignPdfFileAsync(byte[] document)
    {
        return Task.FromResult(DocumentSigningOperationResult.Success([], Guid.Empty));
    }

    public Task<DocumentSignatureValidationOperationResult> ValidateAdesSignatureAsync(byte[] document)
    {
        return Task.FromResult(DocumentSignatureValidationOperationResult.Success(true, Guid.Empty, Guid.Empty));
    }
}