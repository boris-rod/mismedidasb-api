﻿using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
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

        public async Task<IList<UserReferral>> CreateReferralsAsync(int loggedUser, IList<CreateUserReferralRequest> request)
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

        public async Task RemoveReferralsAsync(int loggedUser, IList<UserReferral> referrals)
        {
            foreach (var referral in referrals)
            {
                if (referral.UserId == loggedUser)
                    _uow.UserReferralRepository.Delete(referral);
            }

            await _uow.CommitAsync();
        }
    }
}
