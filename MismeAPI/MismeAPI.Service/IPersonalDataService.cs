using MismeAPI.Common.DTO.Request;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IPersonalDataService
    {
        Task<IEnumerable<PersonalData>> GetPersonalDataVariablesAsync();

        Task<PersonalData> GetPersonalDataByIdAsync(int id);

        Task<UserPersonalData> GetUserPersonalDataByIdAsync(int id, int userId);

        Task<IEnumerable<UserPersonalData>> GetHistoricalUserPersonalDataByIdAsync(int id, int userId);

        Task<IEnumerable<UserPersonalData>> GetUserCurrentPersonalDatasAsync(int userId);

        Task<PersonalData> CreatePersonalDataAsync(int loggedUser, CreatePersonalDataRequest personalData);

        Task<PersonalData> UpdatePersonalDataAsync(int loggedUser, UpdatePersonalDataRequest personalData);

        Task DeletePersonalDataAsync(int loggedUser, int id);

        Task<UserPersonalData> SetPersonalDataValueAsync(int loggedUser, int id, string value);
    }
}