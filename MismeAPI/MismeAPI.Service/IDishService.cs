using MismeAPI.Common.DTO.Request;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IDishService
    {
        Task<IEnumerable<Dish>> GetDishesAsync(string search, List<int> tags);

        Task<Dish> GetDishByIdAsync(int id);

        Task<Dish> CreateDishAsync(int loggedUser, CreateDishRequest dish);

        Task<Dish> UpdateDishAsync(int loggedUser, UpdateDishRequest dish);

        Task DeleteDishAsync(int loggedUser, int id);
    }
}