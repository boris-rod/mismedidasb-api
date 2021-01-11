using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
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

        public async Task<PaginatedList<Group>> GetGroupsAsync(int pag, int perPag, string sortOrder)
        {
            var result = _uow.GroupRepository.GetAll()
                .AsQueryable();

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

                    default:
                        break;
                }
            }

            return await PaginatedList<Group>.CreateAsync(result, pag, perPag);
        }

        public async Task<Group> GetGroupAsync(int groupId)
        {
            var group = await _uow.GroupRepository.GetAsync(groupId);
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
            var group = await _uow.GroupRepository.GetAll().FirstOrDefaultAsync(g => g.Name == groupName);

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

            var generatedPasword = await SetGroupAdminAsyn(group, request.AdminEmail, request.Language);

            return (group, generatedPasword);
        }

        public async Task<bool> ValidateGroupAdminFirstLoginAsync(User user)
        {
            var invitation = await _uow.GroupInvitationRepository.GetAll()
                .FirstOrDefaultAsync(gi => gi.UserId == user.Id && gi.Status == StatusInvitatonEnum.PENDING);

            if (invitation != null)
            {
                var group = await _uow.GroupRepository.GetAll().FirstOrDefaultAsync(g => g.Id == invitation.GroupId);
                invitation.Status = StatusInvitatonEnum.ACCEPTED;
                invitation.ModifiedAt = DateTime.UtcNow;
                await _uow.GroupInvitationRepository.UpdateAsync(invitation, invitation.Id);

                user.ActivatedAt = DateTime.UtcNow;
                user.Status = StatusEnum.ACTIVE;
                user.Group = group;
                await _uow.UserRepository.UpdateAsync(user, user.Id);
                await _uow.CommitAsync();

                return true;
            }

            return false;
        }

        public async Task<(Group Group, string GeneratedPassword)> UpdateGroupAsync(int groupId, UpdateGroupRequest request)
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
            group.IsActive = true;
            group.ModifiedAt = DateTime.UtcNow;

            return (group, generatedPasword);
        }

        public Task DeleteGroupAsync(int groupId)
        {
            throw new NotImplementedException();
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
            }
            else
            {
                if (user.Group != null)
                    throw new InvalidDataException("El usuario ya pertenece a un grupo, solo un grupo por usuario es permitido.", "AdminEmail");
            }

            var groupInvitation = new GroupInvitation
            {
                User = user,
                Group = group,
                Status = StatusInvitatonEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            await _uow.GroupInvitationRepository.AddAsync(groupInvitation);
            await _uow.CommitAsync();

            return generatedPassword;
        }
    }
}
