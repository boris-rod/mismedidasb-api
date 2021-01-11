using MismeAPI.Common.DTO.Group;
using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IGroupService
    {
        Task<PaginatedList<Group>> GetGroupsAsync(int pag, int perPag, string sortOrder);

        Task<Group> GetGroupAsync(int groupId);

        Task<(Group Group, string GeneratedPassword)> CreateGroupAsync(CreateGroupRequest request);

        Task<(Group Group, string GeneratedPassword)> UpdateGroupAsync(int groupId, UpdateGroupRequest request);

        Task DeleteGroupAsync(int groupId);

        Task<bool> ValidateGroupAdminFirstLoginAsync(User user);
    }
}
