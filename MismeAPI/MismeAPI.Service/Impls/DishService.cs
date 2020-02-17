using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class DishService : IDishService
    {
        private readonly IUnitOfWork _uow;

        public DishService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
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