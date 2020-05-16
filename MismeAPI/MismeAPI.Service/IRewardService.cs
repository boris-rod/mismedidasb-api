using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Dish;
using MismeAPI.Common.DTO.Request.Reward;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IRewardService
    {
        Task<RewardHistory> CreateRewardAsync(CreateRewardRequest reward);

        Task DeleteRewardAsync(int loggedUser, int id);
    }
}
