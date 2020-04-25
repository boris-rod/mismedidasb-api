using MismeAPI.Common.DTO.Request;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IGeneralContentService
    {
        Task<IEnumerable<GeneralContent>> GetGeneralContentsAdminAsync(int loggedUser);

        Task ChangeContentTranslationAsync(int loggedUser, GeneralContentTranslationRequest contentTranslationRequest, int id);

        Task<GeneralContent> GetGeneralContentsByTypeAsync(int contentType);

        Task AcceptTermsAndConditionsAsync(int loggedUser);
    }
}