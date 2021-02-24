using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO;
using MismeAPI.Common.DTO.Group;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Service.Utils;
using MismeAPI.Services;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly INotificationService _notificationService;
        private readonly IAccountService _accountService;

        public GroupService(IUnitOfWork uow, IUserService userService, IConfiguration config, INotificationService notificationService, IAccountService accountService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public async Task<PaginatedList<Group>> GetGroupsAsync(int pag, int perPag, string sortOrder, string search, bool? isActive)
        {
            var result = _uow.GroupRepository.GetAll()
                .Include(g => g.Users)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(i => i.Name.ToLower().Contains(search.ToLower()) || i.AdminEmail.ToLower().Contains(search.ToLower()));
            }

            if (isActive.HasValue)
            {
                result = result.Where(i => i.IsActive == isActive.Value);
            }

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "name_desc":
                        result = result.OrderByDescending(i => i.Name);
                        break;

                    case "name_asc":
                        result = result.OrderBy(i => i.Name);
                        break;

                    case "description_desc":
                        result = result.OrderByDescending(i => i.Description);
                        break;

                    case "description_asc":
                        result = result.OrderBy(i => i.Description);
                        break;

                    case "createdAt_desc":
                        result = result.OrderByDescending(i => i.CreatedAt);
                        break;

                    case "createdAt_asc":
                        result = result.OrderBy(i => i.CreatedAt);
                        break;

                    case "isActive_desc":
                        result = result.OrderByDescending(i => i.IsActive);
                        break;

                    case "isActive_asc":
                        result = result.OrderBy(i => i.IsActive);
                        break;

                    case "adminEmail_desc":
                        result = result.OrderByDescending(i => i.AdminEmail);
                        break;

                    case "adminEmail_asc":
                        result = result.OrderBy(i => i.AdminEmail);
                        break;

                    case "usersCount_desc":
                        result = result.OrderByDescending(i => i.Users.Count());
                        break;

                    case "usersCount_asc":
                        result = result.OrderBy(i => i.Users.Count());
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<Group>.CreateAsync(result, pag, perPag);
        }

        public async Task<Group> GetGroupAsync(int groupId)
        {
            var group = await _uow.GroupRepository.GetAll()
                .Include(g => g.Users)
                .Include(g => g.Invitations)
                    .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Group");

            return group;
        }

        public async Task<Group> GetCurrentUserGroupAsync(int userId)
        {
            var group = await _uow.GroupRepository.GetAll()
               .Include(g => g.Users)
               .Include(g => g.Invitations)
                    .ThenInclude(i => i.User)
               .FirstOrDefaultAsync(g => g.Users.Any(u => u.Id == userId));

            if (group == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Group");

            return group;
        }

        /// <summary>
        /// Get group by name
        /// </summary>
        /// <param name="groupName">group name</param>
        /// <returns>The Group or null if not found</returns>
        public async Task<Group> GetGroupByNameAsync(string groupName)
        {
            var group = await _uow.GroupRepository.GetAll()
                .Include(g => g.Users)
                .Include(g => g.Invitations)
                    .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(g => g.Name == groupName);

            return group;
        }

        public async Task<(Group Group, string GeneratedPassword)> CreateGroupAsync(CreateGroupRequest request)
        {
            var group = await GetGroupByNameAsync(request.Name);

            if (group != null)
            {
                throw new AlreadyExistsException("El nombre del grupo ya esta en uso.");
            }

            group = new Group
            {
                Name = request.Name,
                Description = request.Description,
                AdminEmail = request.AdminEmail,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            await _uow.GroupRepository.AddAsync(group);

            // CommitAsync method is called inside this helper method.
            var generatedPasword = await SetGroupAdminAsyn(group, request.AdminEmail, request.Language);

            return (group, generatedPasword);
        }

        public async Task<(Group Group, string GeneratedPassword)> UpdateGroupAsync(int groupId, AdminUpdateGroupRequest request)
        {
            var generatedPasword = string.Empty;
            var group = await GetGroupAsync(groupId);

            if (group == null)
                throw new NotFoundException("Group");

            if (group.Name != request.Name)
            {
                var existGroup = await GetGroupByNameAsync(request.Name);
                if (existGroup != null)
                {
                    throw new AlreadyExistsException("El nombre del grupo ya esta en uso.");
                }
            }

            if (group.AdminEmail != request.AdminEmail)
            {
                // TODO: Remove old admin user if it is inactive yet?

                generatedPasword = await SetGroupAdminAsyn(group, request.AdminEmail, request.Language);
            }

            group.Name = request.Name;
            group.Description = request.Description;
            group.AdminEmail = request.AdminEmail;
            group.ModifiedAt = DateTime.UtcNow;

            await _uow.GroupRepository.UpdateAsync(group, groupId);
            await _uow.CommitAsync();

            return (group, generatedPasword);
        }

        public async Task<Group> UpdateGroupLimitedAsync(int groupId, UpdateGroupRequest request)
        {
            var group = await GetGroupAsync(groupId);

            if (group == null)
                throw new NotFoundException("Group");

            if (group.Name != request.Name)
            {
                var existGroup = await GetGroupByNameAsync(request.Name);
                if (existGroup != null)
                {
                    throw new AlreadyExistsException("El nombre del grupo ya esta en uso.");
                }
            }

            group.Name = request.Name;
            group.Description = request.Description;

            await _uow.GroupRepository.UpdateAsync(group, groupId);
            await _uow.CommitAsync();

            return group;
        }

        public async Task<Group> UpdateGroupActiveStatusAsync(int groupId, bool isActive)
        {
            var group = await GetGroupAsync(groupId);
            group.IsActive = isActive;
            group.ModifiedAt = DateTime.UtcNow;

            await _uow.GroupRepository.UpdateAsync(group, groupId);
            await _uow.CommitAsync();

            return group;
        }

        public async Task DeleteGroupAsync(int groupId)
        {
            var group = await GetGroupAsync(groupId);

            _uow.GroupRepository.Delete(group);
            await _uow.CommitAsync();
        }

        public async Task<(ICollection<GroupInviteActionResponse> result, ICollection<GroupInvitation> invitations)> InviteUsersToGroupAsync(int groupId, ICollection<EmailRequest> request)
        {
            var group = await GetGroupAsync(groupId);

            var result = new List<GroupInviteActionResponse>();
            var invitations = new List<GroupInvitation>();

            var emails = request.Select(r => r.Email);

            var users = await _uow.UserRepository.GetAll()
                    .Include(u => u.Group)
                    .Where(u => emails.Contains(u.Email))
                    .ToListAsync();

            foreach (var email in emails)
            {
                var user = users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    var groupInvitation = await CreateGroupInvitationAsync(group, null, email);
                    invitations.Add(groupInvitation);

                    var response = new GroupInviteActionResponse
                    {
                        Email = email,
                        SuccessInvited = true
                    };

                    result.Add(response);
                }
                else
                {
                    if (user.Group == null)
                    {
                        var groupInvitation = await CreateGroupInvitationAsync(group, user, user.Email);
                        invitations.Add(groupInvitation);

                        var response = new GroupInviteActionResponse
                        {
                            Email = email,
                            SuccessInvited = true
                        };

                        result.Add(response);
                    }
                    else
                    {
                        var response = new GroupInviteActionResponse
                        {
                            Email = email,
                            SuccessInvited = false,
                            CustomError = "Ya pertenece a un grupo"
                        };

                        result.Add(response);
                    }
                }
            }

            return (result, invitations);
        }

        public async Task DeleteGroupInvitationAsync(int invitationId)
        {
            var invitation = await GetGroupInvitationAsync(invitationId);

            if (invitation.Status == StatusInvitationEnum.PENDING)
            {
                _uow.GroupInvitationRepository.Delete(invitation);
                await _uow.CommitAsync();
            }
            else
            {
                throw new UnprocessableEntityException("La invitacion ya fue procesada por el usuario.");
            }
        }

        public async Task<GroupInvitation> UpdateGroupInvitationAsync(StatusInvitationEnum status, string token)
        {
            var invitation = await GetGroupInvitationByTokenAsync(token);

            if (invitation != null)
            {
                invitation.Status = status;
                invitation.SecurityToken = "";
                invitation.ModifiedAt = DateTime.UtcNow;

                await _uow.GroupInvitationRepository.UpdateAsync(invitation, invitation.Id);
                await _uow.CommitAsync();
            }
            else
            {
                throw new ForbiddenException("El token proporcionado ya no es valido.");
            }

            return invitation;
        }

        public async Task<PaginatedList<GroupInvitation>> GetInvitationsAsync(int groupId, int pag, int perPag, string sortOrder, string search, ICollection<StatusInvitationEnum> statuses)
        {
            var result = _uow.GroupInvitationRepository.GetAll()
                .Where(gi => gi.GroupId == groupId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(i => i.UserEmail.ToLower().Contains(search.ToLower()));
            }

            if (statuses.Count() > 0)
            {
                result = result.Where(gi => statuses.Contains(gi.Status));
            }


            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "createdAt_desc":
                        result = result.OrderByDescending(i => i.CreatedAt);
                        break;

                    case "createdAt_asc":
                        result = result.OrderBy(i => i.CreatedAt);
                        break;

                    case "status_desc":
                        result = result.OrderByDescending(i => i.Status);
                        break;

                    case "status_asc":
                        result = result.OrderBy(i => i.Status);
                        break;

                    case "userEmail_desc":
                        result = result.OrderByDescending(i => i.UserEmail);
                        break;

                    case "userEmail_asc":
                        result = result.OrderBy(i => i.UserEmail);
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<GroupInvitation>.CreateAsync(result, pag, perPag);
        }

        public async Task<PaginatedList<User>> GetUsersAsync(int groupId, int pag, int perPag, string sortOrder, string search)
        {
            var result = _uow.UserRepository.GetAll()
                .Include(u => u.Group)
                .Where(u => u.GroupId.HasValue && u.GroupId.Value == groupId)
                .Where(u => u.Status != StatusEnum.INACTIVE)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(
                        i => i.FullName.ToLower().Contains(search.ToLower()) ||
                             i.Email.ToLower().Contains(search.ToLower()) ||
                             i.Phone.ToLower().Contains(search.ToLower()));
            }

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "fullName_desc":
                        result = result.OrderByDescending(i => i.FullName);
                        break;

                    case "fullName_asc":
                        result = result.OrderBy(i => i.FullName);
                        break;

                    case "email_desc":
                        result = result.OrderByDescending(i => i.Email);
                        break;

                    case "email_asc":
                        result = result.OrderBy(i => i.Email);
                        break;

                    case "phone_desc":
                        result = result.OrderByDescending(i => i.Phone);
                        break;

                    case "phone_asc":
                        result = result.OrderBy(i => i.Phone);
                        break;

                    case "status_desc":
                        result = result.OrderByDescending(i => i.Status);
                        break;

                    case "status_asc":
                        result = result.OrderBy(i => i.Status);
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<User>.CreateAsync(result, pag, perPag);
        }

        private async Task<GroupInvitation> GetGroupInvitationAsync(int id)
        {
            var invitation = await _uow.GroupInvitationRepository.GetAll()
               .Include(g => g.Group)
               .Include(g => g.User)
               .FirstOrDefaultAsync(g => g.Id == id);

            if (invitation == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Invitation");

            return invitation;
        }

        private async Task<GroupInvitation> GetGroupInvitationByTokenAsync(string token)
        {
            var invitation = await _uow.GroupInvitationRepository.GetAll()
               .Include(g => g.Group)
               .Include(g => g.User)
               .FirstOrDefaultAsync(g => g.SecurityToken == token);

            if (invitation == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Invitation");

            return invitation;
        }

        private async Task<string> SetGroupAdminAsyn(Group group, string adminEmail, string lang)
        {
            var generatedPassword = "";
            var user = await _uow.UserRepository.GetAll()
                .Include(g => g.Group)
                .FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (user == null)
            {
                // create user
                var generator = new PasswordGenerator(6, 6, 0, 0, 0, 0);
                generatedPassword = generator.Generate();

                var newUser = new SignUpRequest
                {
                    Email = adminEmail,
                    FullName = "",
                    Language = lang,
                    Password = generatedPassword,
                    ConfirmationPassword = generatedPassword
                };
                user = await _accountService.SignUpAsync(newUser);

                user.Role = RoleEnum.GROUP_ADMIN;
                await _uow.UserRepository.UpdateAsync(user, user.Id);
                await _uow.CommitAsync();
            }
            else
            {
                if (user.Group != null)
                    throw new InvalidDataException("El usuario ya pertenece a un grupo, solo un grupo por usuario es permitido.", "AdminEmail");
            }

            await CreateGroupInvitationAsync(group, user, user.Email);

            return generatedPassword;
        }

        private async Task<GroupInvitation> CreateGroupInvitationAsync(Group group, User user = null, string email = "")
        {
            var securityToken = Guid.NewGuid().ToString();
            var groupInvitation = new GroupInvitation
            {
                User = user,
                Group = group,
                UserEmail = email,
                Status = StatusInvitationEnum.PENDING,
                SecurityToken = securityToken,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            await _uow.GroupInvitationRepository.AddAsync(groupInvitation);
            await _uow.CommitAsync();

            return groupInvitation;
        }
    }
}
