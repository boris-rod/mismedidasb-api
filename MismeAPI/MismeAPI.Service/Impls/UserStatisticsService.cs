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

        public async Task<int> AllowedPoints(int userId, int points)
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
            }

            return statistics;
        }
    }
}
