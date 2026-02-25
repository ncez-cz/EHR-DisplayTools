using DisplayTool.DocSignAuthority.Service.Models;

namespace DisplayTool.DocSignAuthority.Service.Canonicalizers;

public interface ICanonicalizer
{
    public string Canonicalize(string content);
    public DocumentType Type { get; }

    public string MethodUrl { get; }
}