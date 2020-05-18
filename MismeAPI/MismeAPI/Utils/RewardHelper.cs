using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using MismeAPI.Common.DTO.Request.Reward;
using MismeAPI.Common.DTO.Response.Reward;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.Entities.NonDatabase;
using MismeAPI.Service;
using MismeAPI.Service.Hubs;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MismeAPI.Utils
{
    public class RewardHelper : IRewardHelper
    {
        private readonly IRewardService _rewardService;
        private IHubContext<UserHub> _hub;
        private readonly IMapper _mapper;

        public RewardHelper(IMapper mapper, IRewardService rewardService, IHubContext<UserHub> hub)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _rewardService = rewardService ?? throw new ArgumentNullException(nameof(rewardService));
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        }

        /// <summary>
        /// Handle the creation of a reward
        /// </summary>
        /// <param name="category">Which type of reward will be</param>
        /// <param name="targetUser">user who will receive the reward</param>
        /// <param name="isPlus">Sum or rest points this reward</param>
        /// <param name="entity1">Info to store in the history</param>
        /// <param name="entity2">Other info to store in the history</param>
        /// <returns>void - notify client that a reward was created via websocket</returns>
        public async Task HandleRewardAsync(RewardCategoryEnum category, int targetUser, bool isPlus, object entity1, object entity2)
        {
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
                await _hub.Clients.All.SendAsync(HubConstants.REWARD_CREATED, mapped);
            }
        }
    }
}
