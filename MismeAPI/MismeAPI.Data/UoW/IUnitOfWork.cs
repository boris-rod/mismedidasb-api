using MismeAPI.Data.Entities;
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

        Task<int> CommitAsync();
    }
}
