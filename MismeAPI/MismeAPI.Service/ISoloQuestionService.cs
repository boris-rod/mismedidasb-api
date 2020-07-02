using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.SoloQuestion;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.NonDatabase;
using MismeAPI.Services.Utils;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface ISoloQuestionService
    {
        Task<PaginatedList<SoloQuestion>> GetQuestionsAsync(int pag, int perPag, string sortOrder, string search);

        Task<PaginatedList<SoloQuestion>> GetUserQuestionsForTodayAsync(int userId, int pag, int perPag);

        Task<SoloQuestion> GetAsync(int id);

        Task<SoloQuestion> CreateAsync(CreateSoloQuestionRequest request);

        Task<SoloQuestion> UpdateAsync(int id, UpdateSoloQuestionRequest request);

        Task DeleteAsync(int id);

        Task SeedSoloQuestionsAsync();

        Task<UserSoloAnswer> SetUserAnswerAsync(int loggedUser, CreateUserSoloAnswerRequest answerRequest);

        Task<ExtendedUserStatistics> GetuserSumaryAsync(int userId, int lastNDays);
    }
}
