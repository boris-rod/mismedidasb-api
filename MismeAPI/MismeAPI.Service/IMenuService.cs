using MismeAPI.Common.DTO.Menu;
using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IMenuService
    {
        Task<PaginatedList<Menu>> GetMenuesAsync(int? groupId, int pag, int perPag, bool? active, int currentUser);

        Task<Menu> GetMenuAsync(int menuId);

        Task<Menu> GetMenuWithEatsAsync(int menuId);

        Task<Menu> CreateMenuAsync(CreateUpdateMenuRequest menuRequest, int loggedUser);

        Task<Menu> UpdateMenuAsync(int menuId, CreateUpdateMenuRequest menuRequest);

        Task<Menu> PatchMenuStatusAsync(int menuId, bool activeStatus);

        Task<Menu> UpdateBulkMenuEatsAsync(int menuId, MenuBulkEatsUpdateRequest menuRequest);

        Task DeleteAsync(int menuId);
    }
}
