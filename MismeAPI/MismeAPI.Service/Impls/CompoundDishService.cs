using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
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

        public async Task<IEnumerable<CompoundDish>> GetAllCompoundDishesAsync(int adminId, string search)
        {
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == adminId && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var results = _uow.CompoundDishRepository.GetAll()
              .Include(d => d.DishCompoundDishes)
                  .ThenInclude(t => t.Dish)
              .Include(d => d.CreatedBy)
              .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                results = results.Where(r => r.Name.ToLower().Contains(search.ToLower()));
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