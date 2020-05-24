using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.CompoundDish;
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
    public class CompoundDishService : ICompoundDishService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFileService _fileService;

        public CompoundDishService(IUnitOfWork uow, IFileService fileService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
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
        }

        public async Task<CompoundDish> CreateCompoundDishAsync(int ownerId, CreateCompoundDishRequest dish)
        {
            var existName = await _uow.CompoundDishRepository.FindByAsync(c => c.UserId == ownerId && c.Name == dish.Name);
            if (existName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Dish name");
            }

            var cDish = new CompoundDish();
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

            await _uow.CompoundDishRepository.AddAsync(cDish);
            await _uow.CommitAsync();

            return cDish;
        }

        public async Task DeleteCompoundDishAsync(int ownerId, int compoundDishId)
        {
            var c = await _uow.CompoundDishRepository.FindAsync(c => c.Id == compoundDishId && c.UserId == ownerId);

            if (c == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Compound Dish");
            }
            _uow.CompoundDishRepository.Delete(c);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<CompoundDish>> GetAllCompoundDishesAsync(int adminId, string search, int filter)
        {
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == adminId && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var results = _uow.CompoundDishRepository.GetAll()
                .Include(d => d.CreatedBy)
                .Include(d => d.DishCompoundDishes)
                  .ThenInclude(t => t.Dish)

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

            return await results.ToListAsync();
        }

        public async Task<IEnumerable<CompoundDish>> GetUserCompoundDishesAsync(int ownerId, string search)
        {
            var results = _uow.CompoundDishRepository.GetAll().Where(c => c.UserId == ownerId)
              .Include(d => d.DishCompoundDishes)
                  .ThenInclude(t => t.Dish)
              .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                results = results.Where(r => r.Name.ToLower().Contains(search.ToLower()));
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
            var compoundDish = await _uow.CompoundDishRepository.GetAll().Where(c => c.Id == id)
                .Include(c => c.DishCompoundDishes)
                .FirstOrDefaultAsync();

            if (compoundDish == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Dish");
            }

            var existName = await _uow.CompoundDishRepository.FindByAsync(c => c.UserId == ownerId && c.Id != id && c.Name == dish.Name);
            if (existName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Dish name");
            }

            compoundDish.Name = dish.Name;
            compoundDish.ModifiedAt = DateTime.UtcNow;
            if (dish.Image != null)
            {
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

            foreach (var item in dish.Dishes)
            {
                var dComDish = new DishCompoundDish
                {
                    CompoundDishId = id,
                    DishId = item.DishId,
                    DishQty = item.Qty
                };
                await _uow.DishCompoundDishRepository.AddAsync(dComDish);
            }

            _uow.CompoundDishRepository.Update(compoundDish);
            await _uow.CommitAsync();

            return compoundDish;
        }
    }
}