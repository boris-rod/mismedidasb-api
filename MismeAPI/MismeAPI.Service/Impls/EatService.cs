using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Service.Utils;
using MismeAPI.Services;
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
        private readonly IScheduleService _scheduleService;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;

        public EatService(IUnitOfWork uow, IScheduleService scheduleService, IUserService userService, IAccountService accountService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
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
            var results = _uow.EatRepository.GetAll().Where(e => e.UserId == userId && (e.CreatedAt.Date >= date.Date && e.CreatedAt.Date <= endDate.Date))
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

        public async Task<PaginatedList<Eat>> GetPaggeableAllUserEatsAsync(int userId, int pag, int perPag, int eatTyp, string sortOrder = "")
        {
            var result = _uow.EatRepository.GetAll().Where(e => e.UserId == userId)
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
                .OrderByDescending(e => e.CreatedAt).ThenBy(e => e.EatType)
                .AsQueryable();

            //filter by type if not -1(null equivalent)
            if (eatTyp != -1)
            {
                result = result.Where(e => e.EatType == (EatTypeEnum)eatTyp);
            }

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "createdAt_desc":
                        result = result.OrderByDescending(i => i.CreatedAt).ThenBy(e => e.EatType);
                        break;

                    case "createdAt_asc":
                        result = result.OrderBy(i => i.CreatedAt).ThenBy(e => e.EatType);
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<Eat>.CreateAsync(result, pag, perPag);
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

        public async Task<IEnumerable<Eat>> GetUserPlanPerDateAsync(int loggedUser, DateTime dateInUtc)
        {
            var userEats = await _uow.EatRepository.GetAll().Where(e => e.UserId == loggedUser && e.CreatedAt.Date == dateInUtc.Date)
                .Include(e => e.EatDishes)
                    .ThenInclude(ed => ed.Dish)
                .Include(e => e.EatCompoundDishes)
                    .ThenInclude(es => es.CompoundDish)
                        .ThenInclude(cd => cd.DishCompoundDishes)
                            .ThenInclude(dcd => dcd.Dish)
                .ToListAsync();

            return userEats;
        }

        public async Task CreateBulkEatAsync(int loggedUser, CreateBulkEatRequest eat)
        {
            var userEats = await _uow.EatRepository.GetAll().Where(e => e.UserId == loggedUser && e.CreatedAt.Date == eat.DateInUtc.Date)
                .Include(e => e.EatDishes)
                .Include(e => e.EatCompoundDishes)
                .Include(e => e.EatSchedule)
                    .ThenInclude(es => es.Schedule)
                .ToListAsync();

            var isNew = true;
            bool? oldPlanBalanced = null;
            DateTime? oldPlanCreatedAt = null;

            foreach (var d in userEats)
            {
                if (isNew)
                {
                    isNew = false;
                    oldPlanBalanced = d.IsBalancedPlan;
                    oldPlanCreatedAt = d.PlanCreatedAt;
                }

                //var jobId = d.EatSchedule?.Schedule?.JobId;
                //if (!String.IsNullOrEmpty(jobId))
                //    await _scheduleService.RemoveJobIfExistIfExistAsync(jobId);

                _uow.EatRepository.Delete(d);
            }
            var eatResult = new List<Eat>();
            ////this days not eat yet
            //if (userEats.Count == 0)
            //{
            var kcal = await _accountService.GetKCalAsync(loggedUser);
            var imc = await _accountService.GetIMCAsync(loggedUser);
            foreach (var item in eat.Eats)
            {
                var e = new Eat();
                e.CreatedAt = eat.DateInUtc;
                e.ModifiedAt = DateTime.UtcNow;
                e.EatType = (EatTypeEnum)item.EatType;
                e.UserId = loggedUser;
                e.IsBalanced = eat.IsBalanced;
                e.EatUtcAt = item.EatUtcAt;
                e.KCalAtThatMoment = kcal;
                e.ImcAtThatMoment = imc;

                if (IsValidDateForPlan(eat.DateInUtc, eat.DateTimeInUserLocalTime))
                {
                    e.PlanCreatedAt = eat.DateInUtc;
                    e.IsBalancedPlan = eat.IsBalanced;
                }
                else
                {
                    e.PlanCreatedAt = oldPlanCreatedAt;
                    e.IsBalancedPlan = oldPlanBalanced;
                }

                await _uow.EatRepository.AddAsync(e);

                eatResult.Add(e);

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

            /*
            var wantNotification = await _userService.GetUserOptInNotificationAsync(loggedUser, SettingsConstants.PREPARE_EAT_REMINDER);
            if (wantNotification)
            {
                foreach (var e in eatResult)
                {
                    if (e.EatUtcAt.HasValue && e.EatUtcAt.Value > DateTime.UtcNow)
                    {
                        var timeReminder = e.EatUtcAt.Value.AddMinutes(-10);
                        var schedule = await _scheduleService.ScheduleEatReminderNotificationAsync(e, timeReminder);
                        e.EatSchedule = new EatSchedule
                        {
                            Schedule = schedule
                        };

                        await _uow.EatRepository.UpdateAsync(e, e.Id);
                    }
                }
                await _uow.CommitAsync();
            }
            API reminder not in use anymore
            */
        }

        public async Task<(double imc, double kcal)> GetKCalImcAsync(int userId, DateTime date)
        {
            var eat = await _uow.EatRepository.GetAll()
                .Where(e => e.UserId == userId && e.CreatedAt.Date == date.Date)
                .FirstOrDefaultAsync();

            var imc = 0.0;
            var kcal = 0.0;

            if (eat != null)
            {
                // kcals and imc stored in the eat in that date
                imc = eat.ImcAtThatMoment;
                kcal = eat.KCalAtThatMoment;
            }
            else
            {
                // there is no plan for that date then we are using the current user imc and kcals
                var user = await _userService.GetUserDevicesAsync(userId);
                imc = user.CurrentImc;
                kcal = user.CurrentKcal;
            }

            return (imc, kcal);
        }

        public async Task<bool> AlreadyHavePlanByDateAsync(int userId, DateTime date)
        {
            var eatsCount = await _uow.EatRepository.GetAll().Where(e => e.UserId == userId && e.CreatedAt.Date == date.Date).CountAsync();

            return eatsCount > 0;
        }

        private bool IsValidDateForPlan(DateTime planDateUtc, DateTime? userCurrentLocalTime)
        {
            var now = DateTime.UtcNow;

            if (planDateUtc.Date > now.Date)
                return true;

            if (planDateUtc.Date == now.Date)
            {
                if (userCurrentLocalTime.HasValue && userCurrentLocalTime.Value.Hour <= 9)
                    return true;
            }

            return false;
        }

        public async Task AddOrUpdateEatAsync(int loggedUser, CreateEatRequest eat)
        {
            if (!eat.EatUtcAt.HasValue)
            {
                throw new InvalidDataException("is required", "EatUtcAt");
            }

            var dateInUtc = eat.EatUtcAt.Value;
            // this assume the user is creating an eat of each type every day
            var eatDb = await _uow.EatRepository.GetAll()
                .Where(e => e.CreatedAt.Date == dateInUtc.Date && e.EatType == (EatTypeEnum)eat.EatType)
                .FirstOrDefaultAsync();
            //this is an update
            if (eatDb != null)
            {
                eatDb.ModifiedAt = DateTime.UtcNow;

                // delete previous related dishes and compund dishes
                var eatCompoundDishes = eatDb.EatCompoundDishes.ToList();
                var eatDishes = eatDb.EatDishes.ToList();
                foreach (var item in eatCompoundDishes)
                {
                    _uow.EatCompoundDishRepository.Delete(item);
                }
                foreach (var item in eatDishes)
                {
                    _uow.EatDishRepository.Delete(item);
                }

                // recreate the related dishes objects
                var eatDishesNew = new List<EatDish>();
                foreach (var ed in eat.Dishes)
                {
                    var eatD = new EatDish();
                    eatD.DishId = ed.DishId;
                    eatD.Qty = ed.Qty;

                    eatDishesNew.Add(eatD);
                }
                eatDb.EatDishes = eatDishesNew;

                var eatCompoundDishesNew = new List<EatCompoundDish>();
                foreach (var ed in eat.CompoundDishes)
                {
                    var eatD = new EatCompoundDish();
                    eatD.CompoundDishId = ed.DishId;
                    eatD.Qty = ed.Qty;
                    eatCompoundDishesNew.Add(eatD);
                }
                eatDb.EatCompoundDishes = eatCompoundDishesNew;
            }
            // this is a create
            else
            {
                var imcKcals = await GetKCalImcAsync(loggedUser, dateInUtc);
                var kcal = imcKcals.kcal;
                var imc = imcKcals.imc;
                var e = new Eat();
                e.EatType = (EatTypeEnum)eat.EatType;
                e.CreatedAt = dateInUtc;
                e.ModifiedAt = DateTime.UtcNow;
                e.UserId = loggedUser;
                e.EatUtcAt = eat.EatUtcAt;
                e.KCalAtThatMoment = kcal;
                e.ImcAtThatMoment = imc;

                var eatDishes = new List<EatDish>();
                foreach (var ed in eat.Dishes)
                {
                    var eatD = new EatDish();
                    eatD.DishId = ed.DishId;
                    eatD.Qty = ed.Qty;

                    eatDishes.Add(eatD);
                }
                e.EatDishes = eatDishes;

                var eatCompoundDishes = new List<EatCompoundDish>();
                foreach (var ed in eat.CompoundDishes)
                {
                    var eatD = new EatCompoundDish();
                    eatD.CompoundDishId = ed.DishId;
                    eatD.Qty = ed.Qty;
                    eatCompoundDishes.Add(eatD);
                }
                e.EatCompoundDishes = eatCompoundDishes;

                await _uow.EatRepository.AddAsync(e);
            }
            await _uow.CommitAsync();

            await SetIsBalancedPlanAync(loggedUser, dateInUtc);
        }

        /// <summary>
        /// Return list of plan summary assuming that only one user's eats are provided
        /// </summary>
        /// <param name="eats">List of eats of an user</param>
        /// <returns></returns>
        public async Task<List<PlanSummaryResponse>> GetPlanSummaryAsync(IEnumerable<Eat> eats)
        {
            var result = new List<PlanSummaryResponse>();
            var anyEat = eats.FirstOrDefault();
            if (anyEat == null)
                return result;

            var user = await _userService.GetUserDevicesAsync(anyEat.UserId);

            foreach (var eat in eats)
            {
                var planDate = eat.CreatedAt;
                if (!result.Any(r => r.PlanDateTime.Date == planDate.Date))
                {
                    var userEats = eats.Where(e => e.CreatedAt.Date == planDate.Date);

                    IHealthyHelper healthyHelper = new HealthyHelper(eat.ImcAtThatMoment, eat.KCalAtThatMoment);
                    var userHealthParameters = healthyHelper.GetUserEatHealtParameters(user);
                    var isBalancedSummary = healthyHelper.IsBalancedPlan(user, userEats);

                    var planSummary = new PlanSummaryResponse
                    {
                        PlanDateTime = planDate,
                        UserEatHealtParameters = userHealthParameters,
                        EatBalancedSummary = isBalancedSummary
                    };

                    result.Add(planSummary);
                }
            }

            return result;
        }

        private async Task SetIsBalancedPlanAync(int loggedUser, DateTime planUtcAt)
        {
            var user = await _userService.GetUserDevicesAsync(loggedUser);
            var userEats = await GetUserPlanPerDateAsync(loggedUser, planUtcAt);
            var imcKcals = await GetKCalImcAsync(loggedUser, planUtcAt);

            bool? oldPlanBalanced = null;
            DateTime? oldPlanCreatedAt = null;

            if (userEats.Count() > 0)
            {
                // Plan Exists
                var d = userEats.FirstOrDefault();

                oldPlanBalanced = d.IsBalancedPlan;
                oldPlanCreatedAt = d.PlanCreatedAt;
            }

            var kcal = imcKcals.kcal;
            var imc = imcKcals.imc;

            IHealthyHelper healthyHelper = new HealthyHelper(imc, kcal);
            var result = healthyHelper.IsBalancedPlan(user, userEats);

            var nowUtc = DateTime.UtcNow;
            var userCurrentDateTime = nowUtc.AddHours(user.TimeZoneOffset);

            foreach (var eat in userEats)
            {
                eat.ModifiedAt = nowUtc;
                eat.IsBalanced = result.IsBalanced;

                if (IsValidDateForPlan(eat.CreatedAt, userCurrentDateTime))
                {
                    eat.PlanCreatedAt = planUtcAt;
                    eat.IsBalancedPlan = result.IsBalanced;
                }
                else
                {
                    eat.PlanCreatedAt = oldPlanCreatedAt;
                    eat.IsBalancedPlan = oldPlanBalanced;
                }

                await _uow.EatRepository.UpdateAsync(eat, eat.Id);
            }

            await _uow.CommitAsync();
        }
    }
}
