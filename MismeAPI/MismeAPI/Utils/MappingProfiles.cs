using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Group;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.CompoundDish;
using MismeAPI.Common.DTO.Response.ContactUs;
using MismeAPI.Common.DTO.Response.CutPoint;
using MismeAPI.Common.DTO.Response.GeneralContent;
using MismeAPI.Common.DTO.Response.Order;
using MismeAPI.Common.DTO.Response.PersonalData;
using MismeAPI.Common.DTO.Response.Product;
using MismeAPI.Common.DTO.Response.Reminder;
using MismeAPI.Common.DTO.Response.Result;
using MismeAPI.Common.DTO.Response.Reward;
using MismeAPI.Common.DTO.Response.Settings;
using MismeAPI.Common.DTO.Response.SoloQuestion;
using MismeAPI.Common.DTO.Response.Subscription;
using MismeAPI.Common.DTO.Response.User;
using MismeAPI.Common.DTO.Response.UserStatistics;
using MismeAPI.Data.Entities;
using MismeAPI.Service;
using MismeAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MismeAPI.Utils
{
    public class MappingProfiles : Profile
    {
        private readonly IAmazonS3Service _amazonS3Service;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MappingProfiles()
        {
        }

        public MappingProfiles(IAmazonS3Service amazonS3Service, IUserStatisticsService userStatisticsService, IHttpContextAccessor httpContextAccessor)
        {
            _amazonS3Service = amazonS3Service ?? throw new ArgumentNullException(nameof(amazonS3Service));
            _userStatisticsService = userStatisticsService ?? throw new ArgumentNullException(nameof(userStatisticsService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(amazonS3Service));

            CreateMap<User, UserResponse>()
                .ForMember(d => d.Language, opts => opts.MapFrom(source => GetLanguage(source.UserSettings)))
                        .ForMember(d => d.StatusId, opts => opts.MapFrom(source => (int)source.Status))
                        .ForMember(d => d.Status, opts => opts.MapFrom(source => source.Status.ToString()))
                        .ForMember(d => d.PlannedEats, opts => opts.MapFrom(source => GetPlannedEats(source)))
                        .ForMember(d => d.EmotionMedia, opts => opts.MapFrom(source => GetEmotionMedia(source)))
                        .ForMember(d => d.Avatar, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Avatar) ? "" : _amazonS3Service.GetPublicUrl(source.Avatar)));

            CreateMap<User, UserWithSubscriptionResponse>()
                .ForMember(d => d.Language, opts => opts.MapFrom(source => GetLanguage(source.UserSettings)))
                        .ForMember(d => d.StatusId, opts => opts.MapFrom(source => (int)source.Status))
                        .ForMember(d => d.Status, opts => opts.MapFrom(source => source.Status.ToString()))
                        .ForMember(d => d.Avatar, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Avatar) ? "" : _amazonS3Service.GetPublicUrl(source.Avatar)));

            CreateMap<User, UserAdminResponse>();

            CreateMap<Poll, PollResponse>()
                //.ForMember(d => d.Tips, opts => opts.MapFrom((src, dest, destMember, context) => GetPollTips(src, context.Items["lang"].ToString())))
                .ForMember(d => d.HtmlContent, opts => opts.MapFrom((src, dest, destMember, context) => GetPollHTMLContent(src, context.Items["lang"].ToString())))
                .ForMember(d => d.Description, opts => opts.MapFrom((src, dest, destMember, context) => GetPollDescription(src, context.Items["lang"].ToString())))
                .ForMember(d => d.Name, opts => opts.MapFrom((src, dest, destMember, context) => GetPollName(src, context.Items["lang"].ToString())))
                .ForMember(d => d.Questions, opts => opts.MapFrom(source => source.Questions.OrderBy(q => q.Order)))
                .ForMember(d => d.Tips, opts => opts.MapFrom(source => source.Tips));
            CreateMap<Poll, PollAdminResponse>();

            CreateMap<Question, QuestionResponse>()
                 .ForMember(d => d.LastAnswer, opts => opts.MapFrom((src, dest, destMember, context) => GetLastAnswer(src, context.Items)))
                 .ForMember(d => d.Title, opts => opts.MapFrom((src, dest, destMember, context) => GetQuestionTitle(src, GetLanguageInMapProp(context.Items))))
                 .ForMember(d => d.Answers, opts => opts.MapFrom(source => source.Answers.OrderBy(q => q.Order)));

            CreateMap<Question, QuestionAdminResponse>()
                 .ForMember(d => d.PollName, opts => opts.MapFrom(source => source.Poll.Name));

            CreateMap<Answer, AnswerResponse>()
                 .ForMember(d => d.Title, opts => opts.MapFrom((src, dest, destMember, context) => GetAnswerTitle(src, context.Items["lang"].ToString())));
            CreateMap<Answer, AnswerAdminResponse>()
                 .ForMember(d => d.PollName, opts => opts.MapFrom(source => source.Question.Poll.Name));

            CreateMap<Concept, ConceptResponse>()
                .ForMember(d => d.Title, opts => opts.MapFrom((src, dest, destMember, context) => GetConceptTitle(src, context.Items["lang"].ToString())))
                .ForMember(d => d.Instructions, opts => opts.MapFrom((src, dest, destMember, context) => GetConceptInstructions(src, context.Items["lang"].ToString())))
                .ForMember(d => d.Description, opts => opts.MapFrom((src, dest, destMember, context) => GetConceptDescription(src, context.Items["lang"].ToString())))
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPublicUrl(source.Image)));

            CreateMap<Concept, ConceptAdminResponse>()
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPublicUrl(source.Image)));

            CreateMap<Tag, TagResponse>();
            CreateMap<Tip, TipResponse>()
                .ForMember(d => d.Content, opts => opts.MapFrom((src, dest, destMember, context) => GetTipContent(src, context.Items["lang"].ToString())))
                .ForMember(d => d.TipPosition, opts => opts.MapFrom(source => (int)source.TipPosition))
                .ForMember(d => d.TipPositionString, opts => opts.MapFrom(source => source.TipPosition.ToString()));
            CreateMap<Tip, TipAdminResponse>()
                .ForMember(d => d.PollName, opts => opts.MapFrom(source => source.Poll != null ? source.Poll.Name : ""))
                .ForMember(d => d.TipPosition, opts => opts.MapFrom(source => (int)source.TipPosition))
                .ForMember(d => d.TipPositionString, opts => opts.MapFrom(source => source.TipPosition.ToString()));

            CreateMap<Dish, DishResponse>()
                 .ForMember(d => d.Calcium, opts => opts.MapFrom(src => src.Calcium ?? 0.0))
                 .ForMember(d => d.Calories, opts => opts.MapFrom(src => src.Calories ?? 0.0))
                 .ForMember(d => d.Carbohydrates, opts => opts.MapFrom(src => src.Carbohydrates ?? 0.0))
                 .ForMember(d => d.Cholesterol, opts => opts.MapFrom(src => src.Cholesterol ?? 0.0))
                 .ForMember(d => d.Fat, opts => opts.MapFrom(src => src.Fat ?? 0.0))
                 .ForMember(d => d.Fiber, opts => opts.MapFrom(src => src.Fiber ?? 0.0))
                 .ForMember(d => d.Iron, opts => opts.MapFrom(src => src.Iron ?? 0.0))
                 .ForMember(d => d.MonoUnsaturatedFat, opts => opts.MapFrom(src => src.MonoUnsaturatedFat ?? 0.0))
                 .ForMember(d => d.NetWeight, opts => opts.MapFrom(src => src.NetWeight ?? 0.0))
                 .ForMember(d => d.Phosphorus, opts => opts.MapFrom(src => src.Phosphorus ?? 0.0))
                 .ForMember(d => d.PolyUnsaturatedFat, opts => opts.MapFrom(src => src.PolyUnsaturatedFat ?? 0.0))
                 .ForMember(d => d.Potassium, opts => opts.MapFrom(src => src.Potassium ?? 0.0))
                 .ForMember(d => d.Proteins, opts => opts.MapFrom(src => src.Proteins ?? 0.0))
                 .ForMember(d => d.SaturatedFat, opts => opts.MapFrom(src => src.SaturatedFat ?? 0.0))
                 .ForMember(d => d.Sodium, opts => opts.MapFrom(src => src.Sodium ?? 0.0))
                 .ForMember(d => d.VitaminA, opts => opts.MapFrom(src => src.VitaminA ?? 0.0))
                 .ForMember(d => d.VitaminB12, opts => opts.MapFrom(src => src.VitaminB12 ?? 0.0))
                 .ForMember(d => d.VitaminB1Thiamin, opts => opts.MapFrom(src => src.VitaminB1Thiamin ?? 0.0))
                 .ForMember(d => d.VitaminB2Riboflavin, opts => opts.MapFrom(src => src.VitaminB2Riboflavin ?? 0.0))
                 .ForMember(d => d.VitaminB3Niacin, opts => opts.MapFrom(src => src.VitaminB3Niacin ?? 0.0))
                 .ForMember(d => d.VitaminB6, opts => opts.MapFrom(src => src.VitaminB6 ?? 0.0))
                 .ForMember(d => d.VitaminB9Folate, opts => opts.MapFrom(src => src.VitaminB9Folate ?? 0.0))
                 .ForMember(d => d.VitaminC, opts => opts.MapFrom(src => src.VitaminC ?? 0.0))
                 .ForMember(d => d.VitaminD, opts => opts.MapFrom(src => src.VitaminD ?? 0.0))
                 .ForMember(d => d.VitaminE, opts => opts.MapFrom(src => src.VitaminE ?? 0.0))
                 .ForMember(d => d.VitaminK, opts => opts.MapFrom(src => src.VitaminK ?? 0.0))
                 .ForMember(d => d.Volume, opts => opts.MapFrom(src => src.Volume ?? 0.0))
                 .ForMember(d => d.Zinc, opts => opts.MapFrom(src => src.Zinc ?? 0.0))
                 .ForMember(d => d.Alcohol, opts => opts.MapFrom(src => src.Alcohol ?? 0.0))
                 .ForMember(d => d.Name, opts => opts.MapFrom((src, dest, destMember, context) => GetDishName(src, GetLanguageInMapProp(context.Items))))
                 .ForMember(d => d.IsFavorite, opts => opts.MapFrom(source => IsFavorite(source)))
                 .ForMember(d => d.IsLackSelfControlDish, opts => opts.MapFrom(source => IsLackSelfControl(source)))
                 .ForMember(d => d.LackSelfControlDishIntensity, opts => opts.MapFrom(source => LackSelfControlIntensity(source)))
                 .ForMember(d => d.Tags, opts => opts.MapFrom(source => source.DishTags))
                 .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPublicUrl(source.Image)));

            CreateMap<Dish, DishAdminResponse>();
            //.ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPublicUrl(source.Image)));
            CreateMap<Reminder, ReminderAdminResponse>();
            CreateMap<Result, ResultAdminResponse>();

            CreateMap<DishTag, TagResponse>()
                .ForMember(d => d.Name, opts => opts.MapFrom(source => source.Tag.Name))
                .ForMember(d => d.Id, opts => opts.MapFrom(source => source.TagId));

            CreateMap<EatDish, EatDishResponse>()
                .ForMember(d => d.Qty, opts => opts.MapFrom(source => source.Qty))
                .ForMember(d => d.Dish, opts => opts.MapFrom(source => source.Dish));

            CreateMap<EatCompoundDish, EatCompoundDishResponse>()
                .ForMember(d => d.Qty, opts => opts.MapFrom(source => source.Qty))
                .ForMember(d => d.CompoundDish, opts => opts.MapFrom(source => source.CompoundDish));

            CreateMap<Eat, EatResponse>()
                       .ForMember(d => d.EatTypeId, opts => opts.MapFrom(source => (int)source.EatType))
                       .ForMember(d => d.EatType, opts => opts.MapFrom(source => source.EatType.ToString()))
                       .ForMember(d => d.EatDishResponse, opts => opts.MapFrom(source => source.EatDishes))
                       .ForMember(d => d.KCal, opts => opts.MapFrom(source => source.KCalAtThatMoment))
                       .ForMember(d => d.IMC, opts => opts.MapFrom(source => source.ImcAtThatMoment))
                       .ForMember(d => d.IsBalanced, opts => opts.MapFrom(source => source.IsBalanced))
                       .ForMember(d => d.EatCompoundDishResponse, opts => opts.MapFrom(source => source.EatCompoundDishes));

            CreateMap<UserSetting, BasicSettingResponse>()
                   .ForMember(d => d.Setting, opts => opts.MapFrom(source => source.Setting.Name))
                   .ForMember(d => d.SettingId, opts => opts.MapFrom(source => source.SettingId))
                   .ForMember(d => d.Value, opts => opts.MapFrom(source => source.Value));

            CreateMap<Setting, ListSettingResponse>();

            CreateMap<GeneralContent, GeneralContentResponse>()
                .ForMember(d => d.Content, opts => opts.MapFrom((src, dest, destMember, context) => GetGeneralContent(src, context.Items["lang"].ToString())))
               .ForMember(d => d.ContentTypeId, opts => opts.MapFrom(source => (int)source.ContentType))
               .ForMember(d => d.ContentType, opts => opts.MapFrom(source => source.ContentType.ToString()));

            CreateMap<GeneralContent, GeneralContentAdminResponse>()
                .ForMember(d => d.ContentTypeId, opts => opts.MapFrom(source => (int)source.ContentType))
               .ForMember(d => d.ContentType, opts => opts.MapFrom(source => source.ContentType.ToString()));

            CreateMap<ContactUs, ContactUsResponse>()
                .ForMember(d => d.UserEmail, opts => opts.MapFrom(source => source.User.Email))
                .ForMember(d => d.UserName, opts => opts.MapFrom(source => source.User.FullName))
                .ForMember(d => d.PriorityId, opts => opts.MapFrom(source => (int)source.Priority))
               .ForMember(d => d.Priority, opts => opts.MapFrom(source => source.Priority.ToString()));

            CreateMap<RewardHistory, RewardResponse>()
                .ForMember(d => d.CategoryId, opts => opts.MapFrom(source => (int)source.RewardCategory.Category))
                .ForMember(d => d.Category, opts => opts.MapFrom(source => source.RewardCategory.Category.ToString()));
            CreateMap<UserStatistics, UserStatisticsResponse>()
                .ForMember(d => d.PersonalRanking, opts => opts.MapFrom(source => _userStatisticsService.GetUserRankingAsync(source.UserId).Result));

            CreateMap<DishCompoundDish, DishCompoundDishResponse>()
              .ForMember(d => d.Qty, opts => opts.MapFrom(source => source.DishQty))
              .ForMember(d => d.Dish, opts => opts.MapFrom(source => source.Dish));
            CreateMap<CompoundDish, CompoundDishResponse>()
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPublicUrl(source.Image)))
                .ForMember(d => d.DishCompoundDishResponse, opts => opts.MapFrom(source => source.DishCompoundDishes))
                .ForMember(d => d.Calcium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Calcium ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Calories, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Calories ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Carbohydrates, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Carbohydrates ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Cholesterol, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Cholesterol ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Fat, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Fat ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Fiber, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Fiber ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Iron, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Iron ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Phosphorus, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Phosphorus ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Potassium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Potassium ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Proteins, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Proteins ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Sodium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Sodium ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminA, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminA ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB12, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB12 ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB6, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB6 ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminC, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminC ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Zinc, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Zinc ?? 0.0) * d.DishQty) : 0.0))

                .ForMember(d => d.MonoUnsaturatedFat, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.MonoUnsaturatedFat ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.NetWeight, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.NetWeight ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.PolyUnsaturatedFat, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.PolyUnsaturatedFat ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.SaturatedFat, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.SaturatedFat ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB1Thiamin, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB1Thiamin ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB2Riboflavin, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB2Riboflavin ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB3Niacin, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB3Niacin ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB9Folate, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB9Folate ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminD, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminD ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminE, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminE ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminK, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminK ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Alcohol, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Alcohol ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Volume, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Volume ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.IsFavorite, opts => opts.MapFrom(source => IsFavorite(source)))
                .ForMember(d => d.IsLackSelfControlDish, opts => opts.MapFrom(source => IsLackSelfControl(source)))
                .ForMember(d => d.LackSelfControlDishIntensity, opts => opts.MapFrom(source => LackSelfControlIntensity(source)));

            CreateMap<CompoundDish, AdminCompoundDishResponse>()
                .ForMember(d => d.Username, opts => opts.MapFrom(source => source.CreatedBy.Username))
                .ForMember(d => d.Fullname, opts => opts.MapFrom(source => source.CreatedBy.FullName))
                .ForMember(d => d.Email, opts => opts.MapFrom(source => source.CreatedBy.Email))
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPublicUrl(source.Image)))
                .ForMember(d => d.DishCompoundDishResponse, opts => opts.MapFrom(source => source.DishCompoundDishes))
                .ForMember(d => d.Calcium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Calcium ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Calories, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Calories ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Carbohydrates, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Carbohydrates ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Cholesterol, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Cholesterol ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Fat, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Fat ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Fiber, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Fiber ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Iron, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Iron ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Phosphorus, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Phosphorus ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Potassium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Potassium ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Proteins, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Proteins ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Sodium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Sodium ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminA, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminA ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB12, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB12 ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB6, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB6 ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminC, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminC ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Zinc, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Zinc ?? 0.0) * d.DishQty) : 0.0))

                .ForMember(d => d.MonoUnsaturatedFat, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.MonoUnsaturatedFat ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.NetWeight, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.NetWeight ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.PolyUnsaturatedFat, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.PolyUnsaturatedFat ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.SaturatedFat, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.SaturatedFat ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB1Thiamin, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB1Thiamin ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB2Riboflavin, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB2Riboflavin ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB3Niacin, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB3Niacin ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminB9Folate, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminB9Folate ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminD, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminD ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminE, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminE ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.VitaminK, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.VitaminK ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Alcohol, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Alcohol ?? 0.0) * d.DishQty) : 0.0))
                .ForMember(d => d.Volume, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => (d.Dish.Volume ?? 0.0) * d.DishQty) : 0.0));

            CreateMap<CutPoint, CutPointResponse>();

            CreateMap<UserReferral, UserReferralResponse>();

            CreateMap<UserSoloAnswer, UserSoloAnswerResponse>();
            CreateMap<SoloQuestion, SoloQuestionResponse>();
            CreateMap<SoloAnswer, SoloAnswerResponse>();

            CreateMap<Subscription, SubscriptionResponse>();
            CreateMap<UserSubscription, UserSubscriptionResponse>()
                .ForMember(d => d.ProductId, opts => opts.MapFrom(source => source.Subscription != null ? (int)source.Subscription.Product : -1))
                .ForMember(d => d.Product, opts => opts.MapFrom(source => source.Subscription != null ? source.Subscription.Product.ToString() : ""))
                .ForMember(d => d.Name, opts => opts.MapFrom(source => source.Subscription != null ? source.Subscription.Name : ""));

            CreateMap<App, AppResponse>();

            CreateMap<HealthReportEntry, HealthCheckResponse>()
             .ForMember(d => d.Description, opts => opts.MapFrom(source => source.Description))
             .ForMember(d => d.Duration, opts => opts.MapFrom(source => source.Duration.TotalSeconds))
             .ForMember(d => d.ServiceStatus, opts => opts.MapFrom(source => source.Status == HealthStatus.Healthy ?
                                                                             HttpStatusCode.OK :
                                                                             (source.Status == HealthStatus.Degraded ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable)))
             .ForMember(d => d.Exception, opts => opts.MapFrom(source => source.Exception == null ? "" : source.Exception.Message));

            CreateMap<Product, ProductResponse>()
                       .ForMember(d => d.TypeId, opts => opts.MapFrom(source => (int)source.Type))
                       .ForMember(d => d.Type, opts => opts.MapFrom(source => source.Type.ToString()));

            CreateMap<Order, OrderResponse>()
                       .ForMember(d => d.StatusId, opts => opts.MapFrom(source => (int)source.Status))
                       .ForMember(d => d.Status, opts => opts.MapFrom(source => source.Status.ToString()))
                       .ForMember(d => d.PaymentMethodId, opts => opts.MapFrom(source => (int)source.PaymentMethod))
                       .ForMember(d => d.PaymentMethod, opts => opts.MapFrom(source => source.PaymentMethod.ToString()));

            CreateMap<Group, GroupResponse>()
                .ForMember(d => d.UsersCount, opts => opts.MapFrom(source => source.Users.Count()));
            CreateMap<Group, GroupExtendedResponse>();
            CreateMap<GroupInvitation, GroupInvitationResponse>()
                .ForMember(d => d.StatusId, opts => opts.MapFrom(source => (int)source.Status))
                .ForMember(d => d.Status, opts => opts.MapFrom(source => source.Status.ToString()));

            CreateMap<PersonalData, PersonalDataResponse>()
                .ForMember(d => d.KeyId, opts => opts.MapFrom(source => (int)source.Key))
                .ForMember(d => d.Key, opts => opts.MapFrom(source => source.Key.ToString()));
        }

        private int GetLastAnswer(Question src, IDictionary<string, object> items = null)
        {
            try
            {
                var dictionary = (Dictionary<int, int>)items["dict"];
                var value = 0;
                dictionary.TryGetValue(src.Id, out value);
                return value;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private string GetGeneralContent(GeneralContent src, string lang)
        {
            switch (lang)
            {
                case "EN":
                    return string.IsNullOrWhiteSpace(src.ContentEN) ? src.Content : src.ContentEN;

                case "IT":
                    return string.IsNullOrWhiteSpace(src.ContentIT) ? src.Content : src.ContentIT;

                default:
                    return src.Content;
            }
        }

        private string GetQuestionTitle(Question src, string lang)
        {
            switch (lang)
            {
                case "EN":
                    return string.IsNullOrWhiteSpace(src.TitleEN) ? src.Title : src.TitleEN;

                case "IT":
                    return string.IsNullOrWhiteSpace(src.TitleIT) ? src.Title : src.TitleIT;

                default:
                    return src.Title;
            }
        }

        private string GetTipContent(Tip src, string lang)
        {
            switch (lang)
            {
                case "EN":
                    return string.IsNullOrWhiteSpace(src.ContentEN) ? src.Content : src.ContentEN;

                case "IT":
                    return string.IsNullOrWhiteSpace(src.ContentIT) ? src.Content : src.ContentIT;

                default:
                    return src.Content;
            }
        }

        private string GetPollHTMLContent(Poll src, string lang)
        {
            switch (lang)
            {
                case "EN":
                    return string.IsNullOrWhiteSpace(src.HtmlContentEN) ? src.HtmlContent : src.HtmlContentEN;

                case "IT":
                    return string.IsNullOrWhiteSpace(src.HtmlContentIT) ? src.HtmlContent : src.HtmlContentIT;

                default:
                    return src.HtmlContent;
            }
        }

        private string GetPollDescription(Poll src, string lang)
        {
            switch (lang)
            {
                case "EN":
                    return string.IsNullOrWhiteSpace(src.DescriptionEN) ? src.Description : src.DescriptionEN;

                case "IT":
                    return string.IsNullOrWhiteSpace(src.DescriptionIT) ? src.Description : src.DescriptionIT;

                default:
                    return src.Description;
            }
        }

        private string GetPollName(Poll src, string lang)
        {
            switch (lang)
            {
                case "EN":
                    return string.IsNullOrWhiteSpace(src.NameEN) ? src.Name : src.NameEN;

                case "IT":
                    return string.IsNullOrWhiteSpace(src.NameIT) ? src.Name : src.NameIT;

                default:
                    return src.Name;
            }
        }

        private string GetAnswerTitle(Answer src, string lang)
        {
            switch (lang)
            {
                case "EN":
                    return string.IsNullOrWhiteSpace(src.TitleEN) ? src.Title : src.TitleEN;

                case "IT":
                    return string.IsNullOrWhiteSpace(src.TitleIT) ? src.Title : src.TitleIT;

                default:
                    return src.Title;
            }
        }

        private string GetDishName(Dish src, string lang)
        {
            switch (lang)
            {
                case "EN":
                    return string.IsNullOrWhiteSpace(src.NameEN) ? src.Name : src.NameEN;

                case "IT":
                    return string.IsNullOrWhiteSpace(src.NameIT) ? src.Name : src.NameIT;

                default:
                    return src.Name;
            }
        }

        private string GetConceptTitle(Concept src, string lang)
        {
            switch (lang)
            {
                case "EN":
                    return string.IsNullOrWhiteSpace(src.TitleEN) ? src.Title : src.TitleEN;

                case "IT":
                    return string.IsNullOrWhiteSpace(src.TitleIT) ? src.Title : src.TitleIT;

                default:
                    return src.Title;
            }
        }

        private string GetConceptDescription(Concept src, string lang)
        {
            switch (lang)
            {
                case "EN":
                    return string.IsNullOrWhiteSpace(src.DescriptionEN) ? src.Description : src.DescriptionEN;

                case "IT":
                    return string.IsNullOrWhiteSpace(src.DescriptionIT) ? src.Description : src.DescriptionIT;

                default:
                    return src.Description;
            }
        }

        private string GetConceptInstructions(Concept src, string lang)
        {
            switch (lang)
            {
                case "EN":
                    return string.IsNullOrWhiteSpace(src.InstructionsEN) ? src.Instructions : src.DescriptionEN;

                case "IT":
                    return string.IsNullOrWhiteSpace(src.InstructionsIT) ? src.Instructions : src.InstructionsIT;

                default:
                    return src.Instructions;
            }
        }

        private string GetLanguage(ICollection<UserSetting> userSettings)
        {
            if (userSettings.Count == 0)
            {
                return "ES";
            }
            else
            {
                var lang = userSettings.Where(u => u.Setting.Name == SettingsConstants.LANGUAGE).FirstOrDefault();
                if (lang != null)
                {
                    return string.IsNullOrWhiteSpace(lang.Value) ? "ES" : lang.Value;
                }
                else
                {
                    return "ES";
                }
            }
        }

        private string GetLanguageInMapProp(IDictionary<string, object> items)
        {
            try
            {
                var lang = items["lang"];
                return lang.ToString();
            }
            catch (Exception)
            {
                return "ES";
            }
        }

        private bool IsFavorite(Dish dish)
        {
            var loggedUser = _httpContextAccessor.CurrentUser();

            return dish.FavoriteDishes.Any(fd => fd.UserId == loggedUser);
        }

        private bool IsFavorite(CompoundDish dish)
        {
            var loggedUser = _httpContextAccessor.CurrentUser();

            return dish.FavoriteDishes.Any(fd => fd.UserId == loggedUser);
        }

        private bool IsLackSelfControl(Dish dish)
        {
            var loggedUser = _httpContextAccessor.CurrentUser();

            return dish.LackSelfControlDishes.Any(fd => fd.UserId == loggedUser);
        }

        private bool IsLackSelfControl(CompoundDish dish)
        {
            var loggedUser = _httpContextAccessor.CurrentUser();

            return dish.LackSelfControlDishes.Any(fd => fd.UserId == loggedUser);
        }

        private int LackSelfControlIntensity(Dish dish)
        {
            var loggedUser = _httpContextAccessor.CurrentUser();

            var item = dish.LackSelfControlDishes.FirstOrDefault(fd => fd.UserId == loggedUser);

            return item != null ? item.Intensity : 0;
        }

        private int LackSelfControlIntensity(CompoundDish dish)
        {
            var loggedUser = _httpContextAccessor.CurrentUser();

            var item = dish.LackSelfControlDishes.FirstOrDefault(fd => fd.UserId == loggedUser);

            return item != null ? item.Intensity : 0;
        }

        private double GetEmotionMedia(User user)
        {
            var media = user.UserSoloAnswers
                .Where(usa => usa.QuestionCode == "SQ-2" && usa.AnswerCode == "SQ-2-SA-1" && !string.IsNullOrEmpty(usa.AnswerValue)).Average(usa => int.Parse(usa.AnswerValue));

            return media;
        }

        private int GetPlannedEats(User user)
        {
            return user.UserStatistics.TotalBalancedEatsPlanned + user.UserStatistics.TotalNonBalancedEatsPlanned;
        }
    }
}
