﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Dish;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data;
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
    public class DishService : IDishService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFileService _fileService;
        private readonly MismeContext _context;
        private readonly IConfiguration _config;

        public DishService(IUnitOfWork uow, IFileService fileService, MismeContext context, IConfiguration config)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task ChangeDishTranslationAsync(int loggedUser, DishTranslationRequest dishTranslationRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var dish = await _uow.DishRepository.GetAll().Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            if (dish == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Dish");
            }

            switch (dishTranslationRequest.Lang)
            {
                case "en":
                    dish.NameEN = dishTranslationRequest.Name;
                    break;

                case "it":
                    dish.NameIT = dishTranslationRequest.Name;
                    break;

                default:
                    dish.Name = dishTranslationRequest.Name;
                    break;
            }

            _uow.DishRepository.Update(dish);
            await _uow.CommitAsync();
            //expire cache

            QueryCacheManager.ExpireTag(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);
        }

        public async Task<Dish> CreateDishAsync(int loggedUser, CreateDishRequest dish)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            // validate dish name
            var existDishName = await _uow.DishRepository.FindByAsync(p => p.Name.ToLower() == dish.Name.ToLower());
            if (existDishName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Dish name");
            }

            var dbDish = new Dish();
            dbDish.Calories = dish.Calories;
            dbDish.Carbohydrates = dish.Carbohydrates;
            dbDish.Fat = dish.Fat;
            dbDish.Fiber = dish.Fiber;
            dbDish.Name = dish.Name;
            dbDish.Proteins = dish.Proteins;

            dbDish.Cholesterol = dish.Cholesterol;
            dbDish.Calcium = dish.Calcium;
            dbDish.Phosphorus = dish.Phosphorus;
            dbDish.Iron = dish.Iron;
            dbDish.Potassium = dish.Potassium;
            dbDish.Sodium = dish.Sodium;
            dbDish.Zinc = dish.Zinc;
            dbDish.VitaminA = dish.VitaminA;
            dbDish.VitaminC = dish.VitaminC;
            dbDish.VitaminB6 = dish.VitaminB6;
            dbDish.VitaminB12 = dish.VitaminB12;
            dbDish.VitaminD = dish.VitaminD;
            dbDish.VitaminE = dish.VitaminE;
            dbDish.VitaminK = dish.VitaminK;

            dbDish.VitaminB1Thiamin = dish.VitaminB1Thiamin;
            dbDish.VitaminB2Riboflavin = dish.VitaminB2Riboflavin;
            dbDish.VitaminB3Niacin = dish.VitaminB3Niacin;
            dbDish.VitaminB9Folate = dish.VitaminB9Folate;
            dbDish.NetWeight = dish.NetWeight;
            dbDish.Volume = dish.Volume;
            dbDish.SaturatedFat = dish.SaturatedFat;
            dbDish.PolyUnsaturatedFat = dish.PolyUnsaturatedFat;
            dbDish.MonoUnsaturatedFat = dish.MonoUnsaturatedFat;
            dbDish.Alcohol = dish.Alcohol;

            if (dish.Classification == 0)
            {
                dbDish.IsProteic = true;
            }
            else if (dish.Classification == 1)
            {
                dbDish.IsCaloric = true;
            }
            else
            {
                dbDish.IsFruitAndVegetables = true;
            }

            // avatar

            if (dish.Image != null)
            {
                string guid = Guid.NewGuid().ToString();
                await _fileService.UploadFileAsync(dish.Image, guid);
                dbDish.Image = guid;
                dbDish.ImageMimeType = dish.Image.ContentType;
            }

            //existing tags
            foreach (var id in dish.TagsIds)
            {
                var t = await _uow.TagRepository.GetAsync(id);
                if (t != null)
                {
                    var dt = new DishTag
                    {
                        Dish = dbDish,
                        TagId = t.Id,
                        TaggedAt = DateTime.UtcNow
                    };

                    await _uow.DishTagRepository.AddAsync(dt);
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
                    Dish = dbDish,
                    Tag = ta,
                    TaggedAt = DateTime.UtcNow
                };

                await _uow.DishTagRepository.AddAsync(dt);
            }

            await _uow.DishRepository.AddAsync(dbDish);
            await _uow.CommitAsync();

            //expire cache
            QueryCacheManager.ExpireTag(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);

            return dbDish;
        }

        public async Task DeleteDishAsync(int loggedUser, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var dish = await _uow.DishRepository.GetAsync(id);
            if (dish == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Dish");
            }

            if (!string.IsNullOrWhiteSpace(dish.Image))
            {
                await _fileService.DeleteFileAsync(dish.Image);
            }
            _uow.DishRepository.Delete(dish);
            await _uow.CommitAsync();

            //expire cache
            QueryCacheManager.ExpireTag(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);
        }

        public async Task<Dish> GetDishByIdAsync(int id)
        {
            var dish = await _uow.DishRepository.GetAll().Where(d => d.Id == id)
                .Include(d => d.FavoriteDishes)
                .Include(d => d.LackSelfControlDishes)
                .Include(d => d.DishTags)
                    .ThenInclude(t => t.Tag)
                .FirstOrDefaultAsync();
            if (dish == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Dish");
            }
            return dish;
        }

        public async Task<IEnumerable<Dish>> GetDishesAdminAsync(int loggedUser)
        {
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var dishes = await _uow.DishRepository.GetAll().ToListAsync();
            return dishes;
        }

        private async Task<IEnumerable<Dish>> GetExactMatches(string search)
        {
            search = search.Trim();
            //var results = _uow.DishRepository.GetAll().Where(r => r.Name.ToLower().Equals(search.ToLower()))
            //    .Include(d => d.DishTags)
            //        .ThenInclude(t => t.Tag)
            //.ToList();

            var results = await _context.Dishes
                .Include(d => d.FavoriteDishes)
                .Include(d => d.LackSelfControlDishes)
                .Include(d => d.DishTags)
                    .ThenInclude(t => t.Tag)
                .FromCacheAsync(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);

            results = results.Where(r => r.Name.ToLower().Equals(search.ToLower()));

            //var results = await _context.Dishes.Where(r => r.Name.ToLower().Equals(search.ToLower()))
            //    .Include(d => d.DishTags)
            //        .ThenInclude(t => t.Tag)
            //.FromCacheAsync(CacheEntries.ALL_DISHES);
            return results;
        }

        private async Task<IEnumerable<Dish>> GetStartWithMatches(string search)
        {
            search = search.Trim();
            //var results = _uow.DishRepository.GetAll().Where(r => r.Name.ToLower() != search.ToLower() && r.Name.ToLower().StartsWith(search.ToLower()))
            //    .Include(d => d.DishTags)
            //        .ThenInclude(t => t.Tag)
            //    .OrderBy(r => r.Name)
            //    .ToList();

            var results = await _context.Dishes
                         .Include(d => d.FavoriteDishes)
                         .Include(d => d.LackSelfControlDishes)
                         .Include(d => d.DishTags)
                             .ThenInclude(t => t.Tag)
                         .FromCacheAsync(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);

            results = results.Where(r => r.Name.ToLower() != search.ToLower() && r.Name.ToLower().StartsWith(search.ToLower()))
                             .OrderBy(r => r.Name);

            //var results = await _context.Dishes.Where(r => r.Name.ToLower() != search.ToLower() && r.Name.ToLower().StartsWith(search.ToLower()))
            //    .Include(d => d.DishTags)
            //        .ThenInclude(t => t.Tag)
            //    .OrderBy(r => r.Name)
            //    .FromCacheAsync(CacheEntries.ALL_DISHES);
            return results;
        }

        private async Task<IEnumerable<Dish>> GetContainsMatches(string search)
        {
            search = search.Trim();

            var results = await _context.Dishes
                           .Include(d => d.FavoriteDishes)
                           .Include(d => d.LackSelfControlDishes)
                           .Include(d => d.DishTags)
                               .ThenInclude(t => t.Tag)
                           .FromCacheAsync(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);

            results = results.Where(r => r.Name.ToLower() != search.ToLower()
                                     && !r.Name.ToLower().StartsWith(search.ToLower())
                                     && r.Name.ToLower().Contains(search.ToLower()))
                             .OrderBy(r => r.Name);

            //var results = await _context.Dishes.Where(r => r.Name.ToLower() != search.ToLower()
            //                       && !r.Name.ToLower().StartsWith(search.ToLower())
            //                       && r.Name.ToLower().Contains(search.ToLower()))
            //                .Include(d => d.DishTags)
            //                    .ThenInclude(t => t.Tag)
            //                 .OrderBy(r => r.Name)
            //                .FromCacheAsync(CacheEntries.ALL_DISHES);

            //var results = _uow.DishRepository.GetAll().Where(r => r.Name.ToLower() != search.ToLower()
            //                && !r.Name.ToLower().StartsWith(search.ToLower())
            //                && r.Name.ToLower().Contains(search.ToLower()))
            //                .Include(d => d.DishTags)
            //                    .ThenInclude(t => t.Tag)
            //                .OrderBy(r => r.Name)
            //                .ToList();
            return results;
        }

        public async Task<PaginatedList<Dish>> GetDishesAsync(string search, List<int> tags, int? page, int? perPage, int? harvardFilter, string sort)
        {
            //var results = _uow.DishRepository.GetAll()
            //    .Include(d => d.DishTags)
            //        .ThenInclude(t => t.Tag).AsQueryable();
            ////.FromCacheAsync(CacheEntries.ALL_DISHES);

            var results = await _context.Dishes
                           .Include(d => d.FavoriteDishes)
                           .Include(d => d.LackSelfControlDishes)
                           .Include(d => d.DishTags)
                               .ThenInclude(t => t.Tag)
                           .FromCacheAsync(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var equals = await GetExactMatches(search);

                var startsWith = await GetStartWithMatches(search);
                var contains = await GetContainsMatches(search);

                results = (equals.Union(startsWith).Union(contains)).AsQueryable();
            }
            else
            {
                results = results.OrderBy(d => d.Name);
            }
            if (tags.Count > 0)
            {
                var total = new List<Dish>().AsQueryable();
                foreach (var t in tags)
                {
                    total = total.Union(results.Where(d => d.DishTags.Any(d => d.TagId == t)));
                }
                results = total;
            }
            if (harvardFilter.HasValue)
            {
                switch (harvardFilter.Value)
                {
                    // proteic
                    case 0:
                        results = results.Where(r => r.IsProteic == true);
                        break;
                    // caloric
                    case 1:
                        results = results.Where(r => r.IsCaloric == true);
                        break;

                    default:
                        results = results.Where(r => r.IsFruitAndVegetables == true);
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

                    case "calories_desc":
                        results = results.OrderByDescending(i => i.Calories);
                        break;

                    case "calories_asc":
                        results = results.OrderBy(i => i.Calories);
                        break;

                    case "proteins_desc":
                        results = results.OrderByDescending(i => i.Proteins);
                        break;

                    case "proteins_asc":
                        results = results.OrderBy(i => i.Proteins);
                        break;

                    case "carbohydrates_desc":
                        results = results.OrderByDescending(i => i.Carbohydrates);
                        break;

                    case "carbohydrates_asc":
                        results = results.OrderBy(i => i.Carbohydrates);
                        break;

                    case "fiber_desc":
                        results = results.OrderByDescending(i => i.Fiber);
                        break;

                    case "fiber_asc":
                        results = results.OrderBy(i => i.Fiber);
                        break;

                    case "fat_desc":
                        results = results.OrderByDescending(i => i.Fat);
                        break;

                    case "fat_asc":
                        results = results.OrderBy(i => i.Fat);
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<Dish>.CreateAsync(results.AsQueryable(), page ?? 1, page.HasValue == false ? results.Count() : perPage ?? 10);
        }

        public async Task<PaginatedList<Dish>> GetFavoriteDishesAsync(int loggedUser, string search, List<int> tags, int? page, int? perPage, int? harvardFilter)
        {
            var results = _uow.DishRepository.GetAll()
                .Where(d => d.FavoriteDishes.Any(fd => fd.UserId == loggedUser))
                .Include(d => d.FavoriteDishes)
                .Include(d => d.LackSelfControlDishes)
                .Include(d => d.DishTags)
                    .ThenInclude(t => t.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                results = results.Where(r => r.Name.ToLower().Contains(search.ToLower()));
            }
            if (tags.Count > 0)
            {
                foreach (var t in tags)
                {
                    results = results.Where(d => d.DishTags.Any(d => d.TagId == t));
                }
            }
            if (harvardFilter.HasValue)
            {
                switch (harvardFilter.Value)
                {
                    // proteic
                    case 0:
                        results = results.Where(r => r.IsProteic == true);
                        break;
                    // caloric
                    case 1:
                        results = results.Where(r => r.IsCaloric == true);
                        break;

                    default:
                        results = results.Where(r => r.IsFruitAndVegetables == true);
                        break;
                }
            }

            return await PaginatedList<Dish>.CreateAsync(results, page ?? 1, page.HasValue == false ? results.Count() : perPage ?? 10);
        }

        public async Task<PaginatedList<Dish>> GetLackSelfControlDishesAsync(int loggedUser, string search, List<int> tags, int? page, int? perPage, int? harvardFilter)
        {
            var results = _uow.DishRepository.GetAll()
                .Where(d => d.LackSelfControlDishes.Any(fd => fd.UserId == loggedUser))
                .Include(d => d.FavoriteDishes)
                .Include(d => d.LackSelfControlDishes)
                .Include(d => d.DishTags)
                    .ThenInclude(t => t.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                results = results.Where(r => r.Name.ToLower().Contains(search.ToLower()));
            }
            if (tags.Count > 0)
            {
                foreach (var t in tags)
                {
                    results = results.Where(d => d.DishTags.Any(d => d.TagId == t));
                }
            }
            if (harvardFilter.HasValue)
            {
                switch (harvardFilter.Value)
                {
                    // proteic
                    case 0:
                        results = results.Where(r => r.IsProteic == true);
                        break;
                    // caloric
                    case 1:
                        results = results.Where(r => r.IsCaloric == true);
                        break;

                    default:
                        results = results.Where(r => r.IsFruitAndVegetables == true);
                        break;
                }
            }

            return await PaginatedList<Dish>.CreateAsync(results, page ?? 1, page.HasValue == false ? results.Count() : perPage ?? 10);
        }

        public async Task<Dish> UpdateDishAsync(int loggedUser, UpdateDishRequest dish)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var dishh = await _uow.DishRepository.GetAll().Where(d => d.Id == dish.Id)
                .Include(d => d.FavoriteDishes)
                .Include(d => d.LackSelfControlDishes)
                .Include(d => d.DishTags)
                    .ThenInclude(t => t.Tag)
                .FirstOrDefaultAsync();
            if (dishh == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Dish");
            }

            // validate dish name
            var existDishName = await _uow.DishRepository.FindByAsync(p => p.Name.ToLower() == dish.Name.ToLower() && p.Id != dish.Id);
            if (existDishName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Dish name");
            }

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
                dishh.IsCaloric = false;
                dishh.IsFruitAndVegetables = false;
            }
            else if (dish.Classification == 1)
            {
                dishh.IsCaloric = true;
                dishh.IsProteic = false;
                dishh.IsFruitAndVegetables = false;
            }
            else
            {
                dishh.IsFruitAndVegetables = true;
                dishh.IsProteic = false;
                dishh.IsCaloric = false;
            }

            // avatar

            if (!string.IsNullOrWhiteSpace(dish.RemovedImage) && dish.RemovedImage != "null")
            {
                await _fileService.DeleteFileAsync(dishh.Image);
                dishh.Image = "";
                dishh.ImageMimeType = "";
            }
            if (dish.Image != null)
            {
                string guid = Guid.NewGuid().ToString();
                await _fileService.UploadFileAsync(dish.Image, guid);
                dishh.Image = guid;
                dishh.ImageMimeType = dish.Image.ContentType;
            }

            //delete current and removed tags
            var results = _uow.DishTagRepository.GetAll().Where(dt => dish.Id == dt.DishId && !dish.TagsIds.Contains(dt.TagId));
            foreach (var dt in results)
            {
                _uow.DishTagRepository.Delete(dt);
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

            await _uow.DishRepository.UpdateAsync(dishh, dishh.Id);
            await _uow.CommitAsync();

            //expire cache
            QueryCacheManager.ExpireTag(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);
            return dishh;
        }

        public async Task<Dish> AddFavoriteDishAsync(int loggedUser, int dishId)
        {
            var dish = await GetDishByIdAsync(dishId);

            var exist = await _uow.FavoriteDishRepository.GetAll()
                .Where(fd => fd.DishId == dishId && fd.UserId == loggedUser)
                .FirstOrDefaultAsync();

            if (exist != null)
                return dish;

            var favoriteDish = new FavoriteDish
            {
                UserId = loggedUser,
                DishId = dishId
            };

            await _uow.FavoriteDishRepository.AddAsync(favoriteDish);
            await _uow.CommitAsync();

            QueryCacheManager.ExpireTag(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);

            return dish;
        }

        public async Task RemoveFavoriteDishAsync(int loggedUser, int dishId)
        {
            await GetDishByIdAsync(dishId);

            var exist = await _uow.FavoriteDishRepository.GetAll()
                .Where(fd => fd.DishId == dishId && fd.UserId == loggedUser)
                .FirstOrDefaultAsync();

            if (exist != null)
            {
                _uow.FavoriteDishRepository.Delete(exist);
                await _uow.CommitAsync();

                QueryCacheManager.ExpireTag(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);
            }
        }

        public async Task<Dish> AddOrUpdateLackselfControlDishAsync(int loggedUser, int dishId, int intensity)
        {
            var dish = await GetDishByIdAsync(dishId);

            var exist = await _uow.LackSelfControlDishRepository.GetAll()
                .Where(fd => fd.DishId == dishId && fd.UserId == loggedUser)
                .FirstOrDefaultAsync();

            if (exist != null)
            {
                exist.Intensity = intensity;

                await _uow.LackSelfControlDishRepository.UpdateAsync(exist, dishId);
                await _uow.CommitAsync();

                QueryCacheManager.ExpireTag(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);

                return dish;
            }

            var noControlDish = new LackSelfControlDish
            {
                UserId = loggedUser,
                DishId = dishId,
                Intensity = intensity
            };

            await _uow.LackSelfControlDishRepository.AddAsync(noControlDish);
            await _uow.CommitAsync();

            QueryCacheManager.ExpireTag(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);

            return dish;
        }

        public async Task RemoveLackselfControlDishAsync(int loggedUser, int dishId)
        {
            await GetDishByIdAsync(dishId);

            var exist = await _uow.LackSelfControlDishRepository.GetAll()
                .Where(fd => fd.DishId == dishId && fd.UserId == loggedUser)
                .FirstOrDefaultAsync();

            if (exist != null)
            {
                _uow.LackSelfControlDishRepository.Delete(exist);
                await _uow.CommitAsync();

                QueryCacheManager.ExpireTag(_config.GetSection("AWS")["CachePrefix"] + CacheEntries.ALL_DISHES);
            }
        }

        public async Task<double> GetConversionFactorAsync(int height, int sex, int code)
        {
            var factor = await _uow.HandConversionFactorRepository.GetAll().Where(c => c.Height == height && c.Gender == (GenderEnum)sex).FirstOrDefaultAsync();
            if (factor != null)
            {
                if (code == 3)
                {
                    return factor.ConversionFactor3Code;
                }
                else if (code == 6)
                {
                    return factor.ConversionFactor6Code;
                }
                else if (code == 10)
                {
                    return factor.ConversionFactor10Code;
                }
                else if (code == 11)
                {
                    return factor.ConversionFactor11Code;
                }
                else if (code == 19)
                {
                    return factor.ConversionFactor19Code;
                }
                else if (code == 4)
                {
                    return factor.ConversionFactor4Code;
                }
                else
                {
                    return 1.0;
                }
            }
            return 1.0;
        }
    }
}