using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Services.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IPersonalDataService
    {
        Task<PaginatedList<PersonalData>> GetUserPersonalDataAsync(int userId, int pag, int perPag, string sortOrder, ICollection<PersonalDataEnum> keys);

        Task AddPersonalDataAsync(int userId, PersonalDataEnum key, string value);
    }
}
