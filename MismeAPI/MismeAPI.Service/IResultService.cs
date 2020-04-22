using MismeAPI.Common.DTO.Request.Result;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IResultService
    {
        Task ChangeResultTranslationAsync(int loggedUser, ResultTranslationRequest resultTranslationRequest, int id);

        Task<IEnumerable<Result>> GetResultsAdminAsync(int loggedUser);
    }
}