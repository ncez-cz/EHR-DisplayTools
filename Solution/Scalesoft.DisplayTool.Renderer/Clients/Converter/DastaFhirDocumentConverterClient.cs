using System.Net.Http.Headers;
using Scalesoft.DisplayTool.Renderer.Models.Enums;

namespace Scalesoft.DisplayTool.Renderer.Clients.Converter;

public class DastaFhirDocumentConverterClient
{
    private readonly HttpClient m_client;

    public DastaFhirDocumentConverterClient(HttpClient client)
    {
        m_client = client;
    }

    public async Task<byte[]> Convert(byte[] fileContent, DocumentType documentType)
    {
        var transformation = GetConverterName(documentType);
        var uri = $"Transform/{transformation}";
        var content = new ByteArrayContent(fileContent);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        
        var response = await m_client.PostAsync(uri, content);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsByteArrayAsync();
    }

    private string GetConverterName(DocumentType documentType)
    {
        return documentType switch
        {
            DocumentType.PatientSummary => "dasta2fhir_patsum_EU",
            DocumentType.DischargeReport => "dasta2fhir_hdr_EU",
            DocumentType.Laboratory => "dasta2fhir_laboratory_EU",
            DocumentType.ImagingOrder => throw new NotImplementedException("Imaging order is not supported yet"),
            DocumentType.ImagingReport => throw new NotImplementedException("Imaging report is not supported yet"),
            DocumentType.LaboratoryOrder => "dasta2fhir_laboratoryOrder_EU",
            DocumentType.AnyBundle => throw new NotImplementedException("General bundles are only applicable to FHIR."),
            _ => throw new ArgumentOutOfRangeException(nameof(documentType), documentType, null),
        };
    }
}
