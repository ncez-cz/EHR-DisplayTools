using Scalesoft.DisplayTool.Shared.Configuration;

namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public interface IPdfSignatureManager
{
    public SignatureProvider SignatureProvider { get; }

    Task<DocumentSigningOperationResult> SignPdfFileAsync(byte[] document);
}