using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Poll;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IPollService
    {
        Task<IEnumerable<Poll>> GetAllPollsAsync(int conceptId);

        Task<Poll> GetPollByIdAsync(int id);

        Task<Poll> CreatePollAsync(int loggedUser, CreatePollRequest poll);

        Task<Poll> UpdatePollDataAsync(int loggedUser, UpdatePollRequest poll);

        Task DeletePollAsync(int loggedUser, int id);

        Task<IEnumerable<Poll>> GetAllPollsByConceptAsync(int conceptId);

        Task SetPollResultAsync(int loggedUser, SetPollResultRequest pollResult);

        Task<Poll> UpdatePollTitleAsync(int loggedUser, string title, int id);

        Task SetPollResultByQuestionsAsync(int loggedUser, ListOfPollResultsRequest result);

        Task ChangePollQuestionOrderAsync(int loggedUser, QuestionOrderRequest questionOrderRequest, int id);
    }
}