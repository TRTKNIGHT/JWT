using AutoMapper;
using Demo0.DTOs;
using Demo0.Models.Entity;

namespace Demo0.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterDto, User>();

        CreateMap<User, ProfileDto>()
            .ForMember(pDto => pDto.Roles, r =>
                r.MapFrom(u =>
                    u.UserRoles.Select(ur => ur.Role.Name).ToList()
                )
            );

        CreateMap<UpdateDto, User>();
    }
}
