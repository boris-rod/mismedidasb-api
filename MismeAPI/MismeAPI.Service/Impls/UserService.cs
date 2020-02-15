using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;

        public UserService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<PaginatedList<User>> GetUsersAsync(int loggedUser, int pag, int perPag, string sortOrder, int statusFilter, string search)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user.Role == RoleEnum.NORMAL)
            {
                throw new NotAllowedException("User");
            }

            var result = _uow.UserRepository.GetAll()
                .Where(u => u.Role == RoleEnum.NORMAL)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(
                        i => i.FullName.ToLower().Contains(search.ToLower()) ||
                             i.Email.ToLower().Contains(search.ToLower()) ||
                             i.Phone.ToLower().Contains(search.ToLower()));
            }

            // define status filter
            if (statusFilter > -1)
            {
                result = result.Where(i => (StatusEnum)statusFilter == i.Status);
            }

            // define sort order
            //if (!string.IsNullOrWhiteSpace(sortOrder))
            //{
            //    // sort order section
            //    switch (sortOrder)
            //    {
            //        case "status_desc":
            //            result = result.OrderByDescending(i => i.Status.ToString());
            //            break;

            // case "status_asc": result = result.OrderBy(i => i.Status.ToString()); break;

            // case "severity_desc": result = result.OrderByDescending(i => i.Severity.ToString()); break;

            // case "severity_asc": result = result.OrderBy(i => i.Severity.ToString()); break;

            // case "issueCode_desc": result = result.OrderByDescending(i => i.Code); break;

            // case "issueCode_asc": result = result.OrderBy(i => i.Code); break;

            // case "issueType_desc": result = result.OrderByDescending(i => i.IssueType.Type); break;

            // case "issueType_asc": result = result.OrderBy(i => i.IssueType.Type); break;

            // case "openedDate_desc": result = result.OrderByDescending(i => i.DateOpened); break;

            // case "openedDate_asc": result = result.OrderBy(i => i.DateOpened); break;

            // case "updatedDate_desc": result = result.OrderBy(i => i.DateUpdated); break;

            // case "updatedDate_asc": result = result.OrderBy(i => i.DateUpdated); break;

            //        default:
            //            break;
            //    }
            //}

            return await PaginatedList<User>.CreateAsync(result, pag, perPag);
        }
    }
}