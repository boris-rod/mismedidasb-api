using MismeAPI.Common.DTO.Request;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IQuestionService
    {
        Task<Question> CreateQuestionAsync(int loggedUser, CreateQuestionRequest question);

        Task<IEnumerable<Question>> GetQuestionsByPollIdAsync(int pollId);

        Task<Question> GetQuestionByIdAsync(int id);

        Task<Question> UpdateQuestionAsync(int loggedUser, UpdateQuestionRequest question);

        Task DeleteQuestionAsync(int loggedUser, int id);
    }
}