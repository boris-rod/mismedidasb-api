using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class UserReferralService : IUserReferralService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public UserReferralService(IUnitOfWork uow, IConfiguration config)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<IEnumerable<UserReferral>> CreateReferralsAsync(int loggedUser, IEnumerable<CreateUserReferralRequest> request)
        {
            var result = new List<UserReferral>();
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");

            foreach (var item in request)
            {
                var referral = new UserReferral
                {
                    Email = item.Email,
                    UserId = loggedUser,
                    User = user,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await _uow.UserReferralRepository.AddAsync(referral);
                result.Add(referral);
            }

            await _uow.CommitAsync();

            return result;
        }

        public async Task RemoveReferralsAsync(int loggedUser, IEnumerable<UserReferral> referrals)
        {
            foreach (var referral in referrals)
            {
                if (referral.UserId == loggedUser)
                    _uow.UserReferralRepository.Delete(referral);
            }

            await _uow.CommitAsync();
        }

        /// <summary>
        /// Assign invited user to the referral table if exist
        /// </summary>
        /// <param name="user">User invited in referral table</param>
        /// <returns>Returns the referral object if found, return null otherwise.</returns>
        public async Task<UserReferral> SetReferralUserAsync(User user)
        {
            var referral = await _uow.UserReferralRepository.GetAll()
                .Include(u => u.User)
                .Where(u => u.Email == user.Email && !u.InvitedId.HasValue)
                .FirstOrDefaultAsync();

            if (referral != null)
            {
                referral.InvitedId = user.Id;
                referral.ModifiedAt = DateTime.UtcNow;

                await _uow.UserReferralRepository.UpdateAsync(referral, referral.Id);
                await _uow.CommitAsync();
            }

            return referral;
        }
    }
}
