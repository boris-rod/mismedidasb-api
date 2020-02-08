using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class PersonalDataService : IPersonalDataService
    {
        private readonly IUnitOfWork _uow;

        public PersonalDataService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<PersonalData> GetPersonalDataByIdAsync(int id)
        {
            var result = await _uow.PersonalDataRepository.GetAll().Where(p => p.Id == id).FirstOrDefaultAsync();
            if (result == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Personal Data");
            }
            return result;
        }

        public async Task<IEnumerable<PersonalData>> GetPersonalDataVariablesAsync()
        {
            var result = await _uow.PersonalDataRepository.GetAllAsync();
            return result.ToList();
        }

        public async Task<UserPersonalData> GetUserPersonalDataByIdAsync(int id, int userId)
        {
            var result = await _uow.UserPersonalDataRepository.GetAll()
                .Where(pd => pd.UserId == userId && pd.PersonalDataId == id)
                .Include(pd => pd.PersonalData)
                .Include(pd => pd.User)
                .OrderByDescending(pd => pd.MeasuredAt)
                .FirstOrDefaultAsync();
            if (result == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Personal Data");
            }
            return result;
        }

        public async Task<IEnumerable<UserPersonalData>> GetHistoricalUserPersonalDataByIdAsync(int id, int userId)
        {
            var result = await _uow.UserPersonalDataRepository.GetAll()
                .Where(pd => pd.UserId == userId && pd.PersonalDataId == id)
                .Include(pd => pd.PersonalData)
                .Include(pd => pd.User)
                .OrderBy(pd => pd.MeasuredAt)
                .ToListAsync();
            return result;
        }
    }
}