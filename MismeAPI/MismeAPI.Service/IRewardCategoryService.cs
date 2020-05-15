using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Dish;
using MismeAPI.Common.DTO.Request.Reward;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IRewardCategoryService
    {
        Task<IEnumerable<RewardCategory>> GetRewardCategoriesAsync();

        Task<RewardCategory> GetRewardCategoryByIdAsync(int id);

        Task<RewardCategory> GetRewardCategoryByCategoryAsync(int category);

        Task<RewardCategory> CreateRewardCategorAsync(int loggedUser, CreateRewardCategoryRequest rewardCategory);

        Task<RewardCategory> UpdateRewardCategoryAsync(int loggedUser, UpdateRewardCategoryRequest rewardCategory);

        Task DeleteRewardCategoryAsync(int loggedUser, int id);

        Task InitRewardCategoriesAsync();
    }
}
