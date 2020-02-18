using MismeAPI.Common.DTO.Request;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IDishService
    {
        Task<IEnumerable<Dish>> GetDishesAsync(string search);

        Task<Dish> GetDishByIdAsync(int id);

        Task<Dish> CreateDishAsync(int loggedUser, CreateDishRequest dish);
    }
}