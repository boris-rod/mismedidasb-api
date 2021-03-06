﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.CompoundDish;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace MismeAPI.Service.Impls
{
    public class CompoundDishService : ICompoundDishService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFileService _fileService;
        private readonly IConfiguration _config;

        public CompoundDishService(IUnitOfWork uow, IFileService fileService, IConfiguration config)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task ConvertUserDishAsync(int loggedUser, UpdateDishRequest dish)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var d = await _uow.CompoundDishRepository.GetAsync(dish.Id);
            if (d == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Dish");
            }

            // validate dish name, this no requires pass the id becaus we are converting a user dish
            // on a general dish
            var existDishName = await _uow.DishRepository.FindByAsync(p => p.Name.ToLower() == dish.Name.ToLower());
            if (existDishName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Dish name");
            }

            var dishh = new Dish();

            dishh.Calories = dish.Calories;
            dishh.Carbohydrates = dish.Carbohydrates;
            dishh.Fat = dish.Fat;
            dishh.Fiber = dish.Fiber;
            dishh.Name = dish.Name;
            dishh.Proteins = dish.Proteins;

            dishh.Cholesterol = dish.Cholesterol;
            dishh.Calcium = dish.Calcium;
            dishh.Phosphorus = dish.Phosphorus;
            dishh.Iron = dish.Iron;
            dishh.Potassium = dish.Potassium;
            dishh.Sodium = dish.Sodium;
            dishh.Zinc = dish.Zinc;
            dishh.VitaminA = dish.VitaminA;
            dishh.VitaminC = dish.VitaminC;
            dishh.VitaminB6 = dish.VitaminB6;
            dishh.VitaminB12 = dish.VitaminB12;
            dishh.VitaminD = dish.VitaminD;
            dishh.VitaminE = dish.VitaminE;
            dishh.VitaminK = dish.VitaminK;

            dishh.VitaminB1Thiamin = dish.VitaminB1Thiamin;
            dishh.VitaminB2Riboflavin = dish.VitaminB2Riboflavin;
            dishh.VitaminB3Niacin = dish.VitaminB3Niacin;
            dishh.VitaminB9Folate = dish.VitaminB9Folate;
            dishh.NetWeight = dish.NetWeight;
            dishh.Volume = dish.Volume;
            dishh.SaturatedFat = dish.SaturatedFat;
            dishh.PolyUnsaturatedFat = dish.PolyUnsaturatedFat;
            dishh.MonoUnsaturatedFat = dish.MonoUnsaturatedFat;
            dishh.Alcohol = dish.Alcohol;

            if (dish.Classification == 0)
            {
                dishh.IsProteic = true;
            }
            else if (dish.Classification == 1)
            {
                dishh.IsCaloric = true;
            }
            else
            {
                dishh.IsFruitAndVegetables = true;
            }

            // avatar

            // take the same user image
            if (dish.RemovedImage == "null" && dish.Image == null)
            {
                if (!string.IsNullOrWhiteSpace(d.Image))
                {
                    string destinationKey = Guid.NewGuid().ToString();
                    string sourceKey = d.Image;

                    await _fileService.CopyFileAsync(sourceKey, destinationKey);
                    dishh.Image = destinationKey;
                    dishh.ImageMimeType = d.ImageMimeType;
                }
            }
            else if (dish.Image != null)
            {
                string guid = Guid.NewGuid().ToString();
                await _fileService.UploadFileAsync(dish.Image, guid);
                dishh.Image = guid;
                dishh.ImageMimeType = dish.Image.ContentType;
            }

            //existing tags
            foreach (var id in dish.TagsIds)
            {
                var t = await _uow.TagRepository.GetAsync(id);
                if (t != null)
                {
                    var exist = await _uow.DishTagRepository.GetAll().Where(t => t.TagId == id && t.DishId == dish.Id).FirstOrDefaultAsync();
                    if (exist == null)
                    {
                        var dt = new DishTag
                        {
                            Dish = dishh,
                            TagId = t.Id,
                            TaggedAt = DateTime.UtcNow
                        };

                        await _uow.DishTagRepository.AddAsync(dt);
                    }
                }
            }
            //new tags
            foreach (var name in dish.NewTags)
            {
                var ta = new Tag
                {
                    Name = name
                };
                await _uow.TagRepository.AddAsync(ta);

                var dt = new DishTag
                {
                    Dish = dishh,
                    Tag = ta,
                    TaggedAt = DateTime.UtcNow
                };

                await _uow.DishTagRepository.AddAsync(dt);
            }

            d.IsAdminConverted = true;
            d.IsAdminReviewed = true;
            _uow.CompoundDishRepository.Update(d);

            await _uow.DishRepository.AddAsync(dishh);
            await _uow.CommitAsync();
            QueryCacheManager.ExpireTag(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);
        }

        public async Task<CompoundDish> CreateCompoundDishAsync(int ownerId, CreateCompoundDishRequest dish)
        {
            var existName = await _uow.CompoundDishRepository.FindByAsync(c => c.UserId == ownerId && c.Name == dish.Name && !c.IsDeleted);
            if (existName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Dish name");
            }

            var cDish = await AuxCreateCompoundDishAsync(ownerId, dish);
            await _uow.CommitAsync();

            return cDish;
        }

        public async Task DeleteCompoundDishAsync(int ownerId, int compoundDishId)
        {
            var c = await _uow.CompoundDishRepository.GetAll()
                .Where(c => c.Id == compoundDishId && c.UserId == ownerId)
                .Include(c => c.EatCompoundDishes)
                .Include(d => d.FavoriteDishes)
                .Include(d => d.LackSelfControlDishes)
                .FirstOrDefaultAsync();

            if (c == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Compound Dish");
            }

            var isInUse = c.EatCompoundDishes.Count() > 0;
            if (isInUse)
            {
                c.DeletedAt = DateTime.UtcNow;
                c.IsDeleted = true;
                await _uow.CompoundDishRepository.UpdateAsync(c, c.Id);
            }
            else
            {
                _uow.CompoundDishRepository.Delete(c);
            }

            await _uow.CommitAsync();
        }

        public async Task<PaginatedList<CompoundDish>> GetAllCompoundDishesAsync(int adminId, string search, int filter, int page, int perPage, string sort)
        {
            var user = await _uow.UserRepository.GetAll().Where(u => u.Id == adminId && u.Role == RoleEnum.ADMIN).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var results = _uow.CompoundDishRepository.GetAll()
                .Include(d => d.FavoriteDishes)
                .Include(d => d.LackSelfControlDishes)
                .Include(d => d.CreatedBy)
                .Include(d => d.DishCompoundDishes)
                  .ThenInclude(t => t.Dish)
                .Where(d => d.IsDeleted == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                results = results.Where(r => r.Name.ToLower().Contains(search.ToLower()));
            }
            if (filter != -1)
            {
                switch (filter)
                {
                    case 0:
                        results = results.Where(r => r.IsAdminReviewed == false);
                        break;

                    default:
                        results = results.Where(r => r.IsAdminReviewed == true);
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                // sort order section
                switch (sort)
                {
                    case "name_desc":
                        results = results.OrderByDescending(i => i.Name);
                        break;

                    case "name_asc":
                        results = results.OrderBy(i => i.Name);
                        break;

                    default:
                        break;
                }
            }

            //return await results.ToListAsync();
            return await PaginatedList<CompoundDish>.CreateAsync(results.AsQueryable(), page, perPage);
        }

        public async Task<IEnumerable<CompoundDish>> GetUserCompoundDishesAsync(int ownerId, string search, bool? favorites, bool? lackSelfControl)
        {
            var results = _uow.CompoundDishRepository.GetAll()
                .Where(c => c.UserId == ownerId && c.IsDeleted == false)
                .Include(d => d.FavoriteDishes)
                .Include(d => d.LackSelfControlDishes)
                .Include(d => d.DishCompoundDishes)
                    .ThenInclude(t => t.Dish)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                results = results.Where(r => r.Name.ToLower().Contains(search.ToLower()));
            }

            if (favorites.HasValue)
            {
                if (favorites.Value == true)
                    results.Where(cd => cd.FavoriteDishes.Any(fd => fd.UserId == ownerId));
                else
                    results.Where(cd => !cd.FavoriteDishes.Any(fd => fd.UserId == ownerId));
            }

            if (lackSelfControl.HasValue)
            {
                if (lackSelfControl.Value == true)
                    results.Where(cd => cd.LackSelfControlDishes.Any(fd => fd.UserId == ownerId));
                else
                    results.Where(cd => !cd.LackSelfControlDishes.Any(fd => fd.UserId == ownerId));
            }

            return await results.ToListAsync();
        }

        public async Task MarkCompoundDishAsReviewedAsync(int loggedUser, int id)
        {
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var compoundDish = await _uow.CompoundDishRepository.GetAsync(id);

            if (compoundDish == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Dish");
            }
            compoundDish.IsAdminReviewed = true;
            _uow.CompoundDishRepository.Update(compoundDish);
            await _uow.CommitAsync();
        }

        public async Task<CompoundDish> UpdateCompoundDishAsync(int ownerId, int id, UpdateCompoundDishRequest dish)
        {
            if (dish.Dishes == null)
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Dishes");

            var compoundDish = await _uow.CompoundDishRepository.GetAll().Where(c => c.Id == id && !c.IsDeleted)
                .Include(c => c.CreatedBy)
                .Include(c => c.DishCompoundDishes)
                .Include(c => c.EatCompoundDishes)
                .Include(d => d.FavoriteDishes)
                .Include(d => d.LackSelfControlDishes)
                .FirstOrDefaultAsync();

            if (compoundDish == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Dish");
            }

            var existName = await _uow.CompoundDishRepository.FindByAsync(c => c.UserId == ownerId && c.Id != id && c.Name == dish.Name && !c.IsDeleted);
            if (existName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Dish name");
            }

            var isInUse = compoundDish.EatCompoundDishes.Count() > 0;

            if (isInUse)
            {
                // Duplicate and return the new one, mark the old one as deleted
                var cd = await AuxCreateCompoundDishAsync(ownerId, dish, compoundDish);

                compoundDish.DeletedAt = DateTime.UtcNow;
                compoundDish.IsDeleted = true;
                await _uow.CompoundDishRepository.UpdateAsync(compoundDish, compoundDish.Id);
                await _uow.CommitAsync();

                // After create the new one then update with new values
                await AuxUpdateCompountDishAsync(cd, dish);
                await _uow.CommitAsync();

                return cd;
            }
            else
            {
                // It is not in use so update it as usually
                await AuxUpdateCompountDishAsync(compoundDish, dish);
                await _uow.CommitAsync();

                return compoundDish;
            }
        }

        public async Task<CompoundDish> GetCompoundDishByIdAsync(int id)
        {
            var cDish = await _uow.CompoundDishRepository.GetAll()
                .Where(c => c.Id == id)
                .Include(d => d.FavoriteDishes)
                .Include(d => d.LackSelfControlDishes)
                .Include(d => d.DishCompoundDishes)
                    .ThenInclude(t => t.Dish)
                .FirstOrDefaultAsync();

            if (cDish == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Compound Dish");

            return cDish;
        }

        public async Task<CompoundDish> AddFavoriteAsync(int loggedUser, int dishId)
        {
            var dish = await GetCompoundDishByIdAsync(dishId);

            var exist = await _uow.FavoriteCompoundDishRepository.GetAll()
                .Where(fd => fd.DishId == dishId && fd.UserId == loggedUser)
                .FirstOrDefaultAsync();

            if (exist != null)
                return dish;

            var favoriteDish = new FavoriteCompoundDishes
            {
                UserId = loggedUser,
                DishId = dishId
            };

            await _uow.FavoriteCompoundDishRepository.AddAsync(favoriteDish);
            await _uow.CommitAsync();

            return dish;
        }

        public async Task RemoveFavoriteDishAsync(int loggedUser, int dishId)
        {
            await GetCompoundDishByIdAsync(dishId);

            var exist = await _uow.FavoriteCompoundDishRepository.GetAll()
                .Where(fd => fd.DishId == dishId && fd.UserId == loggedUser)
                .FirstOrDefaultAsync();

            if (exist != null)
            {
                _uow.FavoriteCompoundDishRepository.Delete(exist);
                await _uow.CommitAsync();
            }
        }

        public async Task<CompoundDish> AddOrUpdateLackselfControlDishAsync(int loggedUser, int dishId, int intensity)
        {
            var dish = await GetCompoundDishByIdAsync(dishId);

            var exist = await _uow.LackSelfControlCompoundDishRepository.GetAll()
                .Where(fd => fd.DishId == dishId && fd.UserId == loggedUser)
                .FirstOrDefaultAsync();

            if (exist != null)
            {
                exist.Intensity = intensity;

                await _uow.LackSelfControlCompoundDishRepository.UpdateAsync(exist, dishId);
                await _uow.CommitAsync();

                return dish;
            }

            var noControlDish = new LackSelfControlCompoundDish
            {
                UserId = loggedUser,
                DishId = dishId,
                Intensity = intensity
            };

            await _uow.LackSelfControlCompoundDishRepository.AddAsync(noControlDish);
            await _uow.CommitAsync();

            return dish;
        }

        public async Task RemoveLackselfControlDishAsync(int loggedUser, int dishId)
        {
            await GetCompoundDishByIdAsync(dishId);

            var exist = await _uow.LackSelfControlCompoundDishRepository.GetAll()
                .Where(fd => fd.DishId == dishId && fd.UserId == loggedUser)
                .FirstOrDefaultAsync();

            if (exist != null)
            {
                _uow.LackSelfControlCompoundDishRepository.Delete(exist);
                await _uow.CommitAsync();
            }
        }

        private async Task<CompoundDish> AuxCreateCompoundDishAsync(int ownerId, CreateCompoundDishRequest dish, CompoundDish existingDish = null)
        {
            var cDish = new CompoundDish();

            if (existingDish != null)
            {
                cDish.Name = existingDish.Name;
                cDish.UserId = existingDish.UserId;
                cDish.CreatedBy = existingDish.CreatedBy;
                cDish.IsAdminConverted = false;
                cDish.IsAdminReviewed = false;

                cDish.IsDeleted = false;
                cDish.ModifiedAt = DateTime.UtcNow;
                cDish.CreatedAt = DateTime.UtcNow;

                foreach (var item in existingDish.DishCompoundDishes)
                {
                    cDish.DishCompoundDishes.Add(new DishCompoundDish
                    {
                        DishId = item.DishId,
                        DishQty = item.DishQty
                    });
                }

                if (existingDish.Image != null)
                {
                    string guid = Guid.NewGuid().ToString();
                    await _fileService.CopyFileAsync(existingDish.Image, guid);
                    cDish.Image = guid;
                    cDish.ImageMimeType = existingDish.ImageMimeType;
                }
            }
            else
            {
                cDish.Name = dish.Name;
                cDish.UserId = ownerId;
                cDish.ModifiedAt = DateTime.UtcNow;
                cDish.CreatedAt = DateTime.UtcNow;

                if (dish.Image != null)
                {
                    string guid = Guid.NewGuid().ToString();
                    await _fileService.UploadFileAsync(dish.Image, guid);
                    cDish.Image = guid;
                    cDish.ImageMimeType = dish.Image.ContentType;
                }

                if (dish.Dishes != null)
                    foreach (var item in dish.Dishes)
                    {
                        var dComDish = new DishCompoundDish
                        {
                            CompoundDish = cDish,
                            DishId = item.DishId,
                            DishQty = item.Qty
                        };
                        await _uow.DishCompoundDishRepository.AddAsync(dComDish);
                    }
            }

            await _uow.CompoundDishRepository.AddAsync(cDish);
            return cDish;
        }

        private async Task AuxUpdateCompountDishAsync(CompoundDish compoundDish, UpdateCompoundDishRequest dish)
        {
            compoundDish.Name = dish.Name;
            compoundDish.ModifiedAt = DateTime.UtcNow;
            if (dish.Image != null)
            {
                if (!string.IsNullOrEmpty(compoundDish.Image))
                    await _fileService.DeleteFileAsync(compoundDish.Image);

                string guid = Guid.NewGuid().ToString();
                await _fileService.UploadFileAsync(dish.Image, guid);
                compoundDish.Image = guid;
                compoundDish.ImageMimeType = dish.Image.ContentType;
            }

            // delete previous dishes
            foreach (var item in compoundDish.DishCompoundDishes)
            {
                _uow.DishCompoundDishRepository.Delete(item);
            }

            if (dish.Dishes != null)
                foreach (var item in dish.Dishes)
                {
                    var dComDish = new DishCompoundDish
                    {
                        CompoundDishId = compoundDish.Id,
                        DishId = item.DishId,
                        DishQty = item.Qty
                    };
                    await _uow.DishCompoundDishRepository.AddAsync(dComDish);
                }

            _uow.CompoundDishRepository.Update(compoundDish);
        }
    }
}