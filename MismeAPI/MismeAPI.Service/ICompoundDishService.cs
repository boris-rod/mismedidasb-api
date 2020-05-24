using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.CompoundDish;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface ICompoundDishService
    {
        Task<IEnumerable<CompoundDish>> GetUserCompoundDishesAsync(int ownerId, string search);

        Task<IEnumerable<CompoundDish>> GetAllCompoundDishesAsync(int adminId, string search, int filter);

        Task<CompoundDish> CreateCompoundDishAsync(int ownerId, CreateCompoundDishRequest dish);

        Task DeleteCompoundDishAsync(int ownerId, int compoundDishId);

        Task<CompoundDish> UpdateCompoundDishAsync(int loggedUser, int id, UpdateCompoundDishRequest dish);

        Task MarkCompoundDishAsReviewedAsync(int loggedUser, int id);

        Task ConvertUserDishAsync(int loggedUser, UpdateDishRequest dish);
    }
}