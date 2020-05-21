using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Dish;
using MismeAPI.Common.DTO.Request.Reward;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class UserStatisticsService : IUserStatisticsService
    {
        private readonly IUnitOfWork _uow;

        public UserStatisticsService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
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
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User Statistics");
            }

            return existUser.UserStatistics;
        }

        public async Task<UserStatistics> UpdateTotalPoints(User user, int points)
        {
            // validate target user exists
            var existUser = await _uow.UserRepository.GetAll().Where(d => d.Id == user.Id).FirstOrDefaultAsync();

            if (existUser == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            var statistics = await GetOrCreateUserStatisticsAsync(existUser);
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
                    statistic.EatCurrentStreak++;
                    if (statistic.EatMaxStreak < statistic.EatCurrentStreak)
                    {
                        statistic.EatMaxStreak = statistic.EatCurrentStreak;
                    }
                    break;

                case StreakEnum.BALANCED_EAT:
                    statistic.BalancedEatCurrentStreak++;
                    if (statistic.BalancedEatMaxStreak < statistic.BalancedEatCurrentStreak)
                    {
                        statistic.BalancedEatMaxStreak = statistic.BalancedEatCurrentStreak;
                    }
                    break;

                default:
                    break;
            }

            await _uow.UserStatisticsRepository.UpdateAsync(statistic, statistic.Id);

            return statistic;
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
                    User = user,
                    Points = 0,
                    BalancedEatCurrentStreak = 0,
                    BalancedEatMaxStreak = 0,
                    EatCurrentStreak = 0,
                    EatMaxStreak = 0,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await _uow.UserStatisticsRepository.AddAsync(statistics);
                await _uow.CommitAsync();
            }

            return statistics;
        }
    }
}
