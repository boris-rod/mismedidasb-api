using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
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

        public async Task<IEnumerable<UserPersonalData>> GetUserCurrentPersonalDatasAsync(int userId)
        {
            var result = await _uow.UserPersonalDataRepository.GetAll()
              .Where(pd => pd.UserId == userId)
              .Include(pd => pd.PersonalData)
              .Include(pd => pd.User)
              .GroupBy(e => e.PersonalDataId)
              .Select(g => g.OrderByDescending(d => d.MeasuredAt)
                      .FirstOrDefault())
              .ToListAsync();
            return result;
        }

        public async Task<PersonalData> CreatePersonalDataAsync(int loggedUser, CreatePersonalDataRequest personalData)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            // validate codename
            var existCodename = await _uow.PersonalDataRepository.FindByAsync(p => p.CodeName.ToLower() == personalData.CodeName.ToLower());

            if (existCodename.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Codename");
            }
            var pd = new PersonalData();
            pd.CodeName = personalData.CodeName;
            pd.CreatedAt = DateTime.UtcNow;
            pd.ModifiedAt = DateTime.UtcNow; ;
            pd.MeasureUnit = personalData.MeasureUnit;
            pd.Name = personalData.Name;
            pd.Order = personalData.Order;
            pd.Type = (TypeEnum)personalData.Type;

            await _uow.PersonalDataRepository.AddAsync(pd);
            await _uow.CommitAsync();

            return pd;
        }

        public async Task<PersonalData> UpdatePersonalDataAsync(int loggedUser, UpdatePersonalDataRequest personalData)
        {
            // not found personal data?
            var pd = await _uow.PersonalDataRepository.GetAll().Where(p => p.Id == personalData.Id).FirstOrDefaultAsync();
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Personal data");
            }

            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            // validate codename, except itself
            var existCodename = await _uow.PersonalDataRepository.FindByAsync(p => p.CodeName.ToLower() == personalData.CodeName.ToLower() && p.Id != personalData.Id);
            if (existCodename.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Codename");
            }

            pd.CodeName = personalData.CodeName;
            pd.MeasureUnit = personalData.MeasureUnit;
            pd.ModifiedAt = DateTime.UtcNow;
            pd.Name = personalData.Name;
            pd.Order = personalData.Order;
            pd.Type = (TypeEnum)personalData.Type;

            _uow.PersonalDataRepository.Update(pd);
            await _uow.CommitAsync();
            return pd;
        }

        public async Task DeletePersonalDataAsync(int loggedUser, int id)
        {
            // not found personal data?
            var pd = await _uow.PersonalDataRepository.GetAsync(id);
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Personal data");
            }

            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            _uow.PersonalDataRepository.Delete(pd);
            await _uow.CommitAsync();
        }

        public async Task<UserPersonalData> SetPersonalDataValueAsync(int loggedUser, int id, string value)
        {
            // not found personal data?
            var pd = await _uow.PersonalDataRepository.GetAsync(id);
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Personal data");
            }
            var userPd = new UserPersonalData();
            userPd.MeasuredAt = DateTime.UtcNow;
            userPd.PersonalDataId = pd.Id;
            userPd.UserId = loggedUser;
            userPd.Value = value;
            await _uow.UserPersonalDataRepository.AddAsync(userPd);
            await _uow.CommitAsync();
            return userPd;
        }
    }
}