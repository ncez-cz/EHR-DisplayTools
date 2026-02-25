using AutoMapper;
using DisplayTool.DocSignAuthority.Service.Models;
using DisplayTool.DocSignAuthority.Service.Models.Contracts;

namespace DisplayTool.DocSignAuthority.Service.Profiles;

public class VerificationResultProfile : Profile
{
    public VerificationResultProfile()
    {
        CreateMap<DocumentSignatureVerificationResult, DocumentSignatureVerificationResponseContract>()
            .ForMember(dest => dest.IsValid, opt => opt.MapFrom(src => src.IsValid))
            .ForMember(dest => dest.SignedAt, opt => opt.MapFrom(src => src.SignedAt))
            .ForMember(dest => dest.Signor, opt => opt.MapFrom(src => src.Signor));
    }
}