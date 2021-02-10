using MismeAPI.Common.DTO;
using MismeAPI.Common.DTO.Group;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Services.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IGroupService
    {
        Task<PaginatedList<Group>> GetGroupsAsync(int pag, int perPag, string sortOrder, string search, bool? isActive);

        Task<Group> GetGroupAsync(int groupId);

        Task<Group> GetGroupByNameAsync(string groupName);

        Task<Group> GetCurrentUserGroupAsync(int userId);

        Task<(Group Group, string GeneratedPassword)> CreateGroupAsync(CreateGroupRequest request);

        Task<(Group Group, string GeneratedPassword)> UpdateGroupAsync(int groupId, AdminUpdateGroupRequest request);

        Task<Group> UpdateGroupLimitedAsync(int groupId, UpdateGroupRequest request);

        Task<Group> UpdateGroupActiveStatusAsync(int groupId, bool isActive);

        Task DeleteGroupAsync(int groupId);

        Task<(ICollection<GroupInviteActionResponse> result, ICollection<GroupInvitation> invitations)> InviteUsersToGroupAsync(int groupId, ICollection<EmailRequest> emails);

        Task DeleteGroupInvitationAsync(int invitationId);

        Task<GroupInvitation> UpdateGroupInvitationAsync(int invitationId, StatusInvitationEnum status, string token);

        Task<PaginatedList<GroupInvitation>> GetInvitationsAsync(int groupId, int pag, int perPag, string sortOrder, string search, ICollection<StatusInvitationEnum> statuses);

        Task<PaginatedList<User>> GetUsersAsync(int groupId, int pag, int perPag, string sortOrder, string search);
    }
}
