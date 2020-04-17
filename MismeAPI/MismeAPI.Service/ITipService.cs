using MismeAPI.Common.DTO.Request.Tip;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface ITipService
    {
        Task<IEnumerable<Tip>> GetTipsAdminAsync(int loggedUser);

        Task ChangeTipTranslationAsync(int loggedUser, TipTranslationRequest tipTranslationRequest, int id);
    }
}