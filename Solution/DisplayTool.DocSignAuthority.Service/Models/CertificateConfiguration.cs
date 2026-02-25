namespace DisplayTool.DocSignAuthority.Service.Models;

public class CertificateConfiguration
{
    public required string PrivateKeyPath { get; set; }
    public required string CertificatePath { get; set; }
    public string? Password { get; set; }
}