using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Utils
{
    public class ProfileHelthHelper : IProfileHelthHelper
    {
        private readonly IUnitOfWork _uow;

        public ProfileHelthHelper(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<(User user, double kcal, double IMC, DateTime? firstHealtMeasured)> GetUserProfileUseAsync(int loggedUser)
        {
            var user = await _uow.UserRepository.GetAll()
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.Subscription)
                .Where(u => u.Id == loggedUser)
                .FirstOrDefaultAsync();

            var kcal = 0.0;
            var imc = 0.0;
            DateTime? first = null;

            if (user.CurrentImc <= 0 || user.CurrentKcal <= 0 || user.FirtsHealthMeasured.HasValue == false)
            {
                var info = await GetCurrentKCalIMCFirstHeltMeasureAsync(user.Id);
                kcal = info.kcal;
                imc = info.imc;
                first = info.firstHealthMeasure;
                // patch for current users, not needed for new users or old users at second time
                user.CurrentImc = imc;
                user.CurrentKcal = kcal;
                user.FirtsHealthMeasured = first;
                await _uow.UserRepository.UpdateAsync(user, loggedUser);
                await _uow.CommitAsync();
            }
            else
            {
                kcal = user.CurrentKcal;
                imc = user.CurrentImc;
                first = user.FirtsHealthMeasured;
            }

            return (user, kcal, imc, first);
        }

        public async Task<(double kcal, double imc, DateTime? firstHealthMeasure)> GetCurrentKCalIMCFirstHeltMeasureAsync(int userId)
        {
            try
            {
                var concept = await _uow.ConceptRepository.FindAsync(c => c.Codename == CodeNamesConstants.HEALTH_MEASURES);

                if (concept != null)
                {
                    var polls = await _uow.PollRepository.GetAll().Where(p => p.ConceptId == concept.Id)
                          .Include(p => p.Questions)
                          .OrderBy(p => p.Order)
                          .ToListAsync();
                    // this is hardcoded but is the way it is.

                    // poll 1- personal data
                    var questions = polls.ElementAt(0).Questions.OrderBy(q => q.Order);

                    var age = 0;
                    var weight = 0;
                    var height = 0;
                    var sex = 0;
                    DateTime? firstHealthMeasure = null;

                    var count = 0;
                    foreach (var q in questions)
                    {
                        var ua = await _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                            .Include(u => u.Answer)
                                .ThenInclude(a => a.Question)
                            .OrderByDescending(ua => ua.CreatedAt)
                            .FirstOrDefaultAsync();
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
                        var ua = await _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == physicalQuestion.Id)
                                .Include(u => u.Answer)
                                    .ThenInclude(a => a.Question)
                                .OrderByDescending(ua => ua.CreatedAt)
                                .FirstOrDefaultAsync();
                        if (ua != null)
                        {
                            physicalExercise = ua.Answer.Weight;
                        }
                    }

                    // other values
                    var c = await _uow.UserConceptRepository.GetAll().Where(c => c.UserId == userId && c.ConceptId == concept.Id).FirstOrDefaultAsync();

                    var IMC = Convert.ToDouble(weight) / ((Convert.ToDouble(height) / 100) * ((Convert.ToDouble(height) / 100)));
                    var TMB_PROV = 10 * Convert.ToDouble(weight) + 6.25 * Convert.ToDouble(height) - 5 * age;

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

                    if (c != null)
                    {
                        firstHealthMeasure = c.CompletedAt;
                    }

                    return (dailyKalDouble, IMC, firstHealthMeasure);
                }
            }
            catch (Exception)
            {
                return (0.0, 0.0, null);
            }

            return (0.0, 0.0, null);
        }
    }
}
