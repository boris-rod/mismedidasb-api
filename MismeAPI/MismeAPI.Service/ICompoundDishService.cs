using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.CompoundDish;
using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface ICompoundDishService
    {
        Task<IEnumerable<CompoundDish>> GetUserCompoundDishesAsync(int ownerId, string search, bool? favorites, bool? lackSelfControl);

        Task<PaginatedList<CompoundDish>> GetAllCompoundDishesAsync(int adminId, string search, int filter, int page, int perPage, string sort);

        Task<CompoundDish> CreateCompoundDishAsync(int ownerId, CreateCompoundDishRequest dish);

        Task DeleteCompoundDishAsync(int ownerId, int compoundDishId);

        Task<CompoundDish> UpdateCompoundDishAsync(int loggedUser, int id, UpdateCompoundDishRequest dish);

        Task MarkCompoundDishAsReviewedAsync(int loggedUser, int id);

        Task ConvertUserDishAsync(int loggedUser, UpdateDishRequest dish);

        Task<CompoundDish> AddFavoriteAsync(int loggedUser, int dishId);

        Task RemoveFavoriteDishAsync(int loggedUser, int dishId);

        Task<CompoundDish> AddOrUpdateLackselfControlDishAsync(int loggedUser, int dishId, int intensity);

        Task RemoveLackselfControlDishAsync(int loggedUser, int dishId);
    }
}