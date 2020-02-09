﻿using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
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
            var questions = await _uow.QuestionRepository.GetAll().Where(q => q.PollId == pd.Id).ToListAsync();
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

            _uow.QuestionRepository.Delete(pd);
            await _uow.CommitAsync();
        }
    }
}