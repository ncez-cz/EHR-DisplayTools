using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DisplayTool.DocSignAuthority.Service.Models.Contracts;

public class DocumentSignContract
{
    [Required] public required string Base64Document { get; set; }

    [Required]
    [JsonConverter(typeof(StringEnumConverter))]
    public required DocumentTypeContract DocumentType { get; set; }
}