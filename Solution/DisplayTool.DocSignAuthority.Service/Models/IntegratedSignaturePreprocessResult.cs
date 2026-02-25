namespace DisplayTool.DocSignAuthority.Service.Models;

public class IntegratedSignaturePreprocessResult
{
    public required object? MetaNode { get; set; }
    public required object? IdNode { get; set; }
    public required string? SignatureContent { get; set; }
    public required string ContentToSign { get; set; }
}