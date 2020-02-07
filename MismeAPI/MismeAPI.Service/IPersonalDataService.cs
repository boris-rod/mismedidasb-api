using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IPersonalDataService
    {
        Task<IEnumerable<PersonalData>> GetPersonalDataVariablesAsync();

        Task<PersonalData> GetPersonalDataByIdAsync(int id);

        Task<PersonalData> GetUserPersonalDataByIdAsync(int id, int userId);

        Task<IEnumerable<PersonalData>> GetHistoricalUserPersonalDataByIdAsync(int id, int userId);
    }
}