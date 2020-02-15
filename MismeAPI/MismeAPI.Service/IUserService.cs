using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IUserService
    {
        Task<PaginatedList<User>> GetUsersAsync(int loggedUser, int pag, int perPag, string sortOrder, int statusFilter, string search);
    }
}