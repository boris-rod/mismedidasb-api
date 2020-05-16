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
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class RewardCategoryService : IRewardCategoryService
    {
        private readonly IUnitOfWork _uow;

        public RewardCategoryService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<RewardCategory> CreateRewardCategorAsync(int loggedUser, CreateRewardCategoryRequest rewardCategory)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var category = (RewardCategoryEnum)rewardCategory.Category;

            // validate category not duplicated
            var existCategory = await _uow.RewardCategoryRepository.FindBy(p => p.Category == category).CountAsync();
            if (existCategory > 0)
            {
                throw new AlreadyExistsException("The Reward Category already exist.");
            }

            var dbRewardCategory = new RewardCategory();
            dbRewardCategory.Category = category;
            dbRewardCategory.Description = rewardCategory.Description;
            dbRewardCategory.MaxPointsAllowed = rewardCategory.MaxPointsAllowed;
            dbRewardCategory.PointsToDecrement = rewardCategory.PointsToDecrement;
            dbRewardCategory.PointsToIncrement = rewardCategory.PointsToIncrement;

            await _uow.RewardCategoryRepository.AddAsync(dbRewardCategory);
            await _uow.CommitAsync();

            return dbRewardCategory;
        }

        public async Task DeleteRewardCategoryAsync(int loggedUser, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var rewardCategory = await _uow.RewardCategoryRepository.GetAsync(id);
            if (rewardCategory == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Reward Category");
            }

            _uow.RewardCategoryRepository.Delete(rewardCategory);
            await _uow.CommitAsync();
        }

        public async Task<RewardCategory> GetRewardCategoryByIdAsync(int id)
        {
            var rewardCategory = await _uow.RewardCategoryRepository.GetAsync(id);
            if (rewardCategory == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Reward Category");
            }
            return rewardCategory;
        }

        public async Task<RewardCategory> GetRewardCategoryByCategoryAsync(int category)
        {
            var rewardCategory = await _uow.RewardCategoryRepository.GetAll().Where(d => d.Category == (RewardCategoryEnum)category)
                .FirstOrDefaultAsync();
            if (rewardCategory == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Reward Category");
            }
            return rewardCategory;
        }

        public async Task<IEnumerable<RewardCategory>> GetRewardCategoriesAsync()
        {
            var results = _uow.RewardCategoryRepository.GetAll()
                .AsQueryable();

            return await results.ToListAsync();
        }

        public async Task<RewardCategory> UpdateRewardCategoryAsync(int loggedUser, UpdateRewardCategoryRequest rewardCategory)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var dbRewardCategory = await GetRewardCategoryByIdAsync(rewardCategory.Id);

            // validate dish name
            if (rewardCategory.PointsToIncrement < 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Points to increment");
            }
            if (rewardCategory.PointsToDecrement < 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Points to decrement");
            }

            dbRewardCategory.Description = rewardCategory.Description;
            dbRewardCategory.MaxPointsAllowed = rewardCategory.MaxPointsAllowed;
            dbRewardCategory.PointsToDecrement = rewardCategory.PointsToDecrement;
            dbRewardCategory.PointsToIncrement = rewardCategory.PointsToIncrement;

            await _uow.RewardCategoryRepository.UpdateAsync(dbRewardCategory, dbRewardCategory.Id);
            await _uow.CommitAsync();

            return dbRewardCategory;
        }

        public async Task InitRewardCategoriesAsync()
        {
            // POLL_ANSWERED, EAT_CREATED, EAT_BALANCED_CREATED, EAT_CREATED_STREAK,
            // EAT_BALANCED_CREATED_STREAK, DISH_BUILT, NEW_REFERAL, CUT_POINT_REACHED

            await GetCreateRewardCategoryByCategorySaveAsync(RewardCategoryEnum.POLL_ANSWERED);
            await GetCreateRewardCategoryByCategorySaveAsync(RewardCategoryEnum.EAT_CREATED);
            await GetCreateRewardCategoryByCategorySaveAsync(RewardCategoryEnum.EAT_BALANCED_CREATED);
            await GetCreateRewardCategoryByCategorySaveAsync(RewardCategoryEnum.EAT_CREATED_STREAK);
            await GetCreateRewardCategoryByCategorySaveAsync(RewardCategoryEnum.EAT_BALANCED_CREATED_STREAK);
            await GetCreateRewardCategoryByCategorySaveAsync(RewardCategoryEnum.DISH_BUILT);
            await GetCreateRewardCategoryByCategorySaveAsync(RewardCategoryEnum.NEW_REFERAL);
            await GetCreateRewardCategoryByCategorySaveAsync(RewardCategoryEnum.CUT_POINT_REACHED);

            await _uow.CommitAsync();
        }

        private async Task GetCreateRewardCategoryByCategorySaveAsync(RewardCategoryEnum category)
        {
            var rewardCategory = await _uow.RewardCategoryRepository.GetAll().Where(d => d.Category == category)
                .FirstOrDefaultAsync();

            if (rewardCategory == null)
            {
                rewardCategory = new RewardCategory
                {
                    Category = category,
                    Description = "",
                    MaxPointsAllowed = -1,
                    PointsToDecrement = 0,
                    PointsToIncrement = 5,
                };

                switch (category)
                {
                    case RewardCategoryEnum.POLL_ANSWERED:
                        rewardCategory.PointsToIncrement = 10;
                        rewardCategory.MaxPointsAllowed = 30;
                        break;

                    case RewardCategoryEnum.EAT_CREATED:
                        break;

                    case RewardCategoryEnum.EAT_BALANCED_CREATED:
                        rewardCategory.PointsToIncrement = 10;
                        break;

                    case RewardCategoryEnum.EAT_CREATED_STREAK:
                        rewardCategory.PointsToIncrement = 5;
                        rewardCategory.PointsToDecrement = 5;

                        break;

                    case RewardCategoryEnum.EAT_BALANCED_CREATED_STREAK:
                        rewardCategory.PointsToIncrement = 10;
                        rewardCategory.PointsToDecrement = 10;
                        break;

                    case RewardCategoryEnum.DISH_BUILT:
                        rewardCategory.PointsToIncrement = 10;
                        break;

                    case RewardCategoryEnum.NEW_REFERAL:
                        rewardCategory.PointsToIncrement = 25;
                        break;

                    case RewardCategoryEnum.CUT_POINT_REACHED:
                        rewardCategory.PointsToIncrement = 20;
                        break;

                    default:
                        break;
                }

                await _uow.RewardCategoryRepository.AddAsync(rewardCategory);
            }
        }
    }
}
