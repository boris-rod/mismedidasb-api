using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Poll;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class PollService : IPollService
    {
        private readonly IUnitOfWork _uow;
        private readonly IQuestionService _questionService;

        public PollService(IUnitOfWork uow, IQuestionService questionService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _questionService = questionService ?? throw new ArgumentNullException(nameof(questionService));
        }

        public async Task ChangePollQuestionOrderAsync(int loggedUser, QuestionOrderRequest questionOrderRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var poll = await _uow.PollRepository.GetAll().Where(c => c.Id == id)
                .Include(c => c.Questions).FirstOrDefaultAsync();
            if (poll == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }
            var questionOne = poll.Questions.Where(p => p.Id == questionOrderRequest.QuestionOneId).FirstOrDefault();
            if (questionOne != null)
            {
                questionOne.Order = questionOrderRequest.QuestionOneOrder;
                await _uow.QuestionRepository.UpdateAsync(questionOne, questionOne.Id);
            }

            var questionTwo = poll.Questions.Where(p => p.Id == questionOrderRequest.QuestionTwoId).FirstOrDefault();
            if (questionTwo != null)
            {
                questionTwo.Order = questionOrderRequest.QuestionTwoOrder;
                await _uow.QuestionRepository.UpdateAsync(questionTwo, questionTwo.Id);
            }
            await _uow.CommitAsync();
        }

        public async Task ChangePollReadOnlyAsync(int loggedUser, PollReadOnlyRequest pollReadOnlyRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var poll = await _uow.PollRepository.GetAll().Where(c => c.Id == id)
                .Include(c => c.Questions).FirstOrDefaultAsync();
            if (poll == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }

            if (pollReadOnlyRequest.ReadOnly == true)
            {
                poll.HtmlContent = pollReadOnlyRequest.HtmlContent;
                poll.IsReadOnly = true;
                foreach (var item in poll.Questions)
                {
                    _uow.QuestionRepository.Delete(item);
                }
                await _uow.PollRepository.UpdateAsync(poll, id);
            }
            //else
            //{
            //    poll.IsReadOnly = false;
            //    poll.HtmlContent = "";

            // var internalRequest = pollReadOnlyRequest.QuestionWithAnswers; internalRequest.PollId
            // = id;

            //    await _questionService.AddOrUpdateQuestionWithAnswersAsync(loggedUser, internalRequest);
            //}
            await _uow.CommitAsync();
        }

        public async Task<Poll> CreatePollAsync(int loggedUser, CreatePollRequest poll)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            // validate codename
            var existPollName = await _uow.PollRepository.FindByAsync(p => p.Name.ToLower() == poll.Name.ToLower());

            if (existPollName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Poll name");
            }
            var orderMax = await _uow.PollRepository.GetAll().MaxAsync(p => p.Order);

            var p = new Poll();
            p.CreatedAt = DateTime.UtcNow;
            p.ModifiedAt = DateTime.UtcNow; ;
            p.Name = poll.Name;
            p.Description = poll.Description;
            p.ConceptId = poll.ConceptId;
            p.Order = orderMax + 1;

            await _uow.PollRepository.AddAsync(p);
            await _uow.CommitAsync();

            return p;
        }

        public async Task DeletePollAsync(int loggedUser, int id)
        {
            // not found poll?
            var pd = await _uow.PollRepository.GetAsync(id);
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }

            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            _uow.PollRepository.Delete(pd);

            // needed to reorder
            var polls = await _uow.PollRepository.GetAll().Where(p => p.ConceptId == pd.ConceptId && p.Order > pd.Order)
                .OrderBy(p => p.Order)
                .ToListAsync();
            foreach (var poll in polls)
            {
                poll.Order = poll.Order - 1;
                _uow.PollRepository.Update(poll);
            }

            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<Poll>> GetAllPollsAsync(int conceptId)
        {
            var result = _uow.PollRepository.GetAll()
                .Include(p => p.Concept)
                .Include(p => p.Questions)
                    .ThenInclude(q => q.Answers)
                .AsQueryable();
            if (conceptId != -1)
            {
                result = result.Where(p => p.ConceptId == conceptId);
            }
            return await result.ToListAsync();
        }

        public async Task<IEnumerable<Poll>> GetAllPollsByConceptAsync(int conceptId)
        {
            var result = await _uow.PollRepository.GetAll().Where(p => p.ConceptId == conceptId)
                .Include(p => p.Concept)
                .Include(p => p.Questions)
                    .ThenInclude(q => q.Answers)
                .ToListAsync();
            return result;
        }

        public async Task<Poll> GetPollByIdAsync(int id)
        {
            var poll = await _uow.PollRepository.GetAll()
                .Where(p => p.Id == id)
                .Include(p => p.Concept)
                .Include(p => p.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync();
            if (poll == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }
            return poll;
        }

        public async Task SetPollResultAsync(int loggedUser, SetPollResultRequest pollResult)
        {
            // not found poll?
            var pd = await _uow.PollRepository.GetAsync(pollResult.PollId);
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }
            var userPoll = new UserPoll();
            userPoll.PollId = pd.Id;
            userPoll.Result = pollResult.Result;
            userPoll.UserId = loggedUser;
            userPoll.CompletedAt = DateTime.UtcNow;
            await _uow.UserPollRepository.AddAsync(userPoll);
            await _uow.CommitAsync();
        }

        public async Task SetPollResultByQuestionsAsync(int loggedUser, ListOfPollResultsRequest result)
        {
            foreach (var elem in result.PollDatas)
            {
                // not found poll?
                var pda = await _uow.UserPollRepository.GetAll().Where(p => p.PollId == elem.PollId && p.CompletedAt.Date == DateTime.UtcNow.Date)
                    .Include(p => p.Poll)
                        .ThenInclude(p => p.Questions)
                            .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync();
                //today is not answered yet
                if (pda == null)
                {
                    foreach (var item in elem.QuestionsResults)
                    {
                        var re = new UserAnswer();

                        re.CreatedAt = DateTime.UtcNow;
                        re.UserId = loggedUser;
                        re.AnswerId = item.AnswerId;
                        await _uow.UserAnswerRepository.AddAsync(re);
                    }

                    //// CALCULATE THE RESULT OF THE POLL
                }
                // we need to update the values
                else
                {
                    foreach (var item in elem.QuestionsResults)
                    {
                        var re = await _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == loggedUser && u.Answer.QuestionId == item.QuestionId && u.CreatedAt.Date == DateTime.UtcNow.Date)
                            .Include(u => u.Answer)
                            .FirstOrDefaultAsync();
                        if (re != null)
                        {
                            re.AnswerId = item.AnswerId;
                            _uow.UserAnswerRepository.Update(re);
                        }
                    }

                    //// CALCULATE THE RESULT OF THE POLL
                }
            }
        }

        public async Task<Poll> UpdatePollDataAsync(int loggedUser, UpdatePollRequest poll)
        {
            // not found poll?
            var pd = await _uow.PollRepository.GetAll().Where(p => p.Id == poll.Id)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync();
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }

            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            // validate codename, except itself
            var existName = await _uow.PollRepository.FindByAsync(p => p.Name.ToLower() == poll.Name.ToLower() && p.Id != poll.Id);
            if (existName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Name");
            }

            pd.ModifiedAt = DateTime.UtcNow;
            pd.Name = poll.Name;
            pd.Description = poll.Description;
            pd.ConceptId = poll.ConceptId;

            _uow.PollRepository.Update(pd);
            await _uow.CommitAsync();
            return pd;
        }

        public async Task<Poll> UpdatePollTitleAsync(int loggedUser, string title, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            // not found poll?
            var pd = await _uow.PollRepository.GetAll().Where(p => p.Id == id)
                .Include(p => p.Concept)
                .Include(p => p.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync();
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }
            // validate poll title
            var existPollName = await _uow.PollRepository.FindByAsync(p => p.Name.ToLower() == title.ToLower() && p.Id != id);

            if (existPollName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Poll name");
            }
            pd.Name = title;
            await _uow.PollRepository.UpdateAsync(pd, id);
            await _uow.CommitAsync();

            return pd;
        }
    }
}