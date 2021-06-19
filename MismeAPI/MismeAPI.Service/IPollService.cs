using Hangfire;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Poll;
using MismeAPI.Common.DTO.Request.Tip;
using MismeAPI.Data.Entities;
using System;
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

        //Task SetPollResultAsync(int loggedUser, SetPollResultRequest pollResult);

        Task<Poll> UpdatePollTitleAsync(int loggedUser, string title, int id);

        Task<List<string>> SetPollResultByQuestionsAsync(int loggedUser, ListOfPollResultsRequest result, string language);

        Task ChangePollQuestionOrderAsync(int loggedUser, QuestionOrderRequest questionOrderRequest, int id);

        Task ChangePollReadOnlyAsync(int loggedUser, PollReadOnlyRequest pollReadOnlyRequest, int id);

        Task<Tip> AddTipRequestAsync(int loggedUser, AddTipRequest tipRequest);

        Task DeleteTipAsync(int loggedUser, int id);

        Task<Tip> UpdateTipContentAsync(int loggedUser, string content, int id);

        Task ActivateTipAsync(int loggedUser, int id, int pollId, int position);

        Task<List<Poll>> GetAllPollsAdminAsync(int loggedUser);

        Task ChangePollTranslationAsync(int loggedUser, PollTranslationRequest pollTranslationRequest, int id);

        Task<Dictionary<int, int>> GetLastAnsweredDictAsync(int loggedUser);

        Task<bool> HasAnsweredConceptBeforeAsync(int loggedUser, ListOfPollResultsRequest result);

        IEnumerable<int> GetAnsweredPolls(ListOfPollResultsRequest result);

        Task<IEnumerable<Question>> GetLatestPollAnswerByUser(string conceptCode, int userId);

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task TestMethod();

        Task<(int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat)> GetUserPollsInfoAsync(int userId);
    }
}
