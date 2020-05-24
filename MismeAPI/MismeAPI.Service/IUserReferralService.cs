using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IUserReferralService
    {
        Task<IEnumerable<UserReferral>> CreateReferralsAsync(int loggedUser, IEnumerable<CreateUserReferralRequest> request);

        Task RemoveReferralsAsync(int loggedUser, IEnumerable<UserReferral> referrals);

        Task<UserReferral> SetReferralUserAsync(User user);
    }
}
