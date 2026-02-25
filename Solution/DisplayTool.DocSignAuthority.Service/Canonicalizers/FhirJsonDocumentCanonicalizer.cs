using DisplayTool.DocSignAuthority.Service.Models;
using Org.Webpki.JsonCanonicalizer;

namespace DisplayTool.DocSignAuthority.Service.Canonicalizers;

public class FhirJsonDocumentCanonicalizer : ICanonicalizer
{
    public DocumentType Type => DocumentType.FhirJson;
    public string MethodUrl => "http://hl7.org/fhir/canonicalization/json";

    public string Canonicalize(string json)
    {
        var jsonCanonicalizer = new JsonCanonicalizer(json);

        return jsonCanonicalizer.GetEncodedString();
    }
}