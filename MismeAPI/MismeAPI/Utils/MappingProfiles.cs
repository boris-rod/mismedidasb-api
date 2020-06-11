using AutoMapper;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.CompoundDish;
using MismeAPI.Common.DTO.Response.ContactUs;
using MismeAPI.Common.DTO.Response.CutPoint;
using MismeAPI.Common.DTO.Response.GeneralContent;
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

namespace MismeAPI.Utils
{
    public class MappingProfiles : Profile
    {
        private readonly IAmazonS3Service _amazonS3Service;
        private readonly IUserStatisticsService _userStatisticsService;
        //private readonly IHttpContextAccessor _httpContextAccessor;

        public MappingProfiles()
        {
        }

        public MappingProfiles(IAmazonS3Service amazonS3Service, IUserStatisticsService userStatisticsService)
        {
            _amazonS3Service = amazonS3Service ?? throw new ArgumentNullException(nameof(amazonS3Service));
            _userStatisticsService = userStatisticsService ?? throw new ArgumentNullException(nameof(userStatisticsService));
            //_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(amazonS3Service));

            CreateMap<User, UserResponse>()
                .ForMember(d => d.Language, opts => opts.MapFrom(source => GetLanguage(source.UserSettings)))
                        .ForMember(d => d.StatusId, opts => opts.MapFrom(source => (int)source.Status))
                        .ForMember(d => d.Status, opts => opts.MapFrom(source => source.Status.ToString()))
                        .ForMember(d => d.Avatar, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Avatar) ? "" : _amazonS3Service.GetPresignUrl(source.Avatar)));

            CreateMap<User, UserWithSubscriptionResponse>()
                .ForMember(d => d.Language, opts => opts.MapFrom(source => GetLanguage(source.UserSettings)))
                        .ForMember(d => d.StatusId, opts => opts.MapFrom(source => (int)source.Status))
                        .ForMember(d => d.Status, opts => opts.MapFrom(source => source.Status.ToString()))
                        .ForMember(d => d.Avatar, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Avatar) ? "" : _amazonS3Service.GetPresignUrl(source.Avatar)));

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
                 .ForMember(d => d.LastAnswer, opts => opts.MapFrom((src, dest, destMember, context) => GetLastAnswer(src, (Dictionary<int, int>)context.Items["dict"])))
                 .ForMember(d => d.Title, opts => opts.MapFrom((src, dest, destMember, context) => GetQuestionTitle(src, context.Items["lang"].ToString())))
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
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPresignUrl(source.Image)));

            CreateMap<Concept, ConceptAdminResponse>()
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPresignUrl(source.Image)));

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
                 .ForMember(d => d.Name, opts => opts.MapFrom((src, dest, destMember, context) => GetDishName(src, context.Items["lang"].ToString())))
                .ForMember(d => d.Tags, opts => opts.MapFrom(source => source.DishTags))
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPresignUrl(source.Image)));

            CreateMap<Dish, DishAdminResponse>();
            //.ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPresignUrl(source.Image)));
            CreateMap<Reminder, ReminderAdminResponse>();
            CreateMap<Result, ResultAdminResponse>();

            CreateMap<DishTag, TagResponse>()
                .ForMember(d => d.Name, opts => opts.MapFrom(source => source.Tag.Name))
                .ForMember(d => d.Id, opts => opts.MapFrom(source => source.TagId))
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
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPresignUrl(source.Image)))
                .ForMember(d => d.DishCompoundDishResponse, opts => opts.MapFrom(source => source.DishCompoundDishes))
                .ForMember(d => d.Calcium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Calcium * d.DishQty) : 0))
                .ForMember(d => d.Calories, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Calories * d.DishQty) : 0))
                .ForMember(d => d.Carbohydrates, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Carbohydrates * d.DishQty) : 0))
                .ForMember(d => d.Cholesterol, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Cholesterol * d.DishQty) : 0))
                .ForMember(d => d.Fat, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Fat * d.DishQty) : 0))
                .ForMember(d => d.Fiber, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Fiber * d.DishQty) : 0))
                .ForMember(d => d.FolicAcid, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.FolicAcid * d.DishQty) : 0))
                .ForMember(d => d.Iron, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Iron * d.DishQty) : 0))
                .ForMember(d => d.Magnesium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Magnesium * d.DishQty) : 0))
                .ForMember(d => d.Niacin, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Niacin * d.DishQty) : 0))
                .ForMember(d => d.Phosphorus, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Phosphorus * d.DishQty) : 0))
                .ForMember(d => d.Potassium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Potassium * d.DishQty) : 0))
                .ForMember(d => d.Proteins, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Proteins * d.DishQty) : 0))
                .ForMember(d => d.Ribofla, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Ribofla * d.DishQty) : 0))
                .ForMember(d => d.Sodium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Sodium * d.DishQty) : 0))
                .ForMember(d => d.Thiamine, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Thiamine * d.DishQty) : 0))
                .ForMember(d => d.VitaminA, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.VitaminA * d.DishQty) : 0))
                .ForMember(d => d.VitaminB12, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.VitaminB12 * d.DishQty) : 0))
                .ForMember(d => d.VitaminB6, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.VitaminB6 * d.DishQty) : 0))
                .ForMember(d => d.VitaminC, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.VitaminC * d.DishQty) : 0))
                .ForMember(d => d.Zinc, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Zinc * d.DishQty) : 0));

            CreateMap<CompoundDish, AdminCompoundDishResponse>()
                .ForMember(d => d.Username, opts => opts.MapFrom(source => source.CreatedBy.Username))
                .ForMember(d => d.Fullname, opts => opts.MapFrom(source => source.CreatedBy.FullName))
                .ForMember(d => d.Email, opts => opts.MapFrom(source => source.CreatedBy.Email))
                .ForMember(d => d.Image, opts => opts.MapFrom(source => string.IsNullOrWhiteSpace(source.Image) ? "" : _amazonS3Service.GetPresignUrl(source.Image)))
                .ForMember(d => d.DishCompoundDishResponse, opts => opts.MapFrom(source => source.DishCompoundDishes))
                .ForMember(d => d.Calcium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Calcium * d.DishQty) : 0))
                .ForMember(d => d.Calories, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Calories * d.DishQty) : 0))
                .ForMember(d => d.Carbohydrates, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Carbohydrates * d.DishQty) : 0))
                .ForMember(d => d.Cholesterol, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Cholesterol * d.DishQty) : 0))
                .ForMember(d => d.Fat, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Fat * d.DishQty) : 0))
                .ForMember(d => d.Fiber, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Fiber * d.DishQty) : 0))
                .ForMember(d => d.FolicAcid, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.FolicAcid * d.DishQty) : 0))
                .ForMember(d => d.Iron, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Iron * d.DishQty) : 0))
                .ForMember(d => d.Magnesium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Magnesium * d.DishQty) : 0))
                .ForMember(d => d.Niacin, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Niacin * d.DishQty) : 0))
                .ForMember(d => d.Phosphorus, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Phosphorus * d.DishQty) : 0))
                .ForMember(d => d.Potassium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Potassium * d.DishQty) : 0))
                .ForMember(d => d.Proteins, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Proteins * d.DishQty) : 0))
                .ForMember(d => d.Ribofla, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Ribofla * d.DishQty) : 0))
                .ForMember(d => d.Sodium, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Sodium * d.DishQty) : 0))
                .ForMember(d => d.Thiamine, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Thiamine * d.DishQty) : 0))
                .ForMember(d => d.VitaminA, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.VitaminA * d.DishQty) : 0))
                .ForMember(d => d.VitaminB12, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.VitaminB12 * d.DishQty) : 0))
                .ForMember(d => d.VitaminB6, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.VitaminB6 * d.DishQty) : 0))
                .ForMember(d => d.VitaminC, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.VitaminC * d.DishQty) : 0))
                .ForMember(d => d.Zinc, opts => opts.MapFrom(source => source.DishCompoundDishes.Count > 0 ? source.DishCompoundDishes.Sum(d => d.Dish.Zinc * d.DishQty) : 0));

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
        }

        private int GetLastAnswer(Question src, Dictionary<int, int> dictionary)
        {
            var value = 0;
            dictionary.TryGetValue(src.Id, out value);
            return value;
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
    }
}
