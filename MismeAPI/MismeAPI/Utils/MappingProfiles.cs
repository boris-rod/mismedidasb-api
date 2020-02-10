using AutoMapper;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities;
using MismeAPI.Services;
using System;

namespace MismeAPI.Utils
{
    public class MappingProfiles : Profile
    {
        private readonly IAmazonS3Service _amazonS3Service;

        public MappingProfiles()
        {
        }

        public MappingProfiles(IAmazonS3Service amazonS3Service)
        {
            _amazonS3Service = amazonS3Service ?? throw new ArgumentNullException(nameof(amazonS3Service));
            CreateMap<User, UserResponse>()
                        .ForMember(d => d.StatusId, opts => opts.MapFrom(source => (int)source.Status))
                        .ForMember(d => d.Status, opts => opts.MapFrom(source => source.Status.ToString()))
                        .ForMember(d => d.Avatar, opts => opts.MapFrom(source => String.IsNullOrWhiteSpace(source.Avatar) ? "" : _amazonS3Service.GetPresignUrl(source.Avatar)));
            CreateMap<PersonalData, PersonalDataResponse>()
                        .ForMember(d => d.TypeId, opts => opts.MapFrom(source => (int)source.Type))
                        .ForMember(d => d.Type, opts => opts.MapFrom(source => source.Type.ToString()));
            CreateMap<UserPersonalData, UserPersonalDataResponse>();
            CreateMap<Poll, PollResponse>();
            CreateMap<Question, QuestionResponse>();
            CreateMap<Concept, ConceptResponse>();
        }
    }
}