using MismeAPI.Common.DTO.Request.Answer;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IAnswerService
    {
        Task<IEnumerable<Answer>> GetAnswersByQuestionIdAsync(int questionId);

        Task AnswerAQuestionAsync(int loggedUser, int id);

        Task<IEnumerable<Answer>> GetAnswersAdminAsync(int loggedUser);

        Task ChangeAnswerTranslationAsync(int loggedUser, AnswerTranslationRequest answerTranslationRequest, int id);
    }
}