﻿using MismeAPI.Data.Entities;
using MismeAPI.Data.Repository;
using System;
using System.Threading.Tasks;

namespace MismeAPI.Data.UoW
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> UserRepository { get; set; }
        IGenericRepository<UserToken> UserTokenRepository { get; set; }
        IGenericRepository<Poll> PollRepository { get; set; }
        IGenericRepository<Question> QuestionRepository { get; set; }
        IGenericRepository<Answer> AnswerRepository { get; set; }
        IGenericRepository<UserAnswer> UserAnswerRepository { get; set; }
        IGenericRepository<Concept> ConceptRepository { get; set; }
        IGenericRepository<UserConcept> UserConceptRepository { get; set; }
        IGenericRepository<Tag> TagRepository { get; set; }
        IGenericRepository<Dish> DishRepository { get; set; }
        IGenericRepository<DishTag> DishTagRepository { get; set; }
        IGenericRepository<Eat> EatRepository { get; set; }
        IGenericRepository<EatDish> EatDishRepository { get; set; }
        IGenericRepository<Device> DeviceRepository { get; set; }
        IGenericRepository<Tip> TipRepository { get; set; }
        IGenericRepository<Reminder> ReminderRepository { get; set; }
        IGenericRepository<Setting> SettingRepository { get; set; }
        IGenericRepository<UserSetting> UserSettingRepository { get; set; }
        IGenericRepository<Result> ResultRepository { get; set; }
        IGenericRepository<GeneralContent> GeneralContentRepository { get; set; }
        IGenericRepository<ContactUs> ContactUsRepository { get; set; }
        IGenericRepository<RewardCategory> RewardCategoryRepository { get; set; }
        IGenericRepository<RewardAcumulate> RewardAcumulateRepository { get; set; }
        IGenericRepository<RewardHistory> RewardHistoryRepository { get; set; }
        IGenericRepository<UserStatistics> UserStatisticsRepository { get; set; }
        IGenericRepository<CompoundDish> CompoundDishRepository { get; set; }
        IGenericRepository<DishCompoundDish> DishCompoundDishRepository { get; set; }
        IGenericRepository<CutPoint> CutPointRepository { get; set; }
        IGenericRepository<UserReferral> UserReferralRepository { get; set; }
        IGenericRepository<EatCompoundDish> EatCompoundDishRepository { get; set; }
        IGenericRepository<Schedule> ScheduleRepository { get; set; }
        IGenericRepository<EatSchedule> EatScheduleRepository { get; set; }
        IGenericRepository<UserSchedule> UserScheduleRepository { get; set; }
        IGenericRepository<SoloQuestion> SoloQuestionRepository { get; set; }
        IGenericRepository<SoloAnswer> SoloAnswerRepository { get; set; }
        IGenericRepository<UserSoloAnswer> UserSoloAnswerRepository { get; set; }
        IGenericRepository<Subscription> SubscriptionRepository { get; set; }
        IGenericRepository<UserSubscription> UserSubscriptionRepository { get; set; }
        IGenericRepository<UserSubscriptionSchedule> UserSubscriptionScheduleRepository { get; set; }
        IGenericRepository<App> AppRepository { get; set; }
        IGenericRepository<FavoriteDish> FavoriteDishRepository { get; set; }
        IGenericRepository<LackSelfControlDish> LackSelfControlDishRepository { get; set; }
        IGenericRepository<HandConversionFactor> HandConversionFactorRepository { get; set; }
        IGenericRepository<Product> ProductRepository { get; set; }
        IGenericRepository<Order> OrderRepository { get; set; }
        IGenericRepository<FavoriteCompoundDishes> FavoriteCompoundDishRepository { get; set; }
        IGenericRepository<LackSelfControlCompoundDish> LackSelfControlCompoundDishRepository { get; set; }
        IGenericRepository<Group> GroupRepository { get; set; }
        IGenericRepository<GroupInvitation> GroupInvitationRepository { get; set; }
        IGenericRepository<PersonalData> PersonalDataRepository { get; set; }
        IGenericRepository<Menu> MenuRepository { get; set; }
        IGenericRepository<MenuEat> MenuEatRepository { get; set; }
        IGenericRepository<MenuEatDish> MenuEatDishRepository { get; set; }
        IGenericRepository<MenuEatCompoundDish> MenuEatCompoundDishRepository { get; set; }
        IGenericRepository<ScheduledEmail> ScheduledEmailsRepository { get; set; }

        Task<int> CommitAsync();
    }
}
