using System.Runtime.Serialization;

namespace DisplayTool.DocSignAuthority.Service.Models.Contracts;

public enum ErrorCodeContract
{
    [EnumMember(Value = "NoSignature")] NoSignature,

    [EnumMember(Value = "UnsupportedDocumentType")]
    UnsupportedDocumentType,
    [EnumMember(Value = "Other")] Other,
}