﻿using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Question;
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

        Task<Question> UpdateQuestionTitleAsync(int loggedUser, int id, string title);

        Task<Question> AddOrUpdateQuestionWithAnswersAsync(int loggedUser, AddOrUpdateQuestionWithAnswersRequest question);

        Task<IEnumerable<Question>> GetQuestionsAdminAsync(int loggedUser);

        Task ChangeQuestionTranslationAsync(int loggedUser, QuestionTranslationRequest questionTranslationRequest, int id);
    }
}