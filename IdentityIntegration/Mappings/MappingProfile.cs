using AutoMapper;
using IdentityIntegration.Dtos;
using Microsoft.AspNetCore.Identity;

namespace IdentityIntegration.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterDto, IdentityUser>()
            .ForMember(u => u.PasswordHash,
                opt => opt.Ignore()
            );
    }
}
