using MismeAPI.Data.Entities;
using MismeAPI.Data.Repository;
using System;
using System.Threading.Tasks;

namespace MismeAPI.Data.UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MismeContext _context;

        private bool disposed = false;

        public UnitOfWork(MismeContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            UserRepository = UserRepository ?? new GenericRepository<User>(_context);
            UserTokenRepository = UserTokenRepository ?? new GenericRepository<UserToken>(_context);
            PollRepository = PollRepository ?? new GenericRepository<Poll>(_context);
            AnswerRepository = AnswerRepository ?? new GenericRepository<Answer>(_context);
            QuestionRepository = QuestionRepository ?? new GenericRepository<Question>(_context);
            UserAnswerRepository = UserAnswerRepository ?? new GenericRepository<UserAnswer>(_context);
            ConceptRepository = ConceptRepository ?? new GenericRepository<Concept>(_context);
            UserConceptRepository = UserConceptRepository ?? new GenericRepository<UserConcept>(_context);
            TagRepository = TagRepository ?? new GenericRepository<Tag>(_context);
            DishRepository = DishRepository ?? new GenericRepository<Dish>(_context);
            DishTagRepository = DishTagRepository ?? new GenericRepository<DishTag>(_context);
            EatRepository = EatRepository ?? new GenericRepository<Eat>(_context);
            EatDishRepository = EatDishRepository ?? new GenericRepository<EatDish>(_context);
            DeviceRepository = DeviceRepository ?? new GenericRepository<Device>(_context);
            TipRepository = TipRepository ?? new GenericRepository<Tip>(_context);
            ReminderRepository = ReminderRepository ?? new GenericRepository<Reminder>(_context);
            SettingRepository = SettingRepository ?? new GenericRepository<Setting>(_context);
            UserSettingRepository = UserSettingRepository ?? new GenericRepository<UserSetting>(_context);
            ResultRepository = ResultRepository ?? new GenericRepository<Result>(_context);
            GeneralContentRepository = GeneralContentRepository ?? new GenericRepository<GeneralContent>(_context);
            ContactUsRepository = ContactUsRepository ?? new GenericRepository<ContactUs>(_context);
            RewardCategoryRepository = RewardCategoryRepository ?? new GenericRepository<RewardCategory>(_context);
            RewardAcumulateRepository = RewardAcumulateRepository ?? new GenericRepository<RewardAcumulate>(_context);
            RewardHistoryRepository = RewardHistoryRepository ?? new GenericRepository<RewardHistory>(_context);
            UserStatisticsRepository = UserStatisticsRepository ?? new GenericRepository<UserStatistics>(_context);
            DishCompoundDishRepository = DishCompoundDishRepository ?? new GenericRepository<DishCompoundDish>(_context);
            CompoundDishRepository = CompoundDishRepository ?? new GenericRepository<CompoundDish>(_context);
            CutPointRepository = CutPointRepository ?? new GenericRepository<CutPoint>(_context);
            UserReferralRepository = UserReferralRepository ?? new GenericRepository<UserReferral>(_context);
            EatCompoundDishRepository = EatCompoundDishRepository ?? new GenericRepository<EatCompoundDish>(_context);
            ScheduleRepository = ScheduleRepository ?? new GenericRepository<Schedule>(_context);
            EatScheduleRepository = EatScheduleRepository ?? new GenericRepository<EatSchedule>(_context);
            UserScheduleRepository = UserScheduleRepository ?? new GenericRepository<UserSchedule>(_context);
            SoloQuestionRepository = SoloQuestionRepository ?? new GenericRepository<SoloQuestion>(_context);
            SoloAnswerRepository = SoloAnswerRepository ?? new GenericRepository<SoloAnswer>(_context);
            UserSoloAnswerRepository = UserSoloAnswerRepository ?? new GenericRepository<UserSoloAnswer>(_context);
            SubscriptionRepository = SubscriptionRepository ?? new GenericRepository<Subscription>(_context);
            UserSubscriptionRepository = UserSubscriptionRepository ?? new GenericRepository<UserSubscription>(_context);
            UserSubscriptionScheduleRepository = UserSubscriptionScheduleRepository ?? new GenericRepository<UserSubscriptionSchedule>(_context);
            AppRepository = AppRepository ?? new GenericRepository<App>(_context);
            FavoriteDishRepository = FavoriteDishRepository ?? new GenericRepository<FavoriteDish>(_context);
            LackSelfControlDishRepository = LackSelfControlDishRepository ?? new GenericRepository<LackSelfControlDish>(_context);
            HandConversionFactorRepository = HandConversionFactorRepository ?? new GenericRepository<HandConversionFactor>(_context);
            FavoriteCompoundDishRepository = FavoriteCompoundDishRepository ?? new GenericRepository<FavoriteCompoundDishes>(_context);
            LackSelfControlCompoundDishRepository = LackSelfControlCompoundDishRepository ?? new GenericRepository<LackSelfControlCompoundDish>(_context);
            ProductRepository = ProductRepository ?? new GenericRepository<Product>(_context);
            OrderRepository = OrderRepository ?? new GenericRepository<Order>(_context);
            GroupRepository = GroupRepository ?? new GenericRepository<Group>(_context);
            GroupInvitationRepository = GroupInvitationRepository ?? new GenericRepository<GroupInvitation>(_context);
            PersonalDataRepository = PersonalDataRepository ?? new GenericRepository<PersonalData>(_context);
            MenuRepository = MenuRepository ?? new GenericRepository<Menu>(_context);
            MenuEatRepository = MenuEatRepository ?? new GenericRepository<MenuEat>(_context);
            MenuEatDishRepository = MenuEatDishRepository ?? new GenericRepository<MenuEatDish>(_context);
            MenuEatCompoundDishRepository = MenuEatCompoundDishRepository ?? new GenericRepository<MenuEatCompoundDish>(_context);
            ScheduledEmailsRepository = ScheduledEmailsRepository ?? new GenericRepository<ScheduledEmail>(_context);
        }

        public IGenericRepository<User> UserRepository { get; set; }
        public IGenericRepository<UserToken> UserTokenRepository { get; set; }

        public IGenericRepository<Poll> PollRepository { get; set; }
        public IGenericRepository<Answer> AnswerRepository { get; set; }
        public IGenericRepository<Question> QuestionRepository { get; set; }
        public IGenericRepository<UserAnswer> UserAnswerRepository { get; set; }
        public IGenericRepository<Concept> ConceptRepository { get; set; }
        public IGenericRepository<UserConcept> UserConceptRepository { get; set; }
        public IGenericRepository<Tag> TagRepository { get; set; }
        public IGenericRepository<Dish> DishRepository { get; set; }
        public IGenericRepository<DishTag> DishTagRepository { get; set; }
        public IGenericRepository<Eat> EatRepository { get; set; }
        public IGenericRepository<EatDish> EatDishRepository { get; set; }
        public IGenericRepository<Device> DeviceRepository { get; set; }
        public IGenericRepository<Tip> TipRepository { get; set; }
        public IGenericRepository<Reminder> ReminderRepository { get; set; }
        public IGenericRepository<Setting> SettingRepository { get; set; }
        public IGenericRepository<UserSetting> UserSettingRepository { get; set; }
        public IGenericRepository<Result> ResultRepository { get; set; }
        public IGenericRepository<GeneralContent> GeneralContentRepository { get; set; }
        public IGenericRepository<ContactUs> ContactUsRepository { get; set; }
        public IGenericRepository<RewardCategory> RewardCategoryRepository { get; set; }
        public IGenericRepository<RewardAcumulate> RewardAcumulateRepository { get; set; }
        public IGenericRepository<RewardHistory> RewardHistoryRepository { get; set; }
        public IGenericRepository<UserStatistics> UserStatisticsRepository { get; set; }
        public IGenericRepository<CompoundDish> CompoundDishRepository { get; set; }
        public IGenericRepository<DishCompoundDish> DishCompoundDishRepository { get; set; }
        public IGenericRepository<CutPoint> CutPointRepository { get; set; }
        public IGenericRepository<UserReferral> UserReferralRepository { get; set; }
        public IGenericRepository<EatCompoundDish> EatCompoundDishRepository { get; set; }
        public IGenericRepository<Schedule> ScheduleRepository { get; set; }
        public IGenericRepository<EatSchedule> EatScheduleRepository { get; set; }
        public IGenericRepository<UserSchedule> UserScheduleRepository { get; set; }
        public IGenericRepository<SoloQuestion> SoloQuestionRepository { get; set; }
        public IGenericRepository<SoloAnswer> SoloAnswerRepository { get; set; }
        public IGenericRepository<UserSoloAnswer> UserSoloAnswerRepository { get; set; }
        public IGenericRepository<Subscription> SubscriptionRepository { get; set; }
        public IGenericRepository<UserSubscription> UserSubscriptionRepository { get; set; }
        public IGenericRepository<UserSubscriptionSchedule> UserSubscriptionScheduleRepository { get; set; }
        public IGenericRepository<FavoriteDish> FavoriteDishRepository { get; set; }
        public IGenericRepository<App> AppRepository { get; set; }
        public IGenericRepository<HandConversionFactor> HandConversionFactorRepository { get; set; }
        public IGenericRepository<LackSelfControlDish> LackSelfControlDishRepository { get; set; }
        public IGenericRepository<Product> ProductRepository { get; set; }
        public IGenericRepository<Order> OrderRepository { get; set; }
        public IGenericRepository<FavoriteCompoundDishes> FavoriteCompoundDishRepository { get; set; }
        public IGenericRepository<LackSelfControlCompoundDish> LackSelfControlCompoundDishRepository { get; set; }
        public IGenericRepository<Group> GroupRepository { get; set; }
        public IGenericRepository<GroupInvitation> GroupInvitationRepository { get; set; }
        public IGenericRepository<PersonalData> PersonalDataRepository { get; set; }
        public IGenericRepository<Menu> MenuRepository { get; set; }
        public IGenericRepository<MenuEat> MenuEatRepository { get; set; }
        public IGenericRepository<MenuEatDish> MenuEatDishRepository { get; set; }
        public IGenericRepository<MenuEatCompoundDish> MenuEatCompoundDishRepository { get; set; }
        public IGenericRepository<ScheduledEmail> ScheduledEmailsRepository { get; set; }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                this.disposed = true;
            }
        }
    }
}
