using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Services.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IUserStatisticsService
    {
        Task<PaginatedList<UserStatistics>> GetUserStatisticsAsync(int loggedUser, int pag, int perPag, string sortOrder);

        Task<UserStatistics> GetUserStatisticsByUserAsync(int userId);

        Task<UserStatistics> UpdateTotalPoints(User user, int points);

        Task<UserStatistics> UpdateTotalCoinsAsync(User user, int coins);

        Task<int> AllowedPointsAsync(int userId, int points);

        Task<UserStatistics> GetOrCreateUserStatisticsByUserAsync(int userId);

        Task<UserStatistics> IncrementCurrentStreakAsync(UserStatistics statistic, StreakEnum streak);

        Task<UserStatistics> CutCurrentStreakAsync(UserStatistics statistic, StreakEnum streak, IEnumerable<Device> devices);

        Task<UserRankingResponse> GetUserRankingAsync(int userId);

        Task RewardCoinsToUsersAsync(IEnumerable<int> userIds, int coins);

        Task RewardTestersAsync();
    }
}
