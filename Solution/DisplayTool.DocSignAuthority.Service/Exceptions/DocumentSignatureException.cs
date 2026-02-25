using DisplayTool.DocSignAuthority.Service.Models;

namespace DisplayTool.DocSignAuthority.Service.Exceptions;

public class DocumentSignatureException : Exception
{
    public readonly ErrorCode? ErrorCode;

    public DocumentSignatureException(string? message) : base(message)
    {
    }

    public DocumentSignatureException(string? message, ErrorCode errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public DocumentSignatureException(string? message, Exception innerException) : base(message, innerException)
    {
    }
}