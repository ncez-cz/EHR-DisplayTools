using System.ComponentModel.DataAnnotations;

namespace DisplayTool.DocSignAuthority.Service.Models.Contracts;

public class DocumentSignResponseContract
{
    [Required] public required string Base64Document { get; set; }
}