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
        Task<IList<UserReferral>> CreateReferralsAsync(int loggedUser, IList<CreateUserReferralRequest> request);

        Task RemoveReferralsAsync(int loggedUser, IList<UserReferral> referrals);

        Task<UserReferral> SetReferralUserAsync(User user);
    }
}
