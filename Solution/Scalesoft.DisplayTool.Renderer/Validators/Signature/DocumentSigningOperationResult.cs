using System.Diagnostics.CodeAnalysis;
using Scalesoft.DisplayTool.Shared.Signature;

namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public class DocumentSigningOperationResult
{
    public static DocumentSigningOperationResult Success(byte[] signedDocument)
    {
        return new DocumentSigningOperationResult(true, signedDocument);
    }

    public static DocumentSigningOperationResult Error(
        string? errorMsg,
        DocumentSignatureOperationErrorCode? errorCode = null
    )
    {
        return new DocumentSigningOperationResult(false, null, errorMsg: errorMsg, errorCode: errorCode);
    }

    private DocumentSigningOperationResult(
        bool operationSuccess,
        byte[]? signedDocument,
        string? errorMsg = null,
        DocumentSignatureOperationErrorCode? errorCode = null
    )
    {
        OperationSuccess = operationSuccess;
        SignedDocument = signedDocument;
        ErrorMsg = errorMsg;
        ErrorCode = errorCode;
    }

    [MemberNotNullWhen(true, nameof(SignedDocument))]
    public bool OperationSuccess { get; }

    public byte[]? SignedDocument { get; }

    public string? ErrorMsg { get; set; }

    public DocumentSignatureOperationErrorCode? ErrorCode { get; set; }
}