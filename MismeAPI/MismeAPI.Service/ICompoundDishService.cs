using MismeAPI.Common.DTO.Request.CompoundDish;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface ICompoundDishService
    {
        Task<IEnumerable<CompoundDish>> GetUserCompoundDishesAsync(int ownerId, string search);

        Task<IEnumerable<CompoundDish>> GetAllCompoundDishesAsync(int adminId, string search);

        Task<CompoundDish> CreateCompoundDishAsync(int ownerId, CreateCompoundDishRequest dish);

        Task DeleteCompoundDishAsync(int ownerId, int compoundDishId);

        Task<CompoundDish> UpdateCompoundDishAsync(int loggedUser, int id, UpdateCompoundDishRequest dish);
    }
}