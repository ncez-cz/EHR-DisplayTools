using System.Runtime.Serialization;

namespace DisplayTool.DocSignAuthority.Service.Models.Contracts;

public enum DocumentTypeContract
{
    [EnumMember(Value = "FhirJson")] FhirJson,
    [EnumMember(Value = "FhirXml")] FhirXml,
}