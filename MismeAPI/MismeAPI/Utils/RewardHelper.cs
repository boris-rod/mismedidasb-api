using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common.DTO.Request.Reward;
using MismeAPI.Common.DTO.Response.Reward;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.Entities.NonDatabase;
using MismeAPI.Service;
using MismeAPI.Service.Hubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Utils
{
    public class RewardHelper : IRewardHelper
    {
        private readonly IRewardService _rewardService;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly ICutPointService _cutPointService;
        private IHubContext<UserHub> _hub;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly INotificationService _notificationService;

        public RewardHelper(IMapper mapper, IRewardService rewardService, IHubContext<UserHub> hub, IUserStatisticsService userStatisticsService,
            ICutPointService cutPointService, IUserService userService, IConfiguration config, INotificationService notificationService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _rewardService = rewardService ?? throw new ArgumentNullException(nameof(rewardService));
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
            _userStatisticsService = userStatisticsService ?? throw new ArgumentNullException(nameof(userStatisticsService));
            _cutPointService = cutPointService ?? throw new ArgumentNullException(nameof(cutPointService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// Handle the creation of a reward
        /// </summary>
        /// <param name="category">Which type of reward will be</param>
        /// <param name="targetUser">user who will receive the reward</param>
        /// <param name="isPlus">Sum or rest points this reward</param>
        /// <param name="entity1">Info to store in the history</param>
        /// <param name="entity2">Other info to store in the history</param>
        /// <param name="notificationType">How will be the user notified</param>
        /// <param name="devices">Firebase notification targets</param>
        /// <returns>void - notify client that a reward was created via websocket</returns>
        public async Task<RewardResponse> HandleRewardAsync(RewardCategoryEnum category, int targetUser, bool isPlus, object entity1, object entity2,
            NotificationTypeEnum notificationType = NotificationTypeEnum.SIGNAL_R, IEnumerable<Device> devices = null)
        {
            var userStatics = await _userStatisticsService.GetOrCreateUserStatisticsByUserAsync(targetUser);
            var currentPoints = userStatics.Points;
            var cutPoints = await _cutPointService.GetNextCutPointsAsync(currentPoints, 5);

            var data = new RewardHistoryData
            {
                // Make sure that this is an important info to store in the history
                Entity1 = JsonConvert.SerializeObject(entity1),
                Entity2 = JsonConvert.SerializeObject(entity2)
            };
            var reward = new CreateRewardRequest
            {
                UserId = targetUser,
                RewardCategoryEnum = (int)category,
                IsPlus = isPlus,
                Data = JsonConvert.SerializeObject(data),
            };

            var dbReward = await _rewardService.CreateRewardAsync(reward);
            RewardResponse mapped = null;
            // assign only if the reward was created after validations
            if (dbReward != null)
            {
                mapped = _mapper.Map<RewardResponse>(dbReward);

                switch (notificationType)
                {
                    case NotificationTypeEnum.SIGNAL_R:
                        await SendSignalRNotificationAsync(HubConstants.REWARD_CREATED, mapped);
                        break;

                    case NotificationTypeEnum.FIREBASE:
                        var lang = await _userService.GetUserLanguageFromUserIdAsync(mapped.UserId);
                        var title = (lang == "EN") ? "You have receipt a new reward" : "Has recibido una nueva recompensa";
                        var body = "";
                        if (category == RewardCategoryEnum.EAT_BALANCED_CREATED_STREAK || category == RewardCategoryEnum.EAT_CREATED_STREAK)
                        {
                            body = GetFirebaseMessageForStreakReward(category, (int)entity1, mapped, lang);
                        }

                        if (category == RewardCategoryEnum.DISH_BUILT)
                        {
                            body = GetFirebaseMessageForDishBuildReward(mapped, lang);
                        }

                        if (category == RewardCategoryEnum.NEW_REFERAL)
                        {
                            body = GetFirebaseMessageForNewReferralReward(mapped, lang);
                        }

                        if (category == RewardCategoryEnum.EAT_BALANCED_CREATED || category == RewardCategoryEnum.EAT_CREATED)
                        {
                            body = GetFirebaseMessageForCreateEatReward(mapped, lang, category);
                        }

                        if (category == RewardCategoryEnum.CUT_POINT_REACHED)
                        {
                            body = GetFirebaseMessageForCutPointReachedReward(mapped, lang);
                        }

                        if (devices != null)
                            await _notificationService.SendFirebaseNotificationAsync(title, body, devices);
                        break;

                    default:
                        break;
                }

                await HandleCutPointRewardsAsync(cutPoints, targetUser);
            }

            return mapped;
        }

        private async Task HandleCutPointRewardsAsync(IEnumerable<CutPoint> cutPoints, int targetUser)
        {
            foreach (var cutPoint in cutPoints)
            {
                var userStatics = await _userStatisticsService.GetOrCreateUserStatisticsByUserAsync(targetUser);
                var currentPoints = userStatics.Points;

                if (cutPoint.Points <= currentPoints)
                {
                    var user = await _userService.GetUserDevicesAsync(targetUser);
                    await HandleRewardAsync(RewardCategoryEnum.CUT_POINT_REACHED, targetUser, true, cutPoint, null, NotificationTypeEnum.FIREBASE, user.Devices);
                }
            }
        }

        private async Task SendSignalRNotificationAsync(string hubConstant, object data)
        {
            await _hub.Clients.All.SendAsync(hubConstant, data);
        }

        private string GetFirebaseMessageForStreakReward(RewardCategoryEnum category, int streak, RewardResponse rewardResponse, string lang)
        {
            var reason = "";

            switch (category)
            {
                case RewardCategoryEnum.EAT_CREATED_STREAK:
                    switch (lang)
                    {
                        case "EN":
                            reason = "consecutive days planning your eat";
                            break;

                        default:
                            reason = "dias consecutivo planificando tu comida";
                            break;
                    }
                    break;

                case RewardCategoryEnum.EAT_BALANCED_CREATED_STREAK:
                    switch (lang)
                    {
                        case "EN":
                            reason = "consecutive days planning your eat balanced";
                            break;

                        default:
                            reason = "dias consecutivo planificando tu comida balanceada";
                            break;
                    }

                    break;

                default:
                    break;
            }

            string message = lang switch
            {
                "EN" => streak.ToString() + " " + reason + ". You receipt " + rewardResponse.Points + " points.",
                _ => streak.ToString() + " " + reason + ". Has recivido " + rewardResponse.Points + " puntos.",
            };
            return message;
        }

        private string GetFirebaseMessageForDishBuildReward(RewardResponse rewardResponse, string lang)
        {
            string message = lang switch
            {
                "EN" => "Congratulations. Dr.PlaniFive have approved your dish and it will be visible to other users now." + ". You receipt " + rewardResponse.Points + " points.",
                _ => "Enhorabuena! El Dr.PlaniFive ha aprobado su alimento y estara visible para otros usuarios en la base de datos general." + ". Ha ganado usted " + rewardResponse.Points + " puntos.",
            };
            return message;
        }

        private string GetFirebaseMessageForCutPointReachedReward(RewardResponse rewardResponse, string lang)
        {
            string message = lang switch
            {
                "EN" => "Congratulations. You reached a cut point (objective)." + ". You receipt " + rewardResponse.Points + " points.",
                _ => "Enhorabuena! Usted ha alcanzado un punto de corte (Objetivo)." + ". Ha ganado usted " + rewardResponse.Points + " puntos.",
            };
            return message;
        }

        private string GetFirebaseMessageForNewReferralReward(RewardResponse rewardResponse, string lang)
        {
            string message = lang switch
            {
                "EN" => "Congratulations. A user invited by you has just joined PlaniFive." + ". You receipt " + rewardResponse.Points + " points.",
                _ => "Enhorabuena! Un usuario invitado por usted se ha unido a PlaniFive." + ". Ha ganado usted " + rewardResponse.Points + " puntos.",
            };
            return message;
        }

        private string GetFirebaseMessageForCreateEatReward(RewardResponse rewardResponse, string lang, RewardCategoryEnum category)
        {
            var eat = lang == "EN" ? "balaced " : "balanceado ";
            if (category == RewardCategoryEnum.EAT_CREATED)
                eat = "";

            string message = lang switch
            {
                "EN" => "Congratulations. You have been rewarded for creating a " + eat + "plan today." + ". You receipt " + rewardResponse.Points + " points.",
                _ => "Enhorabuena! Has sido premiado por crear un plan " + eat + "hoy." + ". Ha ganado usted " + rewardResponse.Points + " puntos.",
            };
            return message;
        }
    }
}
