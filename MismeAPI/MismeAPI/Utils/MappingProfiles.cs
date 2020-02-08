using AutoMapper;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities;

namespace MismeAPI.Utils
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<User, UserResponse>()
                        .ForMember(d => d.StatusId, opts => opts.MapFrom(source => (int)source.Status))
                        .ForMember(d => d.Status, opts => opts.MapFrom(source => source.Status.ToString()));
            CreateMap<PersonalData, PersonalDataResponse>()
                        .ForMember(d => d.TypeId, opts => opts.MapFrom(source => (int)source.Type))
                        .ForMember(d => d.Type, opts => opts.MapFrom(source => source.Type.ToString()));
            CreateMap<UserPersonalData, UserPersonalDataResponse>();
        }
    }
}