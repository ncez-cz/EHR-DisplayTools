using System.ComponentModel.DataAnnotations;

namespace DisplayTool.DocSignAuthority.Service.Models.Contracts;

public class FileDocumentSignContract
{
    [Required] public required IFormFile File { get; set; }

    [Required] public required DocumentTypeContract DocumentType { get; set; }
}