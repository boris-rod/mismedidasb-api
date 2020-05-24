using Microsoft.Extensions.Configuration;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Utils
{
    public class BackgroundJobProcessor : IBackgroundJobProcessor
    {
        private readonly IConfiguration _config;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly IUserService _userService;
        private readonly IRewardHelper _rewardHelper;
        private readonly List<int> STREAK_REWARDS = new List<int> { 7, 30, 60, 90, 120 };

        public BackgroundJobProcessor(IConfiguration config, IUserStatisticsService userStatisticsService, IUserService userService, IRewardHelper rewardHelper)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _userStatisticsService = userStatisticsService ?? throw new ArgumentNullException(nameof(userStatisticsService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _rewardHelper = rewardHelper ?? throw new ArgumentNullException(nameof(rewardHelper));
        }

        public async Task HandleUserStreaksAsync()
        {
            var today = DateTime.UtcNow;
            var usersWithPlans = await _userService.GetUsersWithPlanAsync(today);

            foreach (var user in usersWithPlans)
            {
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
            }

            // Cut current streak section
            var usersWithoutPlans = await _userService.GetUsersWithoutPlanAsync(today);

            foreach (var user in usersWithoutPlans)
            {
                var userStatistics = await _userStatisticsService.GetOrCreateUserStatisticsByUserAsync(user.Id);
                var balancedCurrentStreak = userStatistics.BalancedEatCurrentStreak;
                var eatCurrentStreak = userStatistics.EatCurrentStreak;

                if (balancedCurrentStreak > 0)
                {
                    await _userStatisticsService.CutCurrentStreakAsync(userStatistics, StreakEnum.BALANCED_EAT, user.Devices);
                    await _userStatisticsService.CutCurrentStreakAsync(userStatistics, StreakEnum.EAT, null);
                }
                else if (eatCurrentStreak > 0)
                {
                    await _userStatisticsService.CutCurrentStreakAsync(userStatistics, StreakEnum.EAT, user.Devices);
                }
            }
        }
    }
}
