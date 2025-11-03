namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public interface IDocumentSignatureValidationManager
{
    Task<DocumentSigningOperationResult> SignPdfFileAsync(byte[] document);

    Task<DocumentSignatureValidationOperationResult> ValidateAdesSignatureAsync(byte[] document);
}