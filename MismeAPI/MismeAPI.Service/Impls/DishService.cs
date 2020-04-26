using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Dish;
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
    public class DishService : IDishService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFileService _fileService;

        public DishService(IUnitOfWork uow, IFileService fileService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
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
        }

        public async Task<Dish> GetDishByIdAsync(int id)
        {
            var dish = await _uow.DishRepository.GetAll().Where(d => d.Id == id)
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

        public async Task<IEnumerable<Dish>> GetDishesAsync(string search, List<int> tags)
        {
            var results = _uow.DishRepository.GetAll()
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
            return await results.ToListAsync();

            //var cache = QueryCacheManager.Cache;

            //var dishes = _context.Dishes
            //      .Include(d => d.DishTags)
            //        .ThenInclude(t => t.Tag)
            //      .FromCache("all_dishes");

            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    dishes = dishes.Where(r => r.Name.ToLower().Contains(search.ToLower()));
            //}
            //if (tags.Count > 0)
            //{
            //    foreach (var t in tags)
            //    {
            //        dishes = dishes.Where(d => d.DishTags.Any(d => d.TagId == t));
            //    }
            //}
            //return await Task.FromResult(dishes);
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

            return dishh;
        }
    }
}