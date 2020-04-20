using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Question;
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
    public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _uow;

        public QuestionService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<IEnumerable<Question>> GetQuestionsByPollIdAsync(int id)
        {
            var pd = await _uow.PollRepository.GetAll().Where(p => p.Id == id).FirstOrDefaultAsync();
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }
            var questions = await _uow.QuestionRepository.GetAll().Where(q => q.PollId == pd.Id)
                .Include(q => q.Answers)
                .ToListAsync();
            return questions;
        }

        public async Task<Question> CreateQuestionAsync(int loggedUser, CreateQuestionRequest question)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var poll = await _uow.PollRepository.GetAsync(question.PollId);
            if (poll == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }
            var q = new Question();
            q.CreatedAt = DateTime.UtcNow;
            q.ModifiedAt = DateTime.UtcNow; ;
            q.Order = question.Order;
            q.Title = question.Title;
            q.PollId = poll.Id;

            await _uow.QuestionRepository.AddAsync(q);
            await _uow.CommitAsync();

            return q;
        }

        public async Task<Question> GetQuestionByIdAsync(int id)
        {
            var question = await _uow.QuestionRepository.GetAll().Where(q => q.Id == id)
                        .Include(q => q.Answers)
                        .FirstOrDefaultAsync();
            if (question == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Question");
            }
            return question;
        }

        public async Task<Question> UpdateQuestionAsync(int loggedUser, UpdateQuestionRequest question)
        {
            // not found poll?
            var pd = await _uow.QuestionRepository.GetAll().Where(p => p.Id == question.Id)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync();
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Question");
            }

            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            pd.ModifiedAt = DateTime.UtcNow;
            pd.Order = question.Order;
            pd.Title = question.Title;

            _uow.QuestionRepository.Update(pd);
            await _uow.CommitAsync();
            return pd;
        }

        public async Task DeleteQuestionAsync(int loggedUser, int id)
        {
            // not found question?
            var pd = await _uow.QuestionRepository.GetAsync(id);
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Question");
            }

            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var questionsToUpdateOrder = await _uow.QuestionRepository.GetAll().Where(q => q.Order > pd.Order).ToListAsync();
            foreach (var q in questionsToUpdateOrder)
            {
                q.Order = q.Order - 1;
                await _uow.QuestionRepository.UpdateAsync(q, q.Id);
            }
            _uow.QuestionRepository.Delete(pd);
            await _uow.CommitAsync();
        }

        public async Task<Question> UpdateQuestionTitleAsync(int loggedUser, int id, string title)
        {
            // not found question?
            var pd = await _uow.QuestionRepository.GetAsync(id);
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Question");
            }

            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var exist = await _uow.QuestionRepository.FindByAsync(q => q.Title.ToLower() == title && q.PollId == pd.PollId);
            if (exist.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA);
            }
            pd.Title = title;
            await _uow.QuestionRepository.UpdateAsync(pd, pd.Id);
            await _uow.CommitAsync();
            return pd;
        }

        public async Task<Question> AddOrUpdateQuestionWithAnswersAsync(int loggedUser, AddOrUpdateQuestionWithAnswersRequest question)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            if (question.QuestionId > -1)
            {
                // not found question?
                var pd = await _uow.QuestionRepository.GetAsync(question.QuestionId);
                if (pd == null)
                {
                    throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Question");
                }
                pd.ModifiedAt = DateTime.UtcNow;
                pd.Title = question.QuestionName;
                pd.Order = question.QuestionOrder;

                pd.Answers = await GetNewAnswersAsync(question);
                await _uow.QuestionRepository.UpdateAsync(pd, pd.Id);
                await _uow.CommitAsync();
                return pd;
            }
            else
            {
                var q = new Question();
                q.ModifiedAt = DateTime.UtcNow;
                q.CreatedAt = DateTime.UtcNow;
                q.Title = question.QuestionName;
                q.PollId = question.PollId;

                var orderMax = await _uow.QuestionRepository.GetAll().Where(q => q.PollId == question.PollId).OrderByDescending(p => p.Order).ToListAsync();
                q.Order = orderMax.Count > 0 ? orderMax.ElementAt(0).Order + 1 : 1;

                q.Answers = await GetNewAnswersAsync(question);

                var poll = await _uow.PollRepository.GetAsync(question.PollId);
                if (poll != null && poll.IsReadOnly == true)
                {
                    // remove tips because only read only poll have them
                    var tips = await _uow.TipRepository.GetAll().Where(t => t.PollId == poll.Id).ToListAsync();
                    foreach (var tip in tips)
                    {
                        _uow.TipRepository.Delete(tip);
                    }

                    poll.IsReadOnly = false;
                    poll.HtmlContent = "";
                    await _uow.PollRepository.UpdateAsync(poll, poll.Id);
                }

                await _uow.QuestionRepository.AddAsync(q);
                await _uow.CommitAsync();
                return q;
            }
        }

        private async Task<ICollection<Answer>> GetNewAnswersAsync(AddOrUpdateQuestionWithAnswersRequest question)
        {
            var answers = new List<Answer>();
            //remove previous answers
            if (question.QuestionId > -1)
            {
                var previousAnswers = await _uow.AnswerRepository.GetAll().Where(a => a.QuestionId == question.QuestionId).ToListAsync();
                foreach (var item in previousAnswers)
                {
                    _uow.AnswerRepository.Delete(item);
                }
                await _uow.CommitAsync();
            }

            foreach (var a in question.Answers)
            {
                var ans = new Answer();
                ans.CreatedAt = DateTime.UtcNow;
                ans.ModifiedAt = DateTime.UtcNow;
                ans.Order = a.Order;
                ans.Title = a.Title;
                ans.Weight = a.Weight;
                answers.Add(ans);
            }
            return answers;
        }

        public async Task<IEnumerable<Question>> GetQuestionsAdminAsync(int loggedUser)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var questions = new List<Question>();

            var ques = await _uow.QuestionRepository.GetAll().Where(q => q.Poll.Concept.Codename == CodeNamesConstants.HEALTH_MEASURES)
                .Include(q => q.Poll)
                    .ThenInclude(p => p.Concept)
                .ToListAsync();
            questions.AddRange(ques);

            ques = await _uow.QuestionRepository.GetAll().Where(q => q.Poll.Concept.Codename == CodeNamesConstants.VALUE_MEASURES)
                .Include(q => q.Poll)
                    .ThenInclude(p => p.Concept)
                .ToListAsync();
            questions.AddRange(ques);

            ques = await _uow.QuestionRepository.GetAll().Where(q => q.Poll.Concept.Codename == CodeNamesConstants.WELLNESS_MEASURES)
                .Include(q => q.Poll)
                    .ThenInclude(p => p.Concept)
                .ToListAsync();
            questions.AddRange(ques);

            return questions;
        }

        public async Task ChangeQuestionTranslationAsync(int loggedUser, QuestionTranslationRequest questionTranslationRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var question = await _uow.QuestionRepository.GetAll().Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            if (question == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Question");
            }

            switch (questionTranslationRequest.Lang)
            {
                case "en":
                    question.TitleEN = questionTranslationRequest.Title;
                    break;

                case "it":
                    question.TitleIT = questionTranslationRequest.Title;
                    break;

                default:
                    question.Title = questionTranslationRequest.Title;
                    break;
            }

            _uow.QuestionRepository.Update(question);
            await _uow.CommitAsync();
        }
    }
}