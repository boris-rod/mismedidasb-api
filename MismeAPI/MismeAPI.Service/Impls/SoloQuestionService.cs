using CorePush.Google;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.CutPoints;
using MismeAPI.Common.DTO.Request.SoloAnswer;
using MismeAPI.Common.DTO.Request.SoloQuestion;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class SoloQuestionService : ISoloQuestionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public SoloQuestionService(IUnitOfWork uow, IConfiguration config)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<PaginatedList<SoloQuestion>> GetQuestionsAsync(int pag, int perPag, string sortOrder, string search)
        {
            var result = _uow.SoloQuestionRepository.GetAll().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(i => i.Title.ToLower().Contains(search.ToLower()) || i.TitleEN.ToString().Contains(search) || i.TitleIT.ToString().Contains(search));
            }

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "title_desc":
                        result = result.OrderByDescending(i => i.Title);
                        break;

                    case "title_asc":
                        result = result.OrderBy(i => i.Title);
                        break;

                    case "titleEN_desc":
                        result = result.OrderByDescending(i => i.TitleEN);
                        break;

                    case "titleEN_asc":
                        result = result.OrderBy(i => i.TitleEN);
                        break;

                    case "titleIT_desc":
                        result = result.OrderByDescending(i => i.TitleIT);
                        break;

                    case "titleIT_asc":
                        result = result.OrderBy(i => i.TitleIT);
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<SoloQuestion>.CreateAsync(result, pag, perPag);
        }

        public async Task<PaginatedList<SoloQuestion>> GetUserQuestionsForTodayAsync(int userId, int pag, int perPag)
        {
            var today = DateTime.UtcNow;
            var yesterday = today.AddDays(-1).Date;

            var result = _uow.SoloQuestionRepository.GetAll().Include(sq => sq.SoloAnswers).AsQueryable();

            var userAnswers = await _uow.UserSoloAnswerRepository.GetAll()
                .Where(u => u.Id == userId && u.CreatedAt.Date == yesterday)
                .ToListAsync();

            var plannedExcercicesYesterday = userAnswers.Any(ua => ua.AnswerCode == "SQ-3-SA-1");

            //if (!plannedExcercicesYesterday)
            //    result = result.Where(sq => sq.Code != "SQ-4");

            //var userEat = await _uow.EatRepository.FindAsync(e => e.UserId == userId && e.CreatedAt.Date == today.Date);
            //if (userEat == null)
            //    result = result.Where(sq => sq.Code != "SQ-1");

            return await PaginatedList<SoloQuestion>.CreateAsync(result, pag, perPag);
        }

        public async Task<SoloQuestion> GetAsync(int id)
        {
            var soloQuestion = await _uow.SoloQuestionRepository.GetAsync(id);
            if (soloQuestion == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Question");

            return soloQuestion;
        }

        public async Task<SoloQuestion> CreateAsync(CreateSoloQuestionRequest request)
        {
            var soloQuestion = await _uow.SoloQuestionRepository.FindAsync(c => c.Code == request.Code);
            if (soloQuestion != null)
                throw new AlreadyExistsException("Question already exists");

            soloQuestion = new SoloQuestion
            {
                Code = request.Code,
                Title = request.Title,
                TitleEN = request.TitleEN,
                TitleIT = request.TitleIT,
                AllowCustomAnswer = request.AllowCustomAnswer,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                SoloAnswers = new List<SoloAnswer>()
            };

            foreach (var answerRequest in request.SoloAnswers)
            {
                var answer = await _uow.SoloAnswerRepository.FindAsync(c => c.Code == answerRequest.Code);
                if (answer != null)
                    throw new AlreadyExistsException("Answer already exists");

                answer = new SoloAnswer
                {
                    Code = answerRequest.Code,
                    Title = answerRequest.Title,
                    TitleEN = answerRequest.TitleEN,
                    TitleIT = answerRequest.TitleIT,
                    Points = answerRequest.Points,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                soloQuestion.SoloAnswers.Add(answer);
            }

            await _uow.SoloQuestionRepository.AddAsync(soloQuestion);
            await _uow.CommitAsync();

            return soloQuestion;
        }

        public async Task<SoloQuestion> UpdateAsync(int id, UpdateSoloQuestionRequest request)
        {
            var SoloQuestion = await GetAsync(id);

            SoloQuestion.Title = request.Title;
            SoloQuestion.TitleEN = request.TitleEN;
            SoloQuestion.TitleIT = request.TitleIT;
            SoloQuestion.ModifiedAt = DateTime.UtcNow;

            await _uow.SoloQuestionRepository.UpdateAsync(SoloQuestion, id);
            await _uow.CommitAsync();

            return SoloQuestion;
        }

        public async Task DeleteAsync(int id)
        {
            var SoloQuestion = await GetAsync(id);

            _uow.SoloQuestionRepository.Delete(SoloQuestion);
            await _uow.CommitAsync();
        }

        public async Task<UserSoloAnswer> SetUserAnswerAsync(int loggedUser, CreateUserSoloAnswerRequest answerRequest)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);

            var soloQuestion = await _uow.SoloQuestionRepository.GetAll()
                .Include(sq => sq.SoloAnswers)
                .Where(sq => sq.Code == answerRequest.QuestionCode)
                .FirstOrDefaultAsync();

            if (soloQuestion == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Question");

            var answer = soloQuestion.SoloAnswers.Where(sa => sa.Code == answerRequest.AnswerCode).FirstOrDefault();

            if (answer == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Answer");

            var userAnswer = new UserSoloAnswer
            {
                UserId = loggedUser,
                SoloAnswerId = answer.Id,
                AnswerValue = answerRequest.AnswerValue,
                Points = answer.Points,
                Coins = answer.Points,
                QuestionCode = soloQuestion.Code,
                AnswerCode = answer.Code,
                CreatedAt = DateTime.UtcNow
            };

            // TODO validate that no answer has been registered today for each question before
            // accept this one!

            await _uow.UserSoloAnswerRepository.AddAsync(userAnswer);
            await _uow.CommitAsync();

            return userAnswer;
        }

        public async Task SeedSoloQuestionsAsync()
        {
            var question1 = new CreateSoloQuestionRequest
            {
                Code = "SQ-1",
                Title = "¿Has cumplido tu Plan de Comidas?",
                TitleEN = "Did you meet your eat plan?",
                TitleIT = "",
                AllowCustomAnswer = false,
                SoloAnswers = new List<CreateSoloAnswerRequest>
                {
                    new CreateSoloAnswerRequest
                    {
                        Code="SQ-1-SA-1",
                        Title="Si",
                        TitleEN="Yes",
                        TitleIT="",
                        Points = 10
                    },
                    new CreateSoloAnswerRequest
                    {
                        Code="SQ-1-SA-2",
                        Title="No, he comido más",
                        TitleEN="No, I eat more",
                        TitleIT="",
                        Points = 5
                    },
                    new CreateSoloAnswerRequest
                    {
                        Code="SQ-1-SA-3",
                        Title="No, he comido menos",
                        TitleEN="No, I eat less",
                        TitleIT="",
                        Points = 5
                    }
                }
            };

            var question2 = new CreateSoloQuestionRequest
            {
                Code = "SQ-2",
                Title = "¿Cómo te has sentido hoy?",
                TitleEN = "How did you felt today?",
                TitleIT = "",
                AllowCustomAnswer = true,
                SoloAnswers = new List<CreateSoloAnswerRequest>
                {
                    new CreateSoloAnswerRequest
                    {
                        Code="SQ-2-SA-1",
                        Title="Escala Numerica",
                        TitleEN="Numeric scale",
                        TitleIT="",
                        Points = 10
                    }
                }
            };

            var question3 = new CreateSoloQuestionRequest
            {
                Code = "SQ-3",
                Title = "¿A qué hora planeas realizar ejercicios físicos mañana?",
                TitleEN = "Which time do you plan to make excersices tomorrow?",
                TitleIT = "",
                AllowCustomAnswer = true,
                SoloAnswers = new List<CreateSoloAnswerRequest>
                {
                    new CreateSoloAnswerRequest
                    {
                        Code="SQ-3-SA-1",
                        Title="Si, seleccionar hora",
                        TitleEN="Yes, select an hour",
                        TitleIT="",
                        Points = 10
                    },
                    new CreateSoloAnswerRequest
                    {
                        Code="SQ-3-SA-2",
                        Title="No hare ejercicios",
                        TitleEN="I will not do excercises",
                        TitleIT="",
                        Points = 5
                    }
                }
            };

            var question4 = new CreateSoloQuestionRequest
            {
                Code = "SQ-4",
                Title = "¿Has realizado los ejercicios habías planificado para hoy?",
                TitleEN = "Did you make the excersices planned for today?",
                TitleIT = "",
                AllowCustomAnswer = false,
                SoloAnswers = new List<CreateSoloAnswerRequest>
                {
                    new CreateSoloAnswerRequest
                    {
                        Code="SQ-4-SA-1",
                        Title="Si",
                        TitleEN="Yes",
                        TitleIT="",
                        Points = 10
                    },
                    new CreateSoloAnswerRequest
                    {
                        Code="SQ-4-SA-2",
                        Title="Mas del previsto",
                        TitleEN="More than planned",
                        TitleIT="",
                        Points = 5
                    },
                    new CreateSoloAnswerRequest
                    {
                        Code="SQ-4-SA-3",
                        Title="Menos del planificado",
                        TitleEN="Less than planned",
                        TitleIT="",
                        Points = 5
                    },
                    new CreateSoloAnswerRequest
                    {
                        Code="SQ-4-SA-4",
                        Title="No",
                        TitleEN="No",
                        TitleIT="",
                        Points = 5
                    }
                }
            };

            var soloQuestion1 = await _uow.SoloQuestionRepository.FindAsync(c => c.Code == question1.Code);
            if (soloQuestion1 == null)
                await CreateAsync(question1);

            var soloQuestion2 = await _uow.SoloQuestionRepository.FindAsync(c => c.Code == question2.Code);
            if (soloQuestion2 == null)
                await CreateAsync(question2);

            var soloQuestion3 = await _uow.SoloQuestionRepository.FindAsync(c => c.Code == question3.Code);
            if (soloQuestion3 == null)
                await CreateAsync(question3);

            var soloQuestion4 = await _uow.SoloQuestionRepository.FindAsync(c => c.Code == question4.Code);
            if (soloQuestion4 == null)
                await CreateAsync(question4);
        }
    }
}
