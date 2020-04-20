using AutoMapper;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities;
using MismeAPI.Services;
using System;
using System.Linq;

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
                        .ForMember(d => d.Avatar, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Avatar) ? "" : _amazonS3Service.GetPresignUrl(source.Avatar)));

            CreateMap<Poll, PollResponse>()
                .ForMember(d => d.Questions, opts => opts.MapFrom(source => source.Questions.OrderBy(q => q.Order)))
                .ForMember(d => d.Tips, opts => opts.MapFrom(source => source.Tips));
            CreateMap<Poll, PollAdminResponse>();

            CreateMap<Question, QuestionResponse>()
                 .ForMember(d => d.Answers, opts => opts.MapFrom(source => source.Answers.OrderBy(q => q.Order)));

            CreateMap<Question, QuestionAdminResponse>()
                 .ForMember(d => d.PollName, opts => opts.MapFrom(source => source.Poll.Name));

            CreateMap<Answer, AnswerResponse>();
            CreateMap<Answer, AnswerAdminResponse>()
                 .ForMember(d => d.PollName, opts => opts.MapFrom(source => source.Question.Poll.Name));

            CreateMap<Concept, ConceptResponse>()
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPresignUrl(source.Image)));

            CreateMap<Concept, ConceptAdminResponse>()
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPresignUrl(source.Image)));

            CreateMap<Tag, TagResponse>();
            CreateMap<Tip, TipResponse>()
                .ForMember(d => d.TipPosition, opts => opts.MapFrom(source => (int)source.TipPosition))
                .ForMember(d => d.TipPositionString, opts => opts.MapFrom(source => source.TipPosition.ToString()));
            CreateMap<Tip, TipAdminResponse>()
                .ForMember(d => d.PollName, opts => opts.MapFrom(source => source.Poll != null ? source.Poll.Name : ""))
                .ForMember(d => d.TipPosition, opts => opts.MapFrom(source => (int)source.TipPosition))
                .ForMember(d => d.TipPositionString, opts => opts.MapFrom(source => source.TipPosition.ToString()));

            CreateMap<Dish, DishResponse>()
                .ForMember(d => d.Tags, opts => opts.MapFrom(source => source.DishTags))
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPresignUrl(source.Image)));

            CreateMap<DishTag, TagResponse>()
                .ForMember(d => d.Name, opts => opts.MapFrom(source => source.Tag.Name))
                .ForMember(d => d.Id, opts => opts.MapFrom(source => source.TagId))
                .ForMember(d => d.Id, opts => opts.MapFrom(source => source.TagId));

            CreateMap<EatDish, EatDishResponse>()
                .ForMember(d => d.Qty, opts => opts.MapFrom(source => source.Qty))
                .ForMember(d => d.Dish, opts => opts.MapFrom(source => source.Dish));

            CreateMap<Eat, EatResponse>()
                       .ForMember(d => d.EatTypeId, opts => opts.MapFrom(source => (int)source.EatType))
                       .ForMember(d => d.EatType, opts => opts.MapFrom(source => source.EatType.ToString()))
                       .ForMember(d => d.EatDishResponse, opts => opts.MapFrom(source => source.EatDishes));
        }
    }
}