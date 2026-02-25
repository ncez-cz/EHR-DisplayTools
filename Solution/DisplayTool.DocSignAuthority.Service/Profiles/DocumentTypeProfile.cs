using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using DisplayTool.DocSignAuthority.Service.Models;
using DisplayTool.DocSignAuthority.Service.Models.Contracts;

namespace DisplayTool.DocSignAuthority.Service.Profiles;

public class DocumentTypeProfile : Profile
{
    public DocumentTypeProfile()
    {
        CreateMap<DocumentTypeContract, DocumentType>().ConvertUsingEnumMapping(opt =>
            opt.MapValue(DocumentTypeContract.FhirJson, DocumentType.FhirJson)
                .MapValue(DocumentTypeContract.FhirXml, DocumentType.FhirXml)).ReverseMap();
    }
}