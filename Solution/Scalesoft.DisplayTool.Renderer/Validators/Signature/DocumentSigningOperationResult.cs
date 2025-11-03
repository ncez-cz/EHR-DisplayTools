using System.Diagnostics.CodeAnalysis;

namespace Scalesoft.DisplayTool.Renderer.Validators.Signature;

public class DocumentSigningOperationResult
{
    public static DocumentSigningOperationResult Success(byte[] signedDocument, Guid documentId)
    {
        return new DocumentSigningOperationResult(true, signedDocument, documentId);
    }

    public static DocumentSigningOperationResult Error()
    {
        return new DocumentSigningOperationResult(false, null, null);
    }

    private DocumentSigningOperationResult(bool operationSuccess, byte[]? signedDocument, Guid? documentId)
    {
        OperationSuccess = operationSuccess;
        SignedDocument = signedDocument;
        DocumentId = documentId;
    }

    [MemberNotNullWhen(true, nameof(SignedDocument))]
    [MemberNotNullWhen(true, nameof(DocumentId))]
    public bool OperationSuccess { get; }

    public byte[]? SignedDocument { get; }

    public Guid? DocumentId { get; }
}