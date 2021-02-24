using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using DeviceDetectorNET.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Service;
using MismeAPI.Service.Utils;
using MismeAPI.Services.Utils;
using rlcx.suid;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wangkanai.Detection;

namespace MismeAPI.Services.Impls
{
    public class PersonalDataService : IPersonalDataService
    {
        private readonly IUnitOfWork _uow;

        public PersonalDataService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<PaginatedList<PersonalData>> GetUserPersonalDataAsync(int userId, int pag, int perPag, string sortOrder, ICollection<PersonalDataEnum> keys)
        {
            var result = _uow.PersonalDataRepository.GetAll()
                .Where(u => u.UserId == userId)
                .AsQueryable();

            // define status filter
            if (keys.Count() > 0)
            {
                result = result.Where(i => keys.Contains(i.Key));
            }

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "value_desc":
                        result = result.OrderByDescending(i => i.Value);
                        break;

                    case "value_asc":
                        result = result.OrderBy(i => i.Value);
                        break;

                    case "createdAt_desc":
                        result = result.OrderByDescending(i => i.CreatedAt);
                        break;

                    case "createdAt_asc":
                        result = result.OrderBy(i => i.CreatedAt);
                        break;

                    default:
                        result = result.OrderByDescending(i => i.CreatedAt);
                        break;
                }
            }

            return await PaginatedList<PersonalData>.CreateAsync(result, pag == -1 ? 1 : pag, pag == -1 ? result.Count() : perPag);
        }

        public async Task AddPersonalDataAsync(int userId, PersonalDataEnum key, string value)
        {
            var data = await _uow.PersonalDataRepository.GetAll()
                .Where(pd => pd.UserId == userId && pd.Key == key)
                .OrderByDescending(pd => pd.CreatedAt)
                .FirstOrDefaultAsync();

            if (data == null || data.Value != value)
            {
                var newData = new PersonalData
                {
                    UserId = userId,
                    Key = key,
                    Value = value,
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.PersonalDataRepository.AddAsync(newData);
            }
        }
    }
}
