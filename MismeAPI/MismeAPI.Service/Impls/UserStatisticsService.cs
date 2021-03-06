﻿using Amazon.SimpleSystemsManagement.Model;
using CorePush.Google;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request.Reward;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class UserStatisticsService : IUserStatisticsService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly INotificationService _notificationService;

        public UserStatisticsService(IUnitOfWork uow, IUserService userService, IConfiguration config, INotificationService notificationService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public async Task<PaginatedList<UserStatistics>> GetUserStatisticsAsync(int loggedUser, int pag, int perPag, string sortOrder)
        {
            var result = _uow.UserStatisticsRepository.GetAll()
                .Include(u => u.User)
                .AsQueryable();

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "points_desc":
                        result = result.OrderByDescending(i => i.Points);
                        break;

                    case "pointse_asc":
                        result = result.OrderBy(i => i.Points);
                        break;

                    case "balancedEatCurrentStreak_desc":
                        result = result.OrderByDescending(i => i.BalancedEatCurrentStreak);
                        break;

                    case "email_asc":
                        result = result.OrderBy(i => i.BalancedEatCurrentStreak);
                        break;

                    case "balancedEatMaxStreak_desc":
                        result = result.OrderByDescending(i => i.BalancedEatMaxStreak);
                        break;

                    case "phone_asc":
                        result = result.OrderBy(i => i.BalancedEatMaxStreak);
                        break;

                    case "eatCurrentStreak_desc":
                        result = result.OrderByDescending(i => i.EatCurrentStreak);
                        break;

                    case "eatCurrentStreak_asc":
                        result = result.OrderBy(i => i.EatCurrentStreak);
                        break;

                    case "eatMaxStreak_desc":
                        result = result.OrderByDescending(i => i.EatMaxStreak);
                        break;

                    case "eatMaxStreak_asc":
                        result = result.OrderBy(i => i.EatMaxStreak);
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<UserStatistics>.CreateAsync(result, pag, perPag);
        }

        /// <summary>
        /// Method to get without update database. Security vulnerability patch.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Exeption is thwown if the statics does not exist.</returns>
        public async Task<UserStatistics> GetUserStatisticsByUserAsync(int userId)
        {
            var existUser = await _uow.UserRepository.GetAll()
                .Include(u => u.UserStatistics)
                    .ThenInclude(us => us.User)
                .Where(d => d.Id == userId)
                .FirstOrDefaultAsync();

            if (existUser == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            if (existUser.UserStatistics == null)
            {
                existUser.UserStatistics = await GetOrCreateUserStatisticsAsync(existUser);
            }

            return existUser.UserStatistics;
        }

        public async Task<UserRankingResponse> GetUserRankingAsync(int userId)
        {
            var userStatics = await GetOrCreateUserStatisticsByUserAsync(userId);
            var usersWithNoStatistics = await _uow.UserRepository.GetAll()
                .Include(u => u.UserStatistics)
                .Where(u => u.UserStatistics == null)
                .CountAsync();

            var ranking = await _uow.UserStatisticsRepository.GetAll()
                .OrderByDescending(u => u.Points)
                .ToListAsync();

            var position = ranking.FindIndex(x => x.UserId == userId) + 1;
            var total = ranking.Count() + usersWithNoStatistics;

            if (position == 0)
            {
                position = total - 1;
            }

            var percentageB = ((total - position) * 100) / total;

            var userRanking = new UserRankingResponse
            {
                Points = userStatics.Points,
                RankingPosition = position,
                PercentageBehind = percentageB
            };

            return userRanking;
        }

        public async Task<UserStatistics> UpdateTotalPoints(User user, int points)
        {
            var statistics = await GetOrCreateUserStatisticsAsync(user);
            // TODO user statics missing

            var newPoints = statistics.Points + points;
            if (newPoints < 0)
            {
                newPoints = 0;
            }

            statistics.Points = newPoints;
            statistics.ModifiedAt = DateTime.UtcNow;

            await _uow.UserStatisticsRepository.UpdateAsync(statistics, statistics.Id);
            await _uow.CommitAsync();

            return statistics;
        }

        public async Task<UserStatistics> UpdateTotalCoinsAsync(User user, int coins)
        {
            // validate target user exists
            var existUser = await _uow.UserRepository.GetAll().Where(d => d.Id == user.Id).FirstOrDefaultAsync();

            if (existUser == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            var statistics = await GetOrCreateUserStatisticsAsync(existUser);

            var newCoints = statistics.Coins + coins;
            if (newCoints < 0)
            {
                newCoints = 0;
            }

            statistics.Coins = newCoints;
            statistics.ModifiedAt = DateTime.UtcNow;

            await _uow.UserStatisticsRepository.UpdateAsync(statistics, statistics.Id);
            await _uow.CommitAsync();

            return statistics;
        }

        public async Task<int> AllowedPointsAsync(int userId, int points)
        {
            var statistics = await _uow.UserStatisticsRepository.GetAll()
                .Where(d => d.UserId == userId)
                .FirstOrDefaultAsync();

            if (statistics == null)
                return points;

            var newPoints = statistics.Points + points;

            if (newPoints < 0)
            {
                return statistics.Points;
            }

            return points;
        }

        public async Task<UserStatistics> GetOrCreateUserStatisticsByUserAsync(int userId)
        {
            var existUser = await _uow.UserRepository.GetAll()
               .Include(u => u.UserStatistics)
                   .ThenInclude(us => us.User)
               .Where(d => d.Id == userId)
               .FirstOrDefaultAsync();

            if (existUser == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            var stats = existUser.UserStatistics;

            if (stats == null)
            {
                stats = await GetOrCreateUserStatisticsAsync(existUser);
            }

            return stats;
        }

        public async Task<UserStatistics> IncrementCurrentStreakAsync(UserStatistics statistic, StreakEnum streak)
        {
            switch (streak)
            {
                case StreakEnum.EAT:
                    statistic.TotalNonBalancedEatsPlanned++;
                    statistic.EatCurrentStreak++;
                    if (statistic.EatMaxStreak < statistic.EatCurrentStreak)
                    {
                        statistic.EatMaxStreak = statistic.EatCurrentStreak;
                    }
                    break;

                case StreakEnum.BALANCED_EAT:
                    statistic.TotalBalancedEatsPlanned++;
                    statistic.BalancedEatCurrentStreak++;
                    statistic.EatCurrentStreak++;
                    if (statistic.BalancedEatMaxStreak < statistic.BalancedEatCurrentStreak)
                    {
                        statistic.BalancedEatMaxStreak = statistic.BalancedEatCurrentStreak;
                    }
                    if (statistic.EatMaxStreak < statistic.EatCurrentStreak)
                    {
                        statistic.EatMaxStreak = statistic.EatCurrentStreak;
                    }
                    break;

                default:
                    break;
            }

            await _uow.UserStatisticsRepository.UpdateAsync(statistic, statistic.Id);
            await _uow.CommitAsync();

            return statistic;
        }

        public async Task<UserStatistics> CutCurrentStreakAsync(UserStatistics statistic, StreakEnum streak, IEnumerable<Device> devices)
        {
            switch (streak)
            {
                case StreakEnum.EAT:
                    statistic.EatCurrentStreak = 0;

                    break;

                case StreakEnum.BALANCED_EAT:
                    statistic.BalancedEatCurrentStreak = 0;
                    break;

                default:
                    break;
            }

            await _uow.UserStatisticsRepository.UpdateAsync(statistic, statistic.Id);
            await _uow.CommitAsync();

            await NotifyStreekLooseAsync(statistic.User, streak, devices);

            return statistic;
        }

        public async Task RewardCoinsToUsersAsync(RewardManualCoinsRequest request)
        {
            if (request.UserIds.Count() < 1 && request.GroupIds.Count() < 1)
            {
                throw new InvalidDataException("Users/Groups required");
            }

            if (request.Coins < 1)
            {
                throw new InvalidDataException("Coins");
            }

            var query = _uow.UserRepository.GetAll()
                .Include(u => u.Devices)
                .Include(u => u.UserSettings)
                .Include(u => u.Group)
                .Where(u => u.Status == StatusEnum.ACTIVE)
                .AsQueryable();

            if (request.UserIds.Count() > 0 && request.UserIds.FirstOrDefault() != -1)
                query = query.Where(u => request.UserIds.Contains(u.Id));
            else if (request.GroupIds.Count() > 0)
                query = query.Where(u => u.GroupId.HasValue && request.GroupIds.Contains(u.GroupId.Value));

            var users = await query.ToListAsync();

            var setting = await _uow.SettingRepository.GetAll().Where(s => s.Name == SettingsConstants.LANGUAGE).FirstOrDefaultAsync();
            foreach (var user in users)
            {
                if (user.ActivatedAt.HasValue)
                {
                    var manualReward = await _uow.RewardCategoryRepository.GetAll().FirstOrDefaultAsync(r => r.Category == RewardCategoryEnum.ADMIN_MANUAL);
                    if (manualReward == null)
                    {
                        throw new Exception("Manual reward is not activated.");
                    }

                    await _uow.RewardHistoryRepository.AddAsync(new RewardHistory
                    {
                        RewardCategoryId = manualReward.Id,
                        Points = request.Coins,
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                    await _uow.CommitAsync();

                    await UpdateTotalCoinsAsync(user, request.Coins);

                    if (string.IsNullOrEmpty(request.Title))
                    {
                        await SendTesterCoinsRewardsNotificationAsync(user, request.Coins);
                    }
                    else
                    {
                        var title = request.Title;
                        var body = request.Body;

                        var lang = "ES";
                        if (user.UserSettings != null)
                        {
                            var userSetting = user.UserSettings.FirstOrDefault(us => us.SettingId == setting.Id);
                            if (userSetting != null && !string.IsNullOrWhiteSpace(userSetting.Value))
                            {
                                lang = userSetting.Value;
                            }
                        }

                        switch (lang)
                        {
                            case "ES":
                                await _notificationService.SendFirebaseNotificationAsync(title, body, user.Devices);
                                break;

                            case "EN":
                                var titleEN = string.IsNullOrEmpty(request.TitleEN) ? title : request.TitleEN;
                                var bodyEN = string.IsNullOrEmpty(request.BodyEN) ? body : request.BodyEN;
                                await _notificationService.SendFirebaseNotificationAsync(titleEN, bodyEN, user.Devices);
                                break;

                            case "IT":
                                var titleIT = string.IsNullOrEmpty(request.TitleIT) ? title : request.TitleIT;
                                var bodyIT = string.IsNullOrEmpty(request.BodyIT) ? body : request.BodyIT;
                                await _notificationService.SendFirebaseNotificationAsync(titleIT, bodyIT, user.Devices);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        private async Task SendTesterCoinsRewardsNotificationAsync(User user, int coins)
        {
            var lang = await _userService.GetUserLanguageFromUserIdAsync(user.Id);
            var title = (lang == "EN") ? "You have receipt a reward for being a tester" : "Has recibido una recompensa por ser tester";
            string body = lang switch
            {
                "EN" => "Thank you for your feedback. You receipt " + coins + " coins.",
                _ => "Gracias por tu aporte." + " Has recibido " + coins + " monedas.",
            };

            if (user?.Devices != null)
                await _notificationService.SendFirebaseNotificationAsync(title, body, user.Devices);
        }

        private async Task<UserStatistics> GetOrCreateUserStatisticsAsync(User user)
        {
            var statistics = await _uow.UserStatisticsRepository.GetAll()
                .Where(d => d.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (statistics == null)
            {
                statistics = new UserStatistics
                {
                    UserId = user.Id,
                    Points = 0,
                    BalancedEatCurrentStreak = 0,
                    BalancedEatMaxStreak = 0,
                    EatCurrentStreak = 0,
                    EatMaxStreak = 0,
                    TotalBalancedEatsPlanned = 0,
                    TotalNonBalancedEatsPlanned = 0,
                    Coins = 0,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await _uow.UserStatisticsRepository.AddAsync(statistics);
                await _uow.CommitAsync();
            }

            return statistics;
        }

        private async Task NotifyStreekLooseAsync(User user, StreakEnum streak, IEnumerable<Device> devices)
        {
            if (streak == StreakEnum.BALANCED_EAT || streak == StreakEnum.EAT)
            {
                var lang = await _userService.GetUserLanguageFromUserIdAsync(user.Id);
                var title = (lang == "EN") ? "Actual streak loose" : "Racha actual perdida";
                var body = GetFirebaseMessageForStreakLoose(streak, lang);
                if (devices != null)
                    await SendFirebaseNotificationAsync(title, body, devices);
            }
        }

        private async Task SendFirebaseNotificationAsync(string title, string body, IEnumerable<Device> devices)
        {
            var serverKey = _config["Firebase:ServerKey"];
            var senderId = _config["Firebase:SenderId"];

            foreach (var device in devices)
            {
                using (var fcm = new FcmSender(serverKey, senderId))
                {
                    Message message = new Message()
                    {
                        Notification = new Notification
                        {
                            Title = title,
                            Body = body,
                        },
                        Token = device.Token
                    };
                    try
                    {
                        var response = await fcm.SendAsync(device.Token, message);
                    }
                    catch (Exception)
                    {
                        // TODO
                    }
                }
            }
        }

        private string GetFirebaseMessageForStreakLoose(StreakEnum streak, string lang)
        {
            var reason = "";

            switch (streak)
            {
                case StreakEnum.EAT:
                    switch (lang)
                    {
                        case "EN":
                            reason = "You has lost your actual streak planning your food";
                            break;

                        default:
                            reason = "Has perdido la racha actual planificando tu comida";
                            break;
                    }
                    break;

                case StreakEnum.BALANCED_EAT:
                    switch (lang)
                    {
                        case "EN":
                            reason = "You has lost your actual streak planning your food balanced";
                            break;

                        default:
                            reason = "Has perdido la racha actual planificando tu comida balanceada";
                            break;
                    }

                    break;

                default:
                    break;
            }

            return reason;
        }
    }
}
