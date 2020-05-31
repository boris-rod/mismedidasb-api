using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class EatService : IEatService
    {
        private readonly IUnitOfWork _uow;

        public EatService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<Eat> CreateEatAsync(int loggedUser, CreateEatRequest eat)
        {
            // this assume the user is creating an eat every day
            var eatTypeAlradyExists = await _uow.EatRepository.GetAll()
                .Where(e => e.CreatedAt.Date == DateTime.UtcNow.Date && e.EatType == (EatTypeEnum)eat.EatType)
                .FirstOrDefaultAsync();
            if (eatTypeAlradyExists != null)
            {
                throw new AlreadyExistsException("An eat is already configured this day.");
            }

            var e = new Eat();
            e.EatType = (EatTypeEnum)eat.EatType;
            e.CreatedAt = DateTime.UtcNow;
            e.ModifiedAt = DateTime.UtcNow;
            e.UserId = loggedUser;
            var eatDishes = new List<EatDish>();
            foreach (var ed in eat.Dishes)
            {
                var eatD = new EatDish();
                eatD.DishId = ed.DishId;
                eatD.Qty = ed.Qty;

                eatDishes.Add(eatD);
            }
            e.EatDishes = eatDishes;

            await _uow.EatRepository.AddAsync(e);
            await _uow.CommitAsync();
            return e;
        }

        public async Task<PaginatedList<Eat>> GetAdminAllUserEatsAsync(int adminId, int pag, int perPag, int userId, DateTime? date, int eatTyp)
        {
            var isAdmin = await _uow.UserRepository.GetAll().Where(u => u.Id == adminId && u.Role == RoleEnum.ADMIN).FirstOrDefaultAsync();
            if (isAdmin == null)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var userExists = await _uow.UserRepository.GetAll().Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (userExists == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            var results = _uow.EatRepository.GetAll().Where(e => e.UserId == userId)
              .Include(e => e.User)
              .Include(e => e.EatDishes)
                  .ThenInclude(ed => ed.Dish)
                      .ThenInclude(d => d.DishTags)
                          .ThenInclude(dt => dt.Tag)
              .Include(e => e.EatCompoundDishes)
                  .ThenInclude(ed => ed.CompoundDish)
                      .ThenInclude(d => d.DishCompoundDishes)
                          .ThenInclude(dt => dt.Dish)
                            .ThenInclude(dt => dt.DishTags)
                                .ThenInclude(dt => dt.Tag)
              .AsQueryable();

            if (date.HasValue)
            {
                results = results.Where(e => e.CreatedAt.Date == date.Value.Date);
            }

            //filter by type if not -1(null equivalent)
            if (eatTyp != -1)
            {
                results = results.Where(e => e.EatType == (EatTypeEnum)eatTyp);
            }
            return await PaginatedList<Eat>.CreateAsync(results, pag, perPag);
        }

        public async Task<List<Eat>> GetAllUserEatsByDateAsync(int userId, DateTime date, DateTime endDate, int eatTyp)
        {
            var results = _uow.EatRepository.GetAll().Where(e => e.UserId == userId && (e.CreatedAt.Date >= date.Date || e.CreatedAt.Date <= endDate.Date))
               .Include(e => e.User)
               .Include(e => e.EatDishes)
                   .ThenInclude(ed => ed.Dish)
                       .ThenInclude(d => d.DishTags)
                           .ThenInclude(dt => dt.Tag)
               .Include(e => e.EatCompoundDishes)
                  .ThenInclude(ed => ed.CompoundDish)
                      .ThenInclude(d => d.DishCompoundDishes)
                          .ThenInclude(dt => dt.Dish)
                            .ThenInclude(dt => dt.DishTags)
                                .ThenInclude(dt => dt.Tag)
               .OrderBy(e => e.CreatedAt).ThenBy(e => e.EatType)
               .AsQueryable();

            //filter by type if not -1(null equivalent)
            if (eatTyp != -1)
            {
                results = results.Where(e => e.EatType == (EatTypeEnum)eatTyp);
            }
            return await results.ToListAsync();
        }

        public async Task<PaginatedList<Eat>> GetPaggeableAllUserEatsAsync(int userId, int pag, int perPag, int eatTyp)
        {
            var results = _uow.EatRepository.GetAll().Where(e => e.UserId == userId)
                .Include(e => e.User)
                .Include(e => e.EatDishes)
                    .ThenInclude(ed => ed.Dish)
                        .ThenInclude(d => d.DishTags)
                            .ThenInclude(dt => dt.Tag)
                .Include(e => e.EatCompoundDishes)
                  .ThenInclude(ed => ed.CompoundDish)
                      .ThenInclude(d => d.DishCompoundDishes)
                          .ThenInclude(dt => dt.Dish)
                            .ThenInclude(dt => dt.DishTags)
                                .ThenInclude(dt => dt.Tag)
                .AsQueryable();

            //filter by type if not -1(null equivalent)
            if (eatTyp != -1)
            {
                results = results.Where(e => e.EatType == (EatTypeEnum)eatTyp);
            }
            return await PaginatedList<Eat>.CreateAsync(results, pag, perPag);
        }

        public async Task<Eat> UpdateEatAsync(int loggedUser, UpdateEatRequest eat)
        {
            var e = await _uow.EatRepository.GetAll().Where(e => e.Id == eat.Id)
                        .Include(e => e.User)
                        .Include(e => e.EatDishes)
                            .ThenInclude(ed => ed.Dish)
                                .ThenInclude(d => d.DishTags)
                                    .ThenInclude(dt => dt.Tag)
                        .Include(e => e.EatCompoundDishes)
                          .ThenInclude(ed => ed.CompoundDish)
                              .ThenInclude(d => d.DishCompoundDishes)
                                  .ThenInclude(dt => dt.Dish)
                                    .ThenInclude(dt => dt.DishTags)
                                        .ThenInclude(dt => dt.Tag)
                        .FirstOrDefaultAsync();

            if (e == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Eat");
            }

            e.EatType = (EatTypeEnum)eat.EatType;
            e.ModifiedAt = DateTime.UtcNow;

            foreach (var ed in e.EatDishes)
            {
                _uow.EatDishRepository.Delete(ed);
            }

            var eatDishes = new List<EatDish>();
            foreach (var ed in eat.Dishes)
            {
                var eatD = new EatDish();
                eatD.DishId = ed.DishId;
                eatD.Qty = ed.Qty;

                eatDishes.Add(eatD);
            }
            e.EatDishes = eatDishes;

            _uow.EatRepository.Update(e);
            await _uow.CommitAsync();
            return e;
        }

        public async Task CreateBulkEatAsync(int loggedUser, CreateBulkEatRequest eat)
        {
            var userEats = await _uow.EatRepository.GetAll().Where(e => e.UserId == loggedUser && e.CreatedAt.Date == eat.DateInUtc.Date)
                .Include(e => e.EatDishes)
                .Include(e => e.EatCompoundDishes)
                .ToListAsync();

            var isNew = true;

            foreach (var d in userEats)
            {
                if (isNew)
                    isNew = false;

                _uow.EatRepository.Delete(d);
            }

            ////this days not eat yet
            //if (userEats.Count == 0)
            //{
            foreach (var item in eat.Eats)
            {
                var e = new Eat();
                e.CreatedAt = eat.DateInUtc;
                e.ModifiedAt = DateTime.UtcNow;
                e.EatType = (EatTypeEnum)item.EatType;
                e.UserId = loggedUser;
                e.IsBalanced = eat.IsBalanced;

                if (isNew && IsValidDateForPlan(eat.DateInUtc, eat.DateTimeInUserLocalTime))
                {
                    e.PlanCreatedAt = eat.DateInUtc;
                    e.IsBalancedPlan = eat.IsBalanced;
                }

                await _uow.EatRepository.AddAsync(e);
                foreach (var d in item.Dishes)
                {
                    var ed = new EatDish();
                    ed.DishId = d.DishId;
                    ed.Eat = e;
                    ed.Qty = d.Qty;

                    await _uow.EatDishRepository.AddAsync(ed);
                }
                foreach (var d in item.CompoundDishes)
                {
                    var ed = new EatCompoundDish();
                    ed.CompoundDishId = d.DishId;
                    ed.Eat = e;
                    ed.Qty = d.Qty;

                    await _uow.EatCompoundDishRepository.AddAsync(ed);
                }
            }
            //}
            ////needs to update or add
            //else
            //{
            //    foreach (var item in eat.Eats)
            //    {
            //        var ue = userEats.Where(u => u.EatType == (EatTypeEnum)item.EatType).FirstOrDefault();
            //        if (ue != null)
            //        {
            //            foreach (var ud in ue.EatDishes)
            //            {
            //                _uow.EatDishRepository.Delete(ud);
            //                //await _uow.CommitAsync();
            //            }

            // foreach (var uf in item.Dishes) { var ed = new EatDish(); ed.DishId = uf.DishId;
            // ed.EatId = ue.Id; ed.Qty = uf.Qty;

            //                await _uow.EatDishRepository.AddAsync(ed);
            //            }
            //        }
            //        ue.ModifiedAt = DateTime.UtcNow;
            //        _uow.EatRepository.Update(ue);
            //        //await _uow.CommitAsync();
            //    }
            //}
            await _uow.CommitAsync();
        }

        public async Task<(double imc, double kcal)> GetKCalImcAsync(int userId, DateTime date)
        {
            try
            {
                var concept = _uow.ConceptRepository.GetAll().Where(c => c.Codename == CodeNamesConstants.HEALTH_MEASURES).FirstOrDefault();
                if (concept != null)
                {
                    var polls = _uow.PollRepository.GetAll().Where(p => p.ConceptId == concept.Id)
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
                        var ua = await _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id && u.CreatedAt.Date <= date.Date)
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
                        var ua = await _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == physicalQuestion.Id && u.CreatedAt.Date <= date.Date)
                                .Include(u => u.Answer)
                                    .ThenInclude(a => a.Question)
                                .OrderByDescending(ua => ua.CreatedAt)
                                .FirstOrDefaultAsync();
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
                        var ua = await _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id && u.CreatedAt.Date <= date.Date)
                            .Include(u => u.Answer)
                                .ThenInclude(a => a.Question)
                            .OrderByDescending(ua => ua.CreatedAt)
                            .FirstOrDefaultAsync();

                        dietSummary += ua.Answer.Weight;
                    }

                    // other values
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

                    return (IMC, dailyKalDouble);
                }
            }
            catch (Exception)
            {
                return (0.0, 0.0);
            }
            return (0.0, 0.0);
        }

        public async Task<bool> AlreadyHavePlanByDateAsync(int userId, DateTime date)
        {
            var eatsCount = await _uow.EatRepository.GetAll().Where(e => e.UserId == userId && e.CreatedAt.Date == date.Date).CountAsync();

            return eatsCount > 0;
        }

        private bool IsValidDateForPlan(DateTime planDateUtc, DateTime? userCurrentLocalTime)
        {
            var today = DateTime.UtcNow.Date;

            if (planDateUtc.Date > today.Date)
                return true;

            if (planDateUtc.Date == today.Date)
            {
                if (userCurrentLocalTime.HasValue && userCurrentLocalTime.Value.Hour <= 9)
                    return true;
            }

            return false;
        }
    }
}