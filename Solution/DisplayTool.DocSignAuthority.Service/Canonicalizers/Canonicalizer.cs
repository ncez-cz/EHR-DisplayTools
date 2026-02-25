using DisplayTool.DocSignAuthority.Service.Models;

namespace DisplayTool.DocSignAuthority.Service.Canonicalizers;

public class Canonicalizer
{
    private readonly Dictionary<DocumentType, ICanonicalizer> m_canonicalizers;

    public Canonicalizer(IEnumerable<ICanonicalizer> canonicalizers)
    {
        m_canonicalizers = canonicalizers.ToDictionary(x => x.Type);
    }

    public CanonicalizationResult Canonicalize(string content, DocumentType type)
    {
        if (!m_canonicalizers.TryGetValue(type, out var canonicalizer))
        {
            throw new ArgumentException($"No canonicalizer for type {type}");
        }

        var canonicalized = canonicalizer.Canonicalize(content);

        return new CanonicalizationResult
        {
            CanonicalizedDocument = canonicalized,
            CanonicalizationMethod = canonicalizer.MethodUrl,
        };
    }
}

public class CanonicalizationResult
{
    public required string CanonicalizedDocument { get; set; }

    public required string CanonicalizationMethod { get; set; }
}