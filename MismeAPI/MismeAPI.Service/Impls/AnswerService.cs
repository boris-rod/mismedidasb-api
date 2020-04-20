using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request.Answer;
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
    public class AnswerService : IAnswerService
    {
        private readonly IUnitOfWork _uow;

        public AnswerService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task AnswerAQuestionAsync(int loggedUser, int id)
        {
            var ans = await _uow.AnswerRepository.GetAll().Where(p => p.Id == id).FirstOrDefaultAsync();
            if (ans == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Answer");
            }
            var userAnswer = new UserAnswer();
            userAnswer.AnswerId = id;
            userAnswer.CreatedAt = DateTime.UtcNow;
            userAnswer.UserId = loggedUser;
            await _uow.UserAnswerRepository.AddAsync(userAnswer);
            await _uow.CommitAsync();
        }

        public async Task ChangeAnswerTranslationAsync(int loggedUser, AnswerTranslationRequest answerTranslationRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var answer = await _uow.AnswerRepository.GetAll().Where(a => a.Id == id)
                .Include(a => a.Question)
                    .ThenInclude(q => q.Poll)
                        .ThenInclude(p => p.Concept)
                .FirstOrDefaultAsync();
            // health measures is a special case
            if (answer != null && answer.Question.Poll.Concept.Codename == CodeNamesConstants.HEALTH_MEASURES && answer.Question.Poll.Order == 1)
            {
                switch (answerTranslationRequest.Lang)
                {
                    case "en":
                        answer.TitleEN = answerTranslationRequest.Title;
                        break;

                    case "it":
                        answer.TitleIT = answerTranslationRequest.Title;
                        break;

                    default:
                        answer.Title = answerTranslationRequest.Title;
                        break;
                }

                _uow.AnswerRepository.Update(answer);
            }
            else if (answer != null)
            {
                var poll = await _uow.PollRepository.GetAll().Where(p => p.Id == answer.Question.PollId)
                    .Include(p => p.Questions)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync();
                if (poll != null)
                {
                    foreach (var item in poll.Questions)
                    {
                        var a = item.Answers.Where(a => a.Order == answer.Order).FirstOrDefault();
                        if (a != null)
                        {
                            switch (answerTranslationRequest.Lang)
                            {
                                case "en":
                                    a.TitleEN = answerTranslationRequest.Title;
                                    break;

                                case "it":
                                    a.TitleIT = answerTranslationRequest.Title;
                                    break;

                                default:
                                    a.Title = answerTranslationRequest.Title;
                                    break;
                            }

                            _uow.AnswerRepository.Update(a);
                        }
                    }
                }
            }
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<Answer>> GetAnswersAdminAsync(int loggedUser)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var answers = new List<Answer>();

            var polls = await _uow.PollRepository.GetAll().Where(a => a.Concept.Codename == CodeNamesConstants.HEALTH_MEASURES)
                .Include(p => p.Concept)
                .Include(p => p.Questions)
                    .ThenInclude(q => q.Answers)
                .OrderBy(p => p.Order)
                .ToListAsync();

            var personalData = polls.ElementAt(0);
            var sexQuestion = personalData.Questions.OrderBy(p => p.Order).LastOrDefault();
            if (sexQuestion != null)
            {
                answers.AddRange(sexQuestion.Answers);
            }
            var phiExcersise = polls.ElementAt(1);
            answers.AddRange(phiExcersise.Questions.ElementAt(0).Answers);

            var diet = polls.ElementAt(2);
            answers.AddRange(diet.Questions.ElementAt(0).Answers);

            var poll = await _uow.PollRepository.GetAll().Where(a => a.Concept.Codename == CodeNamesConstants.VALUE_MEASURES)
               .Include(p => p.Concept)
               .Include(p => p.Questions)
                   .ThenInclude(q => q.Answers)
               .OrderBy(p => p.Order)
               .FirstOrDefaultAsync();

            if (poll != null)
            {
                answers.AddRange(poll.Questions.ElementAt(0).Answers);
            }

            poll = await _uow.PollRepository.GetAll().Where(a => a.Concept.Codename == CodeNamesConstants.WELLNESS_MEASURES)
               .Include(p => p.Concept)
               .Include(p => p.Questions)
                   .ThenInclude(q => q.Answers)
               .OrderBy(p => p.Order)
               .FirstOrDefaultAsync();

            if (poll != null)
            {
                answers.AddRange(poll.Questions.ElementAt(0).Answers);
            }

            return answers;
        }

        public async Task<IEnumerable<Answer>> GetAnswersByQuestionIdAsync(int questionId)
        {
            var que = await _uow.QuestionRepository.GetAll().Where(p => p.Id == questionId).FirstOrDefaultAsync();
            if (que == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Question");
            }
            var answers = await _uow.AnswerRepository.GetAll().Where(q => q.QuestionId == questionId).ToListAsync();
            return answers;
        }
    }
}