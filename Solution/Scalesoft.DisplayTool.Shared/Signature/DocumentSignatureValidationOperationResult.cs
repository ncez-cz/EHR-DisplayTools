using System.Diagnostics.CodeAnalysis;

namespace Scalesoft.DisplayTool.Shared.Signature;

public class DocumentSignatureValidationOperationResult
{
    public static DocumentSignatureValidationOperationResult Success(
        bool isValid,
        DateTimeOffset? signedAt = null,
        string? signor = null
    )
    {
        return new DocumentSignatureValidationOperationResult(true, isValid, signedAt, signor);
    }

    public static DocumentSignatureValidationOperationResult Error(
        string? errorMsg,
        DocumentSignatureOperationErrorCode? errorCode = null
    )
    {
        return new DocumentSignatureValidationOperationResult(false, null, errorMsg: errorMsg, errorCode: errorCode);
    }

    private DocumentSignatureValidationOperationResult(
        bool operationSuccess,
        bool? isValid,
        DateTimeOffset? signedAt = null,
        string? signor = null,
        string? errorMsg = null,
        DocumentSignatureOperationErrorCode? errorCode = null
    )
    {
        OperationSuccess = operationSuccess;
        IsValid = isValid;
        SignedAt = signedAt;
        Signor = signor;
        ErrorMsg = errorMsg;
        ErrorCode = errorCode;
    }

    [MemberNotNullWhen(true, nameof(IsValid))]
    public bool OperationSuccess { get; }

    public bool? IsValid { get; }

    public DateTimeOffset? SignedAt { get; }

    public string? Signor { get; }

    public string? ErrorMsg { get; }

    public DocumentSignatureOperationErrorCode? ErrorCode { get; set; }
}