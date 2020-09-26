using CorePush.Google;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Service.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class MismeBackgroundService : IMismeBackgroundService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly IRewardHelper _rewardHelper;
        private readonly ISubscriptionService _subscriptionService;
        private readonly List<int> STREAK_REWARDS = new List<int> { 7, 30, 60, 90, 120 };

        public MismeBackgroundService(IUnitOfWork uow, IConfiguration config, IUserStatisticsService userStatisticsService,
            IRewardHelper rewardHelper, ISubscriptionService subscriptionService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _userStatisticsService = userStatisticsService ?? throw new ArgumentNullException(nameof(userStatisticsService));
            _rewardHelper = rewardHelper ?? throw new ArgumentNullException(nameof(rewardHelper));
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        }

        public async Task CleanExpiredTokensAsync()
        {
            var expiredTokens = await _uow.UserTokenRepository.GetAll().Where(t => DateTime.UtcNow > t.RefreshTokenExpiresDateTime).ToListAsync();
            foreach (var t in expiredTokens)
            {
                _uow.UserTokenRepository.Delete(t);
            }
            await _uow.CommitAsync();
        }

        public async Task RemoveDisabledAccountsAsync()
        {
            var dateToCompare = DateTime.UtcNow.AddDays(-30);
            var disabledUsers = await _uow.UserRepository.GetAll().Where(t => t.MarkedForDeletion == true &&
                                            t.DisabledAt.HasValue &&
                                            dateToCompare > t.DisabledAt.Value)
                                            .ToListAsync();
            foreach (var u in disabledUsers)
            {
                _uow.UserRepository.Delete(u);
            }
            await _uow.CommitAsync();
        }

        public async Task SendFireBaseNotificationsAsync()
        {
            var serverKey = _config["Firebase:ServerKey"];
            var senderId = _config["Firebase:SenderId"];

            var tokens = await _uow.DeviceRepository.GetAllAsync();
            foreach (var device in tokens)
            {
                using (var fcm = new FcmSender(serverKey, senderId))
                {
                    //if (device.Type == Data.Entities.Enums.DeviceTypeEnum.ANDROID)
                    //{
                    //var googleNot = new GoogleNotification();
                    //googleNot.Data = new GoogleNotification.DataPayload
                    //{
                    //    Message = "Testing"
                    //};

                    Message message = new Message()
                    {
                        Notification = new Notification
                        {
                            Title = "My push notification title",
                            Body = "Content for this push notification"
                        }
                        ,
                        //           Data = new Dictionary<string, string>()
                        //{
                        //    { "AdditionalData1", "data 1" },
                        //    { "AdditionalData2", "data 2" },
                        //    { "AdditionalData3", "data 3" },
                        //},
                        Token = device.Token
                    };

                    var response = await fcm.SendAsync(device.Token, message);
                    //}
                    //else
                    //{
                    //    var appleNot = new AppleNotification();
                    //    appleNot.Aps = new AppleNotification.ApsPayload
                    //    {
                    //        AlertBody = "Testing"
                    //    };
                    //    await fcm.SendAsync(device.Token, appleNot);
                    //}
                }
            }
        }

        public async Task SendFireBaseNotificationsRemindersAsync()
        {
            var serverKey = _config["Firebase:ServerKey"];
            var senderId = _config["Firebase:SenderId"];

            var tomorrow = DateTime.UtcNow.AddDays(1);
            var usersWithoutPlans = await _uow.UserRepository.GetAll()
                .Include(u => u.Eats)
                .Include(u => u.Devices)
                .Include(u => u.UserSettings)
                    .ThenInclude(s => s.Setting)
                .Where(u => !u.Eats.Any(e => e.CreatedAt.Date == tomorrow.Date))
                .ToListAsync();

            var reminder = await _uow.ReminderRepository.GetAll().Where(r => r.CodeName == RemindersConstants.NO_EAT_PLANNED_FOR_TOMORROW).FirstOrDefaultAsync();

            if (reminder != null)
            {
                foreach (var us in usersWithoutPlans)
                {
                    var lang = us.UserSettings.Where(us => us.Setting.Name == SettingsConstants.LANGUAGE).FirstOrDefault();
                    var language = "";

                    if (lang == null || string.IsNullOrWhiteSpace(lang.Value))
                    {
                        language = "ES";
                    }
                    else
                    {
                        language = lang.Value.ToUpper();
                    }

                    foreach (var device in us.Devices)
                    {
                        using (var fcm = new FcmSender(serverKey, senderId))
                        {
                            Message message = new Message()
                            {
                                Notification = new Notification
                                {
                                    Title = language == "EN" && !string.IsNullOrWhiteSpace(reminder.TitleEN) ? reminder.TitleEN :
                                    (language == "IT" && !string.IsNullOrWhiteSpace(reminder.TitleIT) ? reminder.TitleIT : reminder.Title),
                                    Body = language == "EN" && !string.IsNullOrWhiteSpace(reminder.BodyEN) ? reminder.BodyEN :
                                    (language == "IT" && !string.IsNullOrWhiteSpace(reminder.BodyIT) ? reminder.BodyIT : reminder.Body),
                                },
                                Token = device.Token
                            };
                            try
                            {
                                var response = await fcm.SendAsync(device.Token, message);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }

            //await RecurringJobSchedulerAsync();
        }

        public async Task HandleUserStreaksAsync(int timeOffsetRange)
        {
            var today = DateTime.UtcNow;

            Log.Information("HandleUserStreaksAsync start for " + timeOffsetRange.ToString());
            Log.Information("HandleUserStreaksAsync utc date: " + today.ToString());

            var query = _uow.UserRepository.GetAll()
                .Include(u => u.UserStatistics)
                .Include(u => u.Eats)
                .Include(u => u.Devices)
                .AsQueryable();

            if (timeOffsetRange == 1)
                query = query.Where(u => u.TimeZoneOffset >= 0);
            else
                query = query.Where(u => u.TimeZoneOffset < 0);

            var users = await query.ToListAsync();

            var userWithPlan = users.Where(u => u.Eats.Any(e => e.PlanCreatedAt.HasValue && e.PlanCreatedAt.Value.Date == today.Date));
            var userWithoutPlan = users.Where(u => !u.Eats.Any(e => e.PlanCreatedAt.HasValue && e.PlanCreatedAt.Value.Date == today.Date));

            foreach (var user in userWithPlan)
            {
                Log.Information("HandleUserStreaksAsync with-plan user: " + user.Email);

                var userStatistics = await _userStatisticsService.GetOrCreateUserStatisticsByUserAsync(user.Id);
                var balancedCurrentStreak = userStatistics.BalancedEatCurrentStreak;
                var eatCurrentStreak = userStatistics.EatCurrentStreak;

                var isBalanced = user.Eats.Any(e => (e.IsBalancedPlan.HasValue && e.IsBalancedPlan.Value) && (e.PlanCreatedAt.HasValue && e.PlanCreatedAt.Value.Date == today.Date));
                var streakType = isBalanced ? StreakEnum.BALANCED_EAT : StreakEnum.EAT;

                userStatistics = await _userStatisticsService.IncrementCurrentStreakAsync(userStatistics, streakType);

                if (eatCurrentStreak != userStatistics.EatCurrentStreak && STREAK_REWARDS.Any(s => s == userStatistics.EatCurrentStreak))
                {
                    /*Give reward for eat current streak*/
                    await _rewardHelper.HandleRewardAsync(RewardCategoryEnum.EAT_CREATED_STREAK, user.Id, true,
                        userStatistics.EatCurrentStreak, userStatistics.EatMaxStreak, NotificationTypeEnum.FIREBASE, user.Devices);

                    if (balancedCurrentStreak > 0)
                    {
                        await _userStatisticsService.CutCurrentStreakAsync(userStatistics, StreakEnum.BALANCED_EAT, user.Devices);
                    }
                }

                if (balancedCurrentStreak != userStatistics.BalancedEatCurrentStreak && STREAK_REWARDS.Any(s => s == userStatistics.BalancedEatCurrentStreak))
                {
                    /*Give reward for balanced eat current streak*/
                    await _rewardHelper.HandleRewardAsync(RewardCategoryEnum.EAT_CREATED_STREAK, user.Id, true,
                        userStatistics.BalancedEatCurrentStreak, userStatistics.BalancedEatMaxStreak, NotificationTypeEnum.FIREBASE, user.Devices);
                }

                /*Reward for creating an eat*/
                var rewardCategory = isBalanced ? RewardCategoryEnum.EAT_BALANCED_CREATED : RewardCategoryEnum.EAT_CREATED;
                await _rewardHelper.HandleRewardAsync(rewardCategory, user.Id, true, today, null, NotificationTypeEnum.FIREBASE, user.Devices);
                /*#end*/
            }

            //Cut current streak section

            foreach (var userNoPlan in userWithoutPlan)
            {
                Log.Information("HandleUserStreaksAsync with-no-plan user: " + userNoPlan.Email);

                var userStatistics = await _userStatisticsService.GetOrCreateUserStatisticsByUserAsync(userNoPlan.Id);
                var balancedCurrentStreak = userStatistics.BalancedEatCurrentStreak;
                var eatCurrentStreak = userStatistics.EatCurrentStreak;

                if (balancedCurrentStreak > 0)
                {
                    await _userStatisticsService.CutCurrentStreakAsync(userStatistics, StreakEnum.BALANCED_EAT, userNoPlan.Devices);
                    await _userStatisticsService.CutCurrentStreakAsync(userStatistics, StreakEnum.EAT, null);
                }
                else if (eatCurrentStreak > 0)
                {
                    await _userStatisticsService.CutCurrentStreakAsync(userStatistics, StreakEnum.EAT, userNoPlan.Devices);
                }
            }
        }

        public async Task HandleSubscriptionsAsync()
        {
            var userSubscriptions = await _uow.UserSubscriptionRepository.GetAllAsync();
            var today = DateTime.UtcNow;

            foreach (var userSubscription in userSubscriptions)
            {
                if (userSubscription.ValidAt.Date < today.Date)
                {
                    /*Extend the subscription to all users to prevent loose plany - TODO disable this when the requirement be requested*/
                    await _subscriptionService.AssignSubscriptionAsync(userSubscription.UserId, SubscriptionEnum.VIRTUAL_ASESSOR);

                    // Do not disable subscriptions now.
                    //await _subscriptionService.DisableUserSubscriptionAsync(userSubscription.Id);
                }
            }
        }
    }
}
