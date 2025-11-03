using System.Diagnostics.CodeAnalysis;

namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public class DocumentSignatureValidationOperationResult
{
    public static DocumentSignatureValidationOperationResult Success(bool isValid, Guid reportId, Guid documentId)
    {
        return new DocumentSignatureValidationOperationResult(true, isValid, reportId, documentId);
    }

    public static DocumentSignatureValidationOperationResult Error()
    {
        return new DocumentSignatureValidationOperationResult(false, null, null, null);
    }

    private DocumentSignatureValidationOperationResult(
        bool operationSuccess,
        bool? isValid,
        Guid? reportId,
        Guid? documentId
    )
    {
        OperationSuccess = operationSuccess;
        IsValid = isValid;
        ReportId = reportId;
        DocumentId = documentId;
    }

    [MemberNotNullWhen(true, nameof(IsValid))]
    [MemberNotNullWhen(true, nameof(ReportId))]
    [MemberNotNullWhen(true, nameof(DocumentId))]
    public bool OperationSuccess { get; }

    public bool? IsValid { get; }


    public Guid? ReportId { get; }

    public Guid? DocumentId { get; }
}