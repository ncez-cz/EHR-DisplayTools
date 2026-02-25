using System.ComponentModel.DataAnnotations;

namespace DisplayTool.DocSignAuthority.Service.Models.Contracts;

public class DocumentSignatureVerificationErrorContract
{
    [Required] public required ErrorCodeContract ErrorCode { get; set; }
}