using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using DisplayTool.DocSignAuthority.Service.Models;
using DisplayTool.DocSignAuthority.Service.Models.Contracts;

namespace DisplayTool.DocSignAuthority.Service.Profiles;

public class ErrorCodeProfile : Profile
{
    public ErrorCodeProfile()
    {
        CreateMap<ErrorCodeContract, ErrorCode>().ConvertUsingEnumMapping(opt => opt
                .MapValue(ErrorCodeContract.NoSignature, ErrorCode.NoSignature)
                .MapValue(ErrorCodeContract.UnsupportedDocumentType, ErrorCode.UnsupportedDocumentType)
                .MapValue(ErrorCodeContract.Other, ErrorCode.Other))
            .ReverseMap();
    }
}