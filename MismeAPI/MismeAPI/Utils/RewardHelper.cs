using AutoMapper;
using CorePush.Google;
using FirebaseAdmin.Messaging;
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
using Serilog;
using System;
using System.Collections;
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

        public RewardHelper(IMapper mapper, IRewardService rewardService, IHubContext<UserHub> hub, IUserStatisticsService userStatisticsService,
            ICutPointService cutPointService, IUserService userService, IConfiguration config)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _rewardService = rewardService ?? throw new ArgumentNullException(nameof(rewardService));
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
            _userStatisticsService = userStatisticsService ?? throw new ArgumentNullException(nameof(userStatisticsService));
            _cutPointService = cutPointService ?? throw new ArgumentNullException(nameof(cutPointService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
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
        public async Task HandleRewardAsync(RewardCategoryEnum category, int targetUser, bool isPlus, object entity1, object entity2,
            NotificationTypeEnum notificationType = NotificationTypeEnum.SIGNAL_R, IEnumerable<Device> devices = null)
        {
            var userStatics = await _userStatisticsService.GetOrCreateUserStatisticsByUserAsync(targetUser);
            var currentPoints = userStatics.Points;
            var cutPoints = await _cutPointService.GetNextCutPointsAsync(currentPoints, 5);

            var data = new RewardHistoryData
            {
                // Make sure that this is an important info to store in the history
                Entity1 = entity1,
                Entity2 = entity2
            };
            var reward = new CreateRewardRequest
            {
                UserId = targetUser,
                RewardCategoryEnum = (int)category,
                IsPlus = isPlus,
                Data = JsonConvert.SerializeObject(data),
            };

            var dbReward = await _rewardService.CreateRewardAsync(reward);
            // assign only if the reward was created after validations
            if (dbReward != null)
            {
                var mapped = _mapper.Map<RewardResponse>(dbReward);

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

                        if (devices != null)
                            await SendFirebaseNotificationAsync(title, body, devices);
                        break;

                    default:
                        break;
                }

                await HandleCutPointRewardsAsync(cutPoints, targetUser);
            }
        }

        private async Task HandleCutPointRewardsAsync(IEnumerable<CutPoint> cutPoints, int targetUser)
        {
            foreach (var cutPoint in cutPoints)
            {
                var userStatics = await _userStatisticsService.GetOrCreateUserStatisticsByUserAsync(targetUser);
                var currentPoints = userStatics.Points;

                if (cutPoint.Points <= currentPoints)
                {
                    await HandleRewardAsync(RewardCategoryEnum.CUT_POINT_REACHED, targetUser, true, cutPoint, null);
                }
            }
        }

        private async Task SendSignalRNotificationAsync(string hubConstant, object data)
        {
            await _hub.Clients.All.SendAsync(hubConstant, data);
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
                        Log.Information("Notification sent", response.ToString());
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Notification send exception");
                    }
                }
            }
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
                "EN" => "Congratulations. Dr.PlaniFive have approved your your dish and it will be visible to other users." + ". You receipt " + rewardResponse.Points + " points.",
                _ => "Enhorabuena! El Dr.PlaniFive ha aprobado su alimento y estara visible para otros usuarios en la base de datos general." + ". Ha ganado usted " + rewardResponse.Points + " puntos.",
            };
            return message;
        }
    }
}
