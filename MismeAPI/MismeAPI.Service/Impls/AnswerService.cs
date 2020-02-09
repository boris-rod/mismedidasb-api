using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
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