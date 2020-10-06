using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Dish;
using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IDishService
    {
        Task<PaginatedList<Dish>> GetDishesAsync(string search, List<int> tags, int? page, int? perPage, int? harvardFilter);

        Task<Dish> GetDishByIdAsync(int id);

        Task<Dish> CreateDishAsync(int loggedUser, CreateDishRequest dish);

        Task<Dish> UpdateDishAsync(int loggedUser, UpdateDishRequest dish);

        Task DeleteDishAsync(int loggedUser, int id);

        Task ChangeDishTranslationAsync(int loggedUser, DishTranslationRequest dishTranslationRequest, int id);

        Task<IEnumerable<Dish>> GetDishesAdminAsync(int loggedUser);

        Task<PaginatedList<Dish>> GetFavoriteDishesAsync(int loggedUser, string search, List<int> tags, int? page, int? perPage, int? harvardFilter);

        Task<Dish> AddFavoriteDishAsync(int loggedUser, int dishId);

        Task RemoveFavoriteDishAsync(int loggedUser, int dishId);

        Task<double> GetConversionFactorAsync(int height, int sex, int code);
    }
}