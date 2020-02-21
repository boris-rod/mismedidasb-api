using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
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

        public async Task<IEnumerable<Dish>> GetDishesAsync(string search)
        {
            var results = _uow.DishRepository.GetAll()
                .Include(d => d.DishTags)
                    .ThenInclude(t => t.Tag)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                results = results.Where(r => r.Name.ToLower().Contains(search.ToLower()));
            }
            return await results.ToListAsync();
        }
    }
}