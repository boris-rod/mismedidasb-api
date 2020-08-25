using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Dish;
using MismeAPI.Common.DTO.Request.Reward;
using MismeAPI.Common.DTO.Response.SoloQuestion;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.Entities.NonDatabase;
using MismeAPI.Data.UoW;
using MismeAPI.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class RewardService : IRewardService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRewardCategoryService _rewardCategoryService;
        private readonly IUserStatisticsService _userStatisticsService;

        public RewardService(IUnitOfWork uow, IRewardCategoryService rewardCategoryService, IUserStatisticsService userStatisticsService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _rewardCategoryService = rewardCategoryService ?? throw new ArgumentNullException(nameof(rewardCategoryService));
            _userStatisticsService = userStatisticsService ?? throw new ArgumentNullException(nameof(userStatisticsService));
        }

        public async Task<RewardHistory> CreateRewardAsync(CreateRewardRequest rewardRequest)
        {
            // validate target user exists
            var existUser = await _uow.UserRepository.GetAsync(rewardRequest.UserId);

            if (existUser == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            // get/validate-exist reward type
            var rewardCategory = await _rewardCategoryService.GetRewardCategoryByCategoryAsync(rewardRequest.RewardCategoryEnum);
            var rewardAcomulate = await GetOrCreateRewardAcomulate(existUser, rewardCategory);

            var allowedPoints = 0;

            if (rewardCategory.Category == RewardCategoryEnum.SOLO_QUESTION_ANSWERED)
            {
                var data = JsonConvert.DeserializeObject<RewardHistoryData>(rewardRequest.Data);
                var userAnswer = JsonConvert.DeserializeObject<UserSoloAnswerResponse>(data.Entity1);
                allowedPoints = userAnswer.Points;
            }
            else
            {
                allowedPoints = await AllowedPointsAsync(rewardCategory, rewardAcomulate, rewardRequest.IsPlus);
            }

            if (allowedPoints == 0)
            {
                return null;
            }

            var rewardPoints = rewardRequest.IsPlus ? rewardCategory.PointsToIncrement : rewardCategory.PointsToDecrement;
            if (rewardCategory.Category == RewardCategoryEnum.SOLO_QUESTION_ANSWERED)
            {
                rewardPoints = allowedPoints;
            }

            var dbReward = new RewardHistory();
            dbReward.User = existUser;
            dbReward.RewardCategory = rewardCategory;
            dbReward.IsPlus = rewardRequest.IsPlus;
            dbReward.Points = allowedPoints;
            dbReward.RewardPoints = rewardPoints;
            dbReward.Data = rewardRequest.Data;
            dbReward.CreatedAt = DateTime.UtcNow;

            await _uow.RewardHistoryRepository.AddAsync(dbReward);

            allowedPoints = rewardRequest.IsPlus ? allowedPoints : allowedPoints * -1;
            rewardAcomulate.Points = rewardAcomulate.Points + allowedPoints;
            rewardAcomulate.ModifiedAt = DateTime.UtcNow;

            await _uow.RewardAcumulateRepository.UpdateAsync(rewardAcomulate, rewardAcomulate.Id);
            await _userStatisticsService.UpdateTotalPoints(existUser, allowedPoints);
            await _userStatisticsService.UpdateTotalCoinsAsync(existUser, allowedPoints);
            await _uow.CommitAsync();

            return dbReward;
        }

        public async Task DeleteRewardAsync(int loggedUser, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var rewardCategory = await _uow.RewardHistoryRepository.GetAsync(id);
            if (rewardCategory == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Reward");
            }

            _uow.RewardHistoryRepository.Delete(rewardCategory);
            await _uow.CommitAsync();
        }

        /// <summary>
        /// Init acumulate if the user have no one for this category
        /// </summary>
        /// <param name="user"></param>
        /// <param name="rewardCategory"></param>
        /// <returns></returns>
        private async Task<RewardAcumulate> GetOrCreateRewardAcomulate(User user, RewardCategory rewardCategory)
        {
            var acumulate = await _uow.RewardAcumulateRepository.GetAll()
                .Where(d => d.UserId == user.Id && d.RewardCategoryId == rewardCategory.Id)
                .FirstOrDefaultAsync();

            if (acumulate == null)
            {
                acumulate = new RewardAcumulate
                {
                    RewardCategory = rewardCategory,
                    User = user,
                    Points = 0,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await _uow.RewardAcumulateRepository.AddAsync(acumulate);
            }

            return acumulate;
        }

        /// <summary>
        /// Return 0 if the reward cannot be assigned to this user due to whathever he cannot
        /// receive or loose more points in this category
        /// </summary>
        /// <param name="rewardCategory"></param>
        /// <param name="rewardAcomulate"></param>
        /// <param name="isPlus">Receive or Loose points</param>
        /// <returns>allowed points</returns>
        private async Task<int> AllowedPointsAsync(RewardCategory rewardCategory, RewardAcumulate rewardAcomulate, bool isPlus)
        {
            var currentAcumulate = rewardAcomulate.Points;
            var points = 0;

            if (isPlus)
            {
                if (rewardCategory.MaxPointsAllowed != -1)
                {
                    var newAcumulate = currentAcumulate + rewardCategory.PointsToIncrement;
                    if (newAcumulate > rewardCategory.MaxPointsAllowed)
                    {
                        points = rewardCategory.MaxPointsAllowed - currentAcumulate;
                    }
                    else
                    {
                        points = rewardCategory.PointsToIncrement;
                    }
                }
                else
                {
                    points = rewardCategory.PointsToIncrement;
                }
            }
            else
            {
                if (currentAcumulate == 0)
                {
                    points = 0;
                }
                else
                {
                    var diff = currentAcumulate - rewardCategory.PointsToDecrement;
                    if (diff < 0)
                    {
                        points = currentAcumulate;
                    }
                    else
                    {
                        points = rewardCategory.PointsToDecrement;
                    }
                }
            }

            // Know if the total allow this reward, should only be not allowed if total gets under 0
            if (points == 0)
                return 0;

            var allowedPoints = await _userStatisticsService.AllowedPointsAsync(rewardAcomulate.UserId, isPlus ? points : points * -1);

            return allowedPoints;
        }
    }
}
