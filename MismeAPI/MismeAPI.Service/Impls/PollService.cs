using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Poll;
using MismeAPI.Common.DTO.Request.Tip;
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

        public async Task ActivateTipAsync(int loggedUser, int id, int pollId, int position)
        {
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var tip = await _uow.TipRepository.GetAsync(id);
            if (tip == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Tip");
            }

            tip.IsActive = true;

            var otherTips = await _uow.TipRepository.GetAll().Where(t => t.PollId == pollId && t.TipPosition == (TipPositionEnum)position && t.Id != id).ToListAsync();
            foreach (var item in otherTips)
            {
                item.IsActive = false;
                await _uow.TipRepository.UpdateAsync(item, item.Id);
            }

            await _uow.TipRepository.UpdateAsync(tip, tip.Id);
            await _uow.CommitAsync();
        }

        public async Task<Tip> AddTipRequestAsync(int loggedUser, AddTipRequest tipRequest)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var tip = new Tip();
            tip.Content = tipRequest.Content;
            tip.PollId = tipRequest.PollId;
            tip.IsActive = tipRequest.IsActive;
            tip.TipPosition = (TipPositionEnum)tipRequest.TipPosition;
            tip.CreatedAt = DateTime.UtcNow;
            tip.ModifiedAt = DateTime.UtcNow;

            await _uow.TipRepository.AddAsync(tip);
            await _uow.CommitAsync();
            return tip;
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

        public async Task DeleteTipAsync(int loggedUser, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var tip = await _uow.TipRepository.GetAsync(id);
            if (tip == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Tip");
            }
            _uow.TipRepository.Delete(tip);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<Poll>> GetAllPollsAsync(int conceptId)
        {
            var result = _uow.PollRepository.GetAll()
                .Include(p => p.Concept)
                .Include(p => p.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(p => p.Tips)
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
                .Include(p => p.Tips)
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
                .Include(p => p.Tips)
                .FirstOrDefaultAsync();
            if (poll == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }
            return poll;
        }

        //public async Task SetPollResultAsync(int loggedUser, SetPollResultRequest pollResult)
        //{
        //    // not found poll?
        //    var pd = await _uow.PollRepository.GetAsync(pollResult.PollId);
        //    if (pd == null)
        //    {
        //        throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
        //    }
        //    var userPoll = new UserPoll();
        //    userPoll.PollId = pd.Id;
        //    userPoll.Result = pollResult.Result;
        //    userPoll.UserId = loggedUser;
        //    userPoll.CompletedAt = DateTime.UtcNow;
        //    await _uow.UserPollRepository.AddAsync(userPoll);
        //    await _uow.CommitAsync();
        //}

        public async Task<List<string>> SetPollResultByQuestionsAsync(int loggedUser, ListOfPollResultsRequest result, string language)
        {
            var message = new List<string>();
            var poll = await _uow.PollRepository.GetAll().Where(p => p.Id == result.PollDatas.ElementAt(0).PollId)
                  .Include(p => p.Concept)
                  .FirstOrDefaultAsync();
            if (poll != null)
            {
                // not found poll?
                var pda = await _uow.UserConceptRepository.GetAll().Where(p => p.ConceptId == poll.ConceptId && p.CompletedAt.Date == DateTime.UtcNow.Date && p.UserId == loggedUser)
                    .FirstOrDefaultAsync();

                foreach (var elem in result.PollDatas)
                {
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
                    }
                }

                await _uow.CommitAsync();

                //// CALCULATE THE RESULT OF THE CONCEPT

                if (poll.Concept.Codename == CodeNamesConstants.HEALTH_MEASURES)
                {
                    message = GetHealthMeasureMessage(poll.ConceptId, loggedUser, language);
                }
                else if (poll.Concept.Codename == CodeNamesConstants.VALUE_MEASURES)
                {
                    message = GetValueMeasureMessage(poll.ConceptId, loggedUser, language);
                }
                else if (poll.Concept.Codename == CodeNamesConstants.WELLNESS_MEASURES)
                {
                    message = GetWellnessMeasureMessage(poll.ConceptId, loggedUser, language);
                }

                // set concept result
                var conceptToUpdate = await _uow.UserConceptRepository.GetAll().Where(c => c.Id == poll.ConceptId
                               && c.UserId == loggedUser
                               && c.CompletedAt.Date == DateTime.UtcNow.Date)
                       .FirstOrDefaultAsync();
                if (conceptToUpdate == null)
                {
                    var conc = new UserConcept();
                    conc.CompletedAt = DateTime.UtcNow;
                    conc.ConceptId = poll.ConceptId;
                    conc.UserId = loggedUser;
                    await _uow.UserConceptRepository.AddAsync(conc);
                    await _uow.CommitAsync();
                }
            }

            return message;
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
                .Include(p => p.Tips)
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

        public async Task<Tip> UpdateTipContentAsync(int loggedUser, string content, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            // not found tip?
            var pd = await _uow.TipRepository.GetAll().Where(p => p.Id == id)
                .FirstOrDefaultAsync();
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Tip");
            }
            pd.Content = content;
            await _uow.TipRepository.UpdateAsync(pd, id);
            await _uow.CommitAsync();

            return pd;
        }

        public async Task<(int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat)> GetUserPollsInfoAsync(int userId)
        {
            var age = 0;
            var weight = 0;
            var height = 0;
            var sex = 0;
            DateTime? healthMeasuresLastUpdate = null;
            DateTime? valueMeasuresLastUpdate = null;
            DateTime? wellnessMeasuresLastUpdate = null;
            DateTime? lastPlanedEat = null;

            var concepts = await _uow.ConceptRepository.GetAllAsync();
            var concept = concepts.FirstOrDefault(c => c.Codename == CodeNamesConstants.HEALTH_MEASURES);

            if (concept != null)
            {
                var polls = _uow.PollRepository.GetAll().Where(p => p.ConceptId == concept.Id)
                    .Include(p => p.Questions)
                    .OrderBy(p => p.Order)
                    .ToList();

                // poll 1- personal data
                var questions = polls.ElementAt(0).Questions.OrderBy(q => q.Order);

                var count = 0;
                foreach (var q in questions)
                {
                    var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                        .Include(u => u.Answer)
                            .ThenInclude(a => a.Question)
                        .OrderByDescending(ua => ua.CreatedAt)
                        .FirstOrDefault();

                    if (ua != null && ua.Answer != null)
                    {
                        //age
                        if (count == 0)
                        {
                            age = ua.Answer.Weight;
                        }
                        //weight
                        else if (count == 1)
                        {
                            weight = ua.Answer.Weight;
                        }
                        //height
                        else if (count == 2)
                        {
                            height = ua.Answer.Weight;
                        }
                        //sex
                        else
                        {
                            sex = ua.Answer.Weight;
                        }
                    }

                    count += 1;
                }

                healthMeasuresLastUpdate = await GetLastConceptUpdateAsync(concept.Id, userId);
            }

            concept = concepts.FirstOrDefault(c => c.Codename == CodeNamesConstants.VALUE_MEASURES);
            if (concept != null)
            {
                valueMeasuresLastUpdate = await GetLastConceptUpdateAsync(concept.Id, userId);
            }

            concept = concepts.FirstOrDefault(c => c.Codename == CodeNamesConstants.WELLNESS_MEASURES);
            if (concept != null)
            {
                wellnessMeasuresLastUpdate = await GetLastConceptUpdateAsync(concept.Id, userId);
            }

            var userEat = await _uow.EatRepository.GetAll()
                .Where(e => e.UserId == userId)
               .OrderByDescending(e => e.ModifiedAt)
               .FirstOrDefaultAsync();

            if (userEat != null)
                lastPlanedEat = userEat.ModifiedAt;

            return (age, weight, height, sex, healthMeasuresLastUpdate, valueMeasuresLastUpdate, wellnessMeasuresLastUpdate, lastPlanedEat);
        }

        public async Task<IEnumerable<Question>> GetLatestPollAnswerByUser(string conceptCode, int userId)
        {
            if (conceptCode != CodeNamesConstants.VALUE_MEASURES && conceptCode != CodeNamesConstants.WELLNESS_MEASURES)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Concept name ");
            }

            var concept = await _uow.ConceptRepository.FindAsync(c => c.Codename == conceptCode);
            if (concept == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Concept ");
            }

            var poll = _uow.PollRepository.GetAll().Where(p => p.ConceptId == concept.Id)
              .Include(p => p.Questions)
              .OrderBy(p => p.Order)
              .FirstOrDefault();

            var result = new List<Question>();
            if (poll != null)
            {
                var questions = poll.Questions.OrderBy(q => q.Order);
                foreach (var q in questions)
                {
                    q.Answers = new List<Answer>();

                    var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                           .Include(u => u.Answer)
                           .OrderByDescending(ua => ua.CreatedAt)
                           .FirstOrDefault();

                    if (ua != null)
                        q.Answers.Add(ua.Answer);

                    result.Add(q);
                }
            }

            return result;
        }

        private async Task<DateTime?> GetLastConceptUpdateAsync(int conceptId, int userId)
        {
            var userConcept = await _uow.UserConceptRepository.GetAll()
                    .Where(c => c.Id == conceptId && c.UserId == userId)
                    .OrderByDescending(c => c.CompletedAt)
                    .FirstOrDefaultAsync();

            return userConcept?.CompletedAt;
        }

        private List<string> GetHealthMeasureMessage(int conceptId, int userId, string language)
        {
            var result = new List<string>();
            var polls = _uow.PollRepository.GetAll().Where(p => p.ConceptId == conceptId)
                .Include(p => p.Questions)
                .OrderBy(p => p.Order)
                .ToList();
            // this is hardcoded but is the way it is.

            // poll 1- personal data
            var questions = polls.ElementAt(0).Questions.OrderBy(q => q.Order);

            var age = 0;
            var weight = 0;
            var height = 0;
            var sex = 0;

            var count = 0;
            foreach (var q in questions)
            {
                var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                    .Include(u => u.Answer)
                        .ThenInclude(a => a.Question)
                    .OrderByDescending(ua => ua.CreatedAt)
                    .FirstOrDefault();
                //age
                if (count == 0 && ua != null)
                {
                    age = ua.Answer.Weight;
                }
                //weight
                else if (count == 1 && ua != null)
                {
                    weight = ua.Answer.Weight;
                }
                //height
                else if (count == 2 && ua != null)
                {
                    height = ua.Answer.Weight;
                }
                //sex
                else
                {
                    sex = ua.Answer.Weight;
                }

                count += 1;
            }

            //poll 2- Physical excersice
            var physicalExercise = 0;
            var physicalQuestion = polls.ElementAt(1).Questions.OrderBy(q => q.Order).FirstOrDefault();
            if (physicalQuestion != null)
            {
                var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == physicalQuestion.Id)
                        .Include(u => u.Answer)
                            .ThenInclude(a => a.Question)
                        .OrderByDescending(ua => ua.CreatedAt)
                        .FirstOrDefault();
                if (ua != null)
                {
                    physicalExercise = ua.Answer.Weight;
                }
            }
            //poll 3- Diet
            var dietSummary = 0;
            questions = polls.ElementAt(2).Questions.OrderBy(q => q.Order);

            foreach (var q in questions)
            {
                var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                    .Include(u => u.Answer)
                        .ThenInclude(a => a.Question)
                    .OrderByDescending(ua => ua.CreatedAt)
                    .FirstOrDefault();

                dietSummary += ua.Answer.Weight;
            }

            // other values
            var IMC = Convert.ToDouble(weight) / ((Convert.ToDouble(height) / 100) * ((Convert.ToDouble(height) / 100)));
            var TMB_PROV = 10 * Convert.ToDouble(weight) + 6.25 * Convert.ToDouble(height) - 5 * Convert.ToDouble(age);

            var dailyKalDouble = 0.0;

            if (sex == 1)
            {
                if (physicalExercise == 1)
                {
                    dailyKalDouble = (TMB_PROV + 5) * 1.2;
                }
                else if (physicalExercise == 2)
                {
                    dailyKalDouble = (TMB_PROV + 5) * 1.375;
                }
                else if (physicalExercise == 3)
                {
                    dailyKalDouble = (TMB_PROV + 5) * 1.55;
                }
                else if (physicalExercise == 4)
                {
                    dailyKalDouble = (TMB_PROV + 5) * 1.725;
                }
                else
                {
                    dailyKalDouble = (TMB_PROV + 5) * 1.9;
                }
            }
            else
            {
                if (physicalExercise == 1)
                {
                    dailyKalDouble = (TMB_PROV - 161) * 1.2;
                }
                else if (physicalExercise == 2)
                {
                    dailyKalDouble = (TMB_PROV - 161) * 1.375;
                }
                else if (physicalExercise == 3)
                {
                    dailyKalDouble = (TMB_PROV - 161) * 1.55;
                }
                else if (physicalExercise == 4)
                {
                    dailyKalDouble = (TMB_PROV - 161) * 1.725;
                }
                else
                {
                    dailyKalDouble = (TMB_PROV - 161) * 1.9;
                }
            }

            var IMCString = string.Format("{0:0.00 }", IMC);

            var dailyKalString = string.Format("{0:0.00 }", dailyKalDouble);

            if (IMC < 15)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_EXTREME).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString);
                }

                result.Add(text);
            }
            else if (IMC >= 15 && IMC < 16)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_SEVERE).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString);
                }

                result.Add(text);
            }

            //Resultados bajo peso 16-17 con deporte y wc<7
            else if (IMC >= 16 &&
                IMC < 17 &&
                dietSummary <= 7 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_MODERATE_1).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 16 &&
              IMC < 17 &&
              dietSummary <= 7 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_MODERATE_2).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 16 &&
              IMC < 17 &&
              dietSummary <= 7 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_MODERATE_3).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 16 &&
              IMC < 17 &&
              dietSummary <= 7 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_MODERATE_4).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 16 &&
              IMC < 17 &&
              dietSummary <= 7 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_MODERATE_5).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Resultados bajo peso 16-17 con deporte y wc>7
            else if (IMC >= 16 &&
                IMC < 17 &&
                dietSummary > 7 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_MODERATE_6).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 16 &&
              IMC < 17 &&
              dietSummary > 7 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_MODERATE_7).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 16 &&
              IMC < 17 &&
              dietSummary > 7 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_MODERATE_8).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 16 &&
              IMC < 17 &&
              dietSummary > 7 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_MODERATE_9).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 16 &&
              IMC < 17 &&
              dietSummary > 7 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_MODERATE_10).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Resultados bajo peso 17-18 con deporte y wc<7
            else if (IMC >= 17 &&
                IMC < 18.5 &&
                dietSummary <= 7 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_LIGHT_1).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 17 &&
              IMC < 18.5 &&
              dietSummary <= 7 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_LIGHT_2).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 17 &&
              IMC < 18.5 &&
              dietSummary <= 7 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_LIGHT_3).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 17 &&
              IMC < 18.5 &&
              dietSummary <= 7 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_LIGHT_4).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 17 &&
              IMC < 18.5 &&
              dietSummary <= 7 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_MODERATE_11).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Resultados bajo peso 17-18 con deporte y wc>7 //Instrucciones para Peso Normal
            else if (IMC >= 17 &&
                IMC < 18.5 &&
                dietSummary > 7 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_LIGHT_5).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 17 &&
              IMC < 18.5 &&
              dietSummary > 7 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_LIGHT_6).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 17 &&
              IMC < 18.5 &&
              dietSummary > 7 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_LIGHT_7).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 17 &&
              IMC < 18.5 &&
              dietSummary > 7 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_LIGHT_8).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 17 &&
              IMC < 18.5 &&
              dietSummary > 7 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.UNDER_WEIGHT_LIGHT_9).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Peso Normal deporte wc<4
            else if (IMC >= 18.5 &&
                IMC < 25 &&
                dietSummary <= 4 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_1).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary <= 4 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_2).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary <= 4 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_3).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary <= 4 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_4).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary <= 4 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_5).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Peso Normal deporte wc entre 4 y 7
            else if (IMC >= 18.5 &&
                IMC < 25 &&
                dietSummary > 4 &&
                dietSummary <= 7 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_6).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_7).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_8).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_9).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_10).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Peso Normal deporte wc entre 7 y 12
            else if (IMC >= 18.5 &&
                IMC < 25 &&
                dietSummary > 7 &&
                dietSummary <= 12 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_11).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_12).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_13).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_14).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_15).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Peso Normal deporte wc > 12
            else if (IMC >= 18.5 &&
                IMC < 25 &&
                dietSummary > 12 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_16).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 12 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_17).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 12 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_18).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 12 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_19).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 18.5 &&
              IMC < 25 &&
              dietSummary > 12 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.NORMAL_WEIGHT_20).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Sobrepeso deporte wc<4
            else if (IMC >= 25 &&
                IMC < 30 &&
                dietSummary <= 4 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_1).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary <= 4 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_2).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary <= 4 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_3).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary <= 4 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_4).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary <= 4 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_5).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Sobrepeso deporte wc entre 4 y 7
            else if (IMC >= 25 &&
                IMC < 30 &&
                dietSummary > 4 &&
                dietSummary <= 7 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_6).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_7).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_8).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_9).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_10).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Sobrepeso deporte wc entre 7 y 12
            else if (IMC >= 25 &&
                IMC < 30 &&
                dietSummary > 7 &&
                dietSummary <= 12 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_11).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_12).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_13).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_14).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_15).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Sobrepeso deporte wc >12
            else if (IMC >= 25 &&
                IMC < 30 &&
                dietSummary > 12 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_16).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 12 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_17).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 12 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_18).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 12 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_19).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 25 &&
              IMC < 30 &&
              dietSummary > 12 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OVER_WEIGHT_20).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Obesidad <40 deporte wc<4
            else if (IMC >= 30 &&
                IMC < 40 &&
                dietSummary <= 4 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_1).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary <= 4 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_2).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary <= 4 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_3).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary <= 4 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_4).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary <= 4 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_5).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Obesidad <40 deporte wc de 5 a 7
            else if (IMC >= 30 &&
                IMC < 40 &&
                dietSummary > 4 &&
                dietSummary <= 7 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_6).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_7).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_8).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_9).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 4 &&
              dietSummary <= 7 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_10).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Obesidad <35 deporte wc de 7 a 12
            else if (IMC >= 30 &&
                IMC < 40 &&
                dietSummary > 7 &&
                dietSummary <= 12 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_11).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_12).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_13).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_14).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 7 &&
              dietSummary <= 12 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_15).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para Obesidad <40 deporte wc>12
            else if (IMC >= 30 &&
                IMC < 40 &&
                dietSummary > 12 &&
                physicalExercise == 1)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_16).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 12 &&
              physicalExercise == 2)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_17).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 12 &&
              physicalExercise == 3)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_18).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 12 &&
              physicalExercise == 4)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_19).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }
            else if (IMC >= 30 &&
              IMC < 40 &&
              dietSummary > 12 &&
              physicalExercise == 5)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_20).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString).Replace("[number2]", dailyKalString);
                }

                result.Add(text);
            }

            //Instrucciones para obesidad mórbida y extrema
            else if (IMC >= 40 && IMC < 50)
            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_21).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString);
                }

                result.Add(text);
            }
            else if (IMC >= 50)

            {
                var text = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.OBESITY_21).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        text = results.Text;
                    }
                    else
                    {
                        text = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                    text = text.Replace("[number1]", IMCString);
                }

                result.Add(text);
            }

            return result;
        }

        private List<string> GetWellnessMeasureMessage(int conceptId, int userId, string language)
        {
            var result = new List<string>();

            // this assume only one poll, it is hardcoded
            var poll = _uow.PollRepository.GetAll().Where(p => p.ConceptId == conceptId)
              .Include(p => p.Questions)
              .OrderBy(p => p.Order)
              .FirstOrDefault();
            if (poll != null)
            {
                var value1 = 0;
                var value2 = 0;
                var value3 = 0;
                var value4 = 0;
                var vGeneral = 0;

                var questions = poll.Questions.OrderBy(q => q.Order);
                var count = 0;
                foreach (var q in questions)
                {
                    var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                           .Include(u => u.Answer)
                               .ThenInclude(a => a.Question)
                           .OrderByDescending(ua => ua.CreatedAt)
                           .FirstOrDefault();
                    if (ua != null)
                    {
                        switch (count)
                        {
                            case 0:
                            case 2:
                            case 8:
                                value1 += ua.Answer.Weight;
                                break;

                            case 3:
                            case 5:
                            case 9:
                                value2 += ua.Answer.Weight;
                                break;

                            case 6:
                            case 7:
                                value3 += ua.Answer.Weight;
                                break;

                            default:
                                value4 += ua.Answer.Weight;
                                break;
                        }
                    }
                    count += 1;
                }

                vGeneral = value1 + value2 + value3 + value4;

                var general = "";
                var resp1 = "";
                var resp2 = "";
                var resp3 = "";
                var resp4 = "";
                //var additional = "";

                if (vGeneral >= 44)
                    general = "Elevados niveles de bienestar, caracterizado por:";
                else if (vGeneral < 44 && vGeneral >= 34)
                    general = "Percepción de bienestar caracterizada por:";
                else
                    general = "Bajos niveles de bienestar, caracterizado por:";

                if (value1 >= 14)
                {
                    resp1 = "1. Elevada búsqueda de ocio y diversión.";
                }
                else if (value1 > 10 && value1 < 14)
                {
                    resp1 = "1. Adecuado manejo del ocio y el tiempo libre.";
                }
                else
                    resp1 = "1. Actividades de ocio escasas o poco gratificantes.";

                if (value2 >= 13)
                {
                    resp2 = "2. Elevada satisfacción consigo mismo.";
                }
                else if (value2 > 9 && value2 < 13)
                {
                    resp2 = "2. Adecuada satisfacción consigo mismo.";
                }
                else
                    resp2 = "2. Baja satisfacción consigo mismo.";

                if (value3 >= 10)
                {
                    resp3 = "3. Elevada satisfacción con la actividad que realiza.";
                }
                else if (value3 > 7 && value3 < 10)
                {
                    resp3 = "3. Adecuada satisfacción con la actividad que realiza.";
                }
                else
                    resp3 = "3. Baja satisfacción con la actividad que realiza.";

                if (value4 >= 10)
                {
                    resp4 = "4. Se percibe como una persona muy saludable.";
                }
                else if (value4 > 6 && value4 < 10)
                {
                    resp4 = "4. Se percibe como una persona saludable.";
                }
                else
                    resp4 = "4. No se percibe como una persona saludable.";

                //additional =
                //    "Total $vGeneral, Hed = $value1, Si mismo = $value2, Activ = $value3, AutoI = $value4";

                // ad-hoc response
                var defaultResp = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.WELLNESS).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        defaultResp = results.Text;
                    }
                    else
                    {
                        defaultResp = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                }

                result.Add(defaultResp);
                result.Add(general);
                result.Add(resp1);
                result.Add(resp2);
                result.Add(resp3);
                result.Add(resp4);
            }

            return result;
        }

        private List<string> GetValueMeasureMessage(int conceptId, int userId, string language)
        {
            var result = new List<string>();

            // this assume only one poll, it is hardcoded
            var poll = _uow.PollRepository.GetAll().Where(p => p.ConceptId == conceptId)
              .Include(p => p.Questions)
              .OrderBy(p => p.Order)
              .FirstOrDefault();
            if (poll != null)
            {
                var value1 = 0;
                var value2 = 0;
                var value3 = 0;
                var value4 = 0;
                var value5 = 0;
                var value6 = 0;
                var value7 = 0;
                var value8 = 0;
                var value9 = 0;
                var value10 = 0;

                var questions = poll.Questions.OrderBy(q => q.Order);
                var count = 0;
                foreach (var q in questions)
                {
                    var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                           .Include(u => u.Answer)
                               .ThenInclude(a => a.Question)
                           .OrderByDescending(ua => ua.CreatedAt)
                           .FirstOrDefault();

                    switch (count)
                    {
                        case 0:
                            value1 = ua.Answer.Weight;
                            break;

                        case 1:
                            value2 = ua.Answer.Weight;
                            break;

                        case 2:
                            value3 = ua.Answer.Weight;
                            break;

                        case 3:
                            value4 = ua.Answer.Weight;
                            break;

                        case 4:
                            value5 = ua.Answer.Weight;
                            break;

                        case 5:
                            value6 = ua.Answer.Weight;
                            break;

                        case 6:
                            value7 = ua.Answer.Weight;
                            break;

                        case 7:
                            value8 = ua.Answer.Weight;
                            break;

                        case 8:
                            value9 = ua.Answer.Weight;
                            break;

                        default:
                            value10 = ua.Answer.Weight;
                            break;
                    }

                    count += 1;
                }

                var resp1 = "";
                var resp2 = "";
                var resp3 = "";
                var resp4 = "";
                var resp5 = "";
                var resp6 = "";
                var resp7 = "";
                var resp8 = "";
                var resp9 = "";
                var resp10 = "";

                resp1 = value1 >= 6 ? "1. Elevada tendencia a mostrar comportamientos socialmente esperados."
                        : (value1 > 4 && value1 < 6
                        ? "1. Tendencia moderada a mostrar comportamientos socialmente esperados."
                        : "1. Escasa tendencia a mostrar comportamientos socialmente esperados.");
                resp2 = value1 >= 6
                    ? "2. Profunda aceptación de las costumbres y tradiciones de su cultura."
                    : (value2 > 4 && value2 < 6
                        ? "2. Respeta las costumbres y tradiciones de su cultura."
                        : "2. Dificultades para aceptar las costumbres y tradiciones de su cultura.");
                resp3 = value3 >= 6
                    ? "3. Alta lealtad, honestidad, y compasión hacia personas cercanas."
                    : "3. Lealtad, honestidad y compasión no son sus valores primarios.";
                resp4 = value4 >= 6
                    ? "4. Gran compromiso con la protección de la naturaleza y la vida."
                    : (value4 > 4 && value4 < 6
                        ? "4. Considera relevante la protección naturaleza y la vida."
                        : "4. Poco compromiso con la protección de la naturaleza y la vida.");
                resp5 = value5 >= 6
                    ? "5. Elevada independencia de pensamiento y acción."
                    : "5. La independencia de pensamiento y acción no su punto fuerte.";
                resp6 = value6 >= 6
                    ? "6. Muy orientado hacia la búsqueda de nuevos retos y experiencias excitantes."
                    : (value6 > 4 && value6 < 6
                        ? "6. Le gustan los retos y las vivencias novedosas."
                        : "6. La búsqueda de retos y experiencias nuevas no es su punto fuerte.");
                resp7 = value7 >= 6
                    ? "7. Elevada búsqueda de placer y gratificación sensorial."
                    : (value7 > 4 && value7 < 6
                        ? "7. Otorga importancia al placer y la gratificación sensorial."
                        : "7. Otorga poca importancia al placer y la gratificación sensorial.");
                resp8 = value8 == 7
                    ? "8. El sacrificio y el trabajo duro son valores primarios."
                    : (value8 > 5 && value8 < 7
                        ? "8. Otorga importancia al sacrificio y al trabajo duro."
                        : "8. El sacrificio y el trabajo duro no son sus puntos fuertes.");
                resp9 = value9 >= 6
                    ? "9. Alta orientación hacia la obtención de estatus y/o prestigio."
                    : (value9 > 4 && value9 < 6
                    ? "9. Otorga importancia a la obtención de estatus y/o prestigio."
                    : "9. Otorga poca importancia a la obtención de estatus y/o prestigio.");
                resp10 = value10 == 7
                    ? "10. Alta orientación hacia la búsqueda de seguridad y estabilidad."
                    : (value10 > 5 && value10 < 7
                    ? "10. La seguridad y la estabilidad son valores importantes para usted."
                    : "10. Poca orientación hacia la búsqueda de seguridad y estabilidad.");

                // ad-hoc response
                var respDefault = "";
                var results = _uow.ResultRepository.GetAll().Where(r => r.CodeName == ResultConstants.VALUES).FirstOrDefault();
                if (results != null)
                {
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        respDefault = results.Text;
                    }
                    else
                    {
                        respDefault = language.ToUpper() == "EN" && !string.IsNullOrWhiteSpace(results.TextEN) ? results.TextEN :
                            (language.ToUpper() == "IT" && !string.IsNullOrWhiteSpace(results.TextIT) ? results.TextIT : results.Text);
                    }
                }

                result.Add(respDefault);
                result.Add(resp1);
                result.Add(resp2);
                result.Add(resp3);
                result.Add(resp4);
                result.Add(resp5);
                result.Add(resp6);
                result.Add(resp7);
                result.Add(resp8);
                result.Add(resp9);
                result.Add(resp10);
            }

            return result;
        }

        public async Task<List<Poll>> GetAllPollsAdminAsync(int loggedUser)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var polls = await _uow.PollRepository.GetAll().Include(p => p.Concept).ToListAsync();
            return polls;
        }

        public async Task ChangePollTranslationAsync(int loggedUser, PollTranslationRequest pollTranslationRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var poll = await _uow.PollRepository.GetAll().Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            if (poll == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }

            switch (pollTranslationRequest.Lang)
            {
                case "en":
                    poll.NameEN = pollTranslationRequest.Name;
                    poll.DescriptionEN = pollTranslationRequest.Description;
                    poll.HtmlContentEN = pollTranslationRequest.HtmlContent;
                    break;

                case "it":
                    poll.NameIT = pollTranslationRequest.Name;
                    poll.DescriptionIT = pollTranslationRequest.Description;
                    poll.HtmlContentIT = pollTranslationRequest.HtmlContent;
                    break;

                default:
                    poll.Name = pollTranslationRequest.Name;
                    poll.Description = pollTranslationRequest.Description;
                    poll.HtmlContent = pollTranslationRequest.HtmlContent;
                    break;
            }

            _uow.PollRepository.Update(poll);
            await _uow.CommitAsync();
        }

        public async Task<Dictionary<int, int>> GetLastAnsweredDictAsync(int loggedUser)
        {
            var userAnswers = await _uow.UserAnswerRepository.GetAll().Where(ua => ua.UserId == loggedUser)
                .Include(a => a.Answer)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            var dict = new Dictionary<int, int>();
            foreach (var item in userAnswers)
            {
                if (!dict.ContainsKey(item.Answer.QuestionId))
                {
                    dict.Add(item.Answer.QuestionId, item.AnswerId);
                }
            }
            return dict;
        }

        public async Task<bool> HasAnsweredConceptBeforeAsync(int loggedUser, ListOfPollResultsRequest result)
        {
            var poll = await _uow.PollRepository.GetAll().Where(p => p.Id == result.PollDatas.ElementAt(0).PollId)
                  .Include(p => p.Concept)
                  .FirstOrDefaultAsync();

            if (poll != null)
            {
                var existUserConcept = await _uow.UserConceptRepository.GetAll()
                    .Where(c => c.Id == poll.ConceptId && c.UserId == loggedUser)
                    .FirstOrDefaultAsync();

                if (existUserConcept != null)
                    return true;
            }

            return false;
        }

        public IEnumerable<int> GetAnsweredPolls(ListOfPollResultsRequest result)
        {
            var polls = new List<int>();
            foreach (var item in result.PollDatas)
            {
                if (item.PollId != 0 && !polls.Contains(item.PollId))
                {
                    polls.Add(item.PollId);
                }
            }

            return polls;
        }
    }
}
