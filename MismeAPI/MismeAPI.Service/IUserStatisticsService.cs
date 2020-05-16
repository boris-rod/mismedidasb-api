using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Dish;
using MismeAPI.Common.DTO.Request.Reward;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IUserStatisticsService
    {
        Task<UserStatistics> UpdateTotalPoints(User user, int points);

        Task<int> AllowedPointsAsync(int userId, int points);
    }
}
