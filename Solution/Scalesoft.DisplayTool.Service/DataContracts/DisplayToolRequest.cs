using System.ComponentModel.DataAnnotations;

namespace Scalesoft.DisplayTool.Service.DataContracts;

public class DisplayToolRequest
{
    [Required]
    public required byte[] FileContent { get; set; }

    [Required]
    public InputFormatContract? InputFormat { get; set; }

    [Required]
    public OutputFormatContract? OutputFormat { get; set; }
    
    [Required]
    public bool IsEmbeddable { get; set; }
    
    public bool ValidateDocument { get; set; }

    public bool ValidateCodeValues { get; set; }

    public bool ValidateDigitalSignature { get; set; }

    public bool PreferTranslationsFromDocument { get; set; } = false;
}
