using CorePush.Google;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.PersonalData;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public UserService(IUnitOfWork uow, IConfiguration config, IEmailService emailService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<PaginatedList<User>> GetUsersAsync(int loggedUser, int pag, int perPag, string sortOrder, int statusFilter,
            string search, int? minPlannedEats, int? maxPlannedEats, int? minEmotionMedia, int? maxEmotionMedia)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user.Role == RoleEnum.NORMAL)
            {
                throw new NotAllowedException("User");
            }

            var result = _uow.UserRepository.GetAll()
                .Include(u => u.UserStatistics)
                .Include(u => u.UserSoloAnswers)
                .Where(u => u.Role == RoleEnum.NORMAL)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(
                        i => i.FullName.ToLower().Contains(search.ToLower()) ||
                             i.Email.ToLower().Contains(search.ToLower()) ||
                             i.Phone.ToLower().Contains(search.ToLower()));
            }

            // define status filter
            if (statusFilter > -1)
            {
                result = result.Where(i => (StatusEnum)statusFilter == i.Status);
            }

            if (minPlannedEats.HasValue)
            {
                result = result.Where(u => (u.UserStatistics.TotalBalancedEatsPlanned + u.UserStatistics.TotalNonBalancedEatsPlanned) >= minPlannedEats.Value);
            }

            if (maxPlannedEats.HasValue)
            {
                result = result.Where(u => (u.UserStatistics.TotalBalancedEatsPlanned + u.UserStatistics.TotalNonBalancedEatsPlanned) <= maxPlannedEats.Value);
            }

            if (minEmotionMedia.HasValue)
            {
                result = result
                    .Where(u => u.UserSoloAnswers
                        .Where(usa => usa.QuestionCode == "SQ-2" && usa.AnswerCode == "SQ-2-SA-1" && !string.IsNullOrEmpty(usa.AnswerValue))
                        .Average(usa => int.Parse(usa.AnswerValue)) >= minEmotionMedia.Value);
            }

            if (maxEmotionMedia.HasValue)
            {
                result = result
                    .Where(u => u.UserSoloAnswers
                        .Where(usa => usa.QuestionCode == "SQ-2" && usa.AnswerCode == "SQ-2-SA-1" && !string.IsNullOrEmpty(usa.AnswerValue))
                        .Average(usa => int.Parse(usa.AnswerValue)) <= maxEmotionMedia.Value);
            }

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "fullName_desc":
                        result = result.OrderByDescending(i => i.FullName);
                        break;

                    case "fullName_asc":
                        result = result.OrderBy(i => i.FullName);
                        break;

                    case "email_desc":
                        result = result.OrderByDescending(i => i.Email);
                        break;

                    case "email_asc":
                        result = result.OrderBy(i => i.Email);
                        break;

                    case "phone_desc":
                        result = result.OrderByDescending(i => i.Phone);
                        break;

                    case "phone_asc":
                        result = result.OrderBy(i => i.Phone);
                        break;

                    case "status_desc":
                        result = result.OrderByDescending(i => i.Status);
                        break;

                    case "status_asc":
                        result = result.OrderBy(i => i.Status);
                        break;

                    case "emotionMedia_desc":
                        result = result.OrderByDescending(i =>
                            i.UserSoloAnswers.Where(usa => usa.QuestionCode == "SQ-2" && usa.AnswerCode == "SQ-2-SA-1" && !string.IsNullOrEmpty(usa.AnswerValue))
                            .Average(usa => int.Parse(usa.AnswerValue)));
                        break;

                    case "emotionMedia_asc":
                        result = result.OrderBy(i =>
                            i.UserSoloAnswers.Where(usa => usa.QuestionCode == "SQ-2" && usa.AnswerCode == "SQ-2-SA-1" && !string.IsNullOrEmpty(usa.AnswerValue))
                            .Average(usa => int.Parse(usa.AnswerValue)));
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<User>.CreateAsync(result, pag == -1 ? 1 : pag, pag == -1 ? result.Count() : perPag);
        }

        public async Task<dynamic> GetUsersStatsAsync(int loggedUser)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user.Role == RoleEnum.NORMAL)
            {
                throw new NotAllowedException("User");
            }

            var results = await _uow.UserRepository.GetAll().Where(u => u.Id != loggedUser).GroupBy(u => u.Status)
                .Select(g => new
                {
                    name = g.Key == StatusEnum.ACTIVE ? "Activo" :
                        (g.Key == StatusEnum.INACTIVE ? "Deshabilitado" : "Por Activar"),
                    value = g.Count()
                })
                .ToListAsync();

            return results;
        }

        public async Task<IEnumerable<EatsByDateSeriesResponse>> GetEatsStatsByDateAsync(int loggedUser, int type)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user.Role == RoleEnum.NORMAL)
            {
                throw new NotAllowedException("User");
            }
            switch (type)
            {
                case 1:
                    return GetEatsStatsFromToday();

                case 2:
                    return GetEatsStatsFromMonth();

                default:
                    return GetEatsStatsFromYear();
            }
        }

        private IEnumerable<EatsByDateSeriesResponse> GetEatsStatsFromToday()
        {
            var breakfast = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.BREAKFAST && u.CreatedAt.Date == DateTime.UtcNow.Date);
            var snack1 = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.SNACK1 && u.CreatedAt.Date == DateTime.UtcNow.Date);
            var lunch = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.LUNCH && u.CreatedAt.Date == DateTime.UtcNow.Date);
            var snack2 = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.SNACK2 && u.CreatedAt.Date == DateTime.UtcNow.Date);
            var dinner = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.DINNER && u.CreatedAt.Date == DateTime.UtcNow.Date);

            var breakGrouped = breakfast.AsEnumerable().GroupBy(c => c.CreatedAt.Hour);
            var snackGrouped = snack1.AsEnumerable().GroupBy(c => c.CreatedAt.Hour);
            var lunchGrouped = lunch.AsEnumerable().GroupBy(c => c.CreatedAt.Hour);
            var snack2Grouped = snack2.AsEnumerable().GroupBy(c => c.CreatedAt.Hour);
            var dinnerGrouped = dinner.AsEnumerable().GroupBy(c => c.CreatedAt.Hour);

            var seriesToReturn = new List<EatsByDateSeriesResponse>();
            for (int i = 0; i <= 23; i++)
            {
                var serieResponse = new EatsByDateSeriesResponse();
                serieResponse.Name = i.ToString();
                var series = new List<BasicSerieResponse>();

                var bre = breakGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (bre != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Desayuno";
                    simpleSerie.Value = bre.Count();
                    if (bre.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                var sn1 = snackGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (sn1 != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Merienda 1";
                    simpleSerie.Value = sn1.Count();
                    if (sn1.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                var lun = lunchGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (lun != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Almuerzo";
                    simpleSerie.Value = lun.Count();
                    if (lun.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                var sn2 = snack2Grouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (sn2 != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Merienda 2";
                    simpleSerie.Value = sn2.Count();
                    if (sn2.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                var din = dinnerGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (din != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Cena";
                    simpleSerie.Value = din.Count();
                    if (din.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                if (series.Count > 0)
                {
                    serieResponse.Series = series;
                    seriesToReturn.Add(serieResponse);
                }
            }

            return seriesToReturn;
        }

        private IEnumerable<EatsByDateSeriesResponse> GetEatsStatsFromMonth()
        {
            var breakfast = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.BREAKFAST && u.CreatedAt.Date.Month == DateTime.UtcNow.Date.Month);
            var snack1 = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.SNACK1 && u.CreatedAt.Date.Month == DateTime.UtcNow.Date.Month);
            var lunch = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.LUNCH && u.CreatedAt.Date.Month == DateTime.UtcNow.Date.Month);
            var snack2 = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.SNACK2 && u.CreatedAt.Date.Month == DateTime.UtcNow.Date.Month);
            var dinner = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.DINNER && u.CreatedAt.Date.Month == DateTime.UtcNow.Date.Month);

            var breakGrouped = breakfast.AsEnumerable().GroupBy(c => c.CreatedAt.Day);
            var snackGrouped = snack1.AsEnumerable().GroupBy(c => c.CreatedAt.Day);
            var lunchGrouped = lunch.AsEnumerable().GroupBy(c => c.CreatedAt.Day);
            var snack2Grouped = snack2.AsEnumerable().GroupBy(c => c.CreatedAt.Day);
            var dinnerGrouped = dinner.AsEnumerable().GroupBy(c => c.CreatedAt.Day);

            var daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);

            var seriesToReturn = new List<EatsByDateSeriesResponse>();
            for (int i = 1; i <= daysInMonth; i++)
            {
                var serieResponse = new EatsByDateSeriesResponse();
                serieResponse.Name = i.ToString();
                var series = new List<BasicSerieResponse>();

                var bre = breakGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (bre != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Desayuno";
                    simpleSerie.Value = bre.Count();
                    if (bre.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                var sn1 = snackGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (sn1 != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Merienda 1";
                    simpleSerie.Value = sn1.Count();
                    if (sn1.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                var lun = lunchGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (lun != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Almuerzo";
                    simpleSerie.Value = lun.Count();
                    if (lun.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                var sn2 = snack2Grouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (sn2 != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Merienda 2";
                    simpleSerie.Value = sn2.Count();
                    if (sn2.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                var din = dinnerGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (din != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Cena";
                    simpleSerie.Value = din.Count();
                    if (din.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                if (series.Count > 0)
                {
                    serieResponse.Series = series;
                    seriesToReturn.Add(serieResponse);
                }
            }

            return seriesToReturn;
        }

        private IEnumerable<EatsByDateSeriesResponse> GetEatsStatsFromYear()
        {
            var breakfast = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.BREAKFAST && u.CreatedAt.Date.Year == DateTime.UtcNow.Date.Year);
            var snack1 = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.SNACK1 && u.CreatedAt.Date.Year == DateTime.UtcNow.Date.Year);
            var lunch = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.LUNCH && u.CreatedAt.Date.Year == DateTime.UtcNow.Date.Year);
            var snack2 = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.SNACK2 && u.CreatedAt.Date.Year == DateTime.UtcNow.Date.Year);
            var dinner = _uow.EatRepository.GetAll().Where(u => u.EatType == EatTypeEnum.DINNER && u.CreatedAt.Date.Year == DateTime.UtcNow.Date.Year);

            var breakGrouped = breakfast.AsEnumerable().GroupBy(c => c.CreatedAt.Month);
            var snackGrouped = snack1.AsEnumerable().GroupBy(c => c.CreatedAt.Month);
            var lunchGrouped = lunch.AsEnumerable().GroupBy(c => c.CreatedAt.Month);
            var snack2Grouped = snack2.AsEnumerable().GroupBy(c => c.CreatedAt.Month);
            var dinnerGrouped = dinner.AsEnumerable().GroupBy(c => c.CreatedAt.Month);

            var seriesToReturn = new List<EatsByDateSeriesResponse>();
            for (int i = 1; i <= 12; i++)
            {
                var serieResponse = new EatsByDateSeriesResponse();
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i);
                serieResponse.Name = monthName;
                var series = new List<BasicSerieResponse>();

                var bre = breakGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (bre != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Desayuno";
                    simpleSerie.Value = bre.Count();
                    if (bre.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                var sn1 = snackGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (sn1 != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Merienda 1";
                    simpleSerie.Value = sn1.Count();
                    if (sn1.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                var lun = lunchGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (lun != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Almuerzo";
                    simpleSerie.Value = lun.Count();
                    if (lun.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                var sn2 = snack2Grouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (sn2 != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Merienda 2";
                    simpleSerie.Value = sn2.Count();
                    if (sn2.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                var din = dinnerGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (din != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Cena";
                    simpleSerie.Value = din.Count();
                    if (din.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                if (series.Count > 0)
                {
                    serieResponse.Series = series;
                    seriesToReturn.Add(serieResponse);
                }
            }

            return seriesToReturn;
        }

        public async Task<IEnumerable<UsersByDateSeriesResponse>> GetUsersStatsByDateAsync(int loggedUser, int type)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user.Role == RoleEnum.NORMAL)
            {
                throw new NotAllowedException("User");
            }
            switch (type)
            {
                case 1:
                    return GetUserStatsFromToday();

                case 2:
                    return GetUserStatsFromMonth();

                default:
                    return GetUserStatsFromYear();
            }
        }

        private IEnumerable<UsersByDateSeriesResponse> GetUserStatsFromToday()
        {
            var created = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.PENDING && u.CreatedAt.Date == DateTime.UtcNow.Date);
            var active = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.ACTIVE && u.ActivatedAt.HasValue && u.ActivatedAt.Value.Date == DateTime.UtcNow.Date);
            var disabled = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.INACTIVE && u.DisabledAt.HasValue && u.DisabledAt.Value.Date == DateTime.UtcNow.Date);

            var creGrouped = created.AsEnumerable().GroupBy(c => c.CreatedAt.Hour);
            var actGrouped = active.AsEnumerable().GroupBy(c => c.ActivatedAt?.Hour);
            var disGrouped = disabled.AsEnumerable().GroupBy(c => c.DisabledAt?.Hour);

            var seriesToReturn = new List<UsersByDateSeriesResponse>();
            for (int i = 0; i <= 23; i++)
            {
                var serieResponse = new UsersByDateSeriesResponse();
                serieResponse.Name = i.ToString();
                var series = new List<BasicSerieResponse>();

                var act = actGrouped.Where(ag => ag.Key.HasValue && ag.Key.Value == i).FirstOrDefault();
                if (act != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Activo";
                    simpleSerie.Value = act.Count();
                    if (act.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                var pend = creGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (pend != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Por Activar";
                    simpleSerie.Value = pend.Count();
                    if (pend.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                var dis = disGrouped.Where(ag => ag.Key.HasValue && ag.Key.Value == i).FirstOrDefault();
                if (dis != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Deshabilitado";
                    simpleSerie.Value = dis.Count();
                    if (dis.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                if (series.Count > 0)
                {
                    serieResponse.Series = series;
                    seriesToReturn.Add(serieResponse);
                }
            }

            return seriesToReturn;
        }

        private IEnumerable<UsersByDateSeriesResponse> GetUserStatsFromMonth()
        {
            var created = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.PENDING && u.CreatedAt.Date.Month == DateTime.UtcNow.Date.Month);
            var active = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.ACTIVE && u.ActivatedAt.HasValue && u.ActivatedAt.Value.Date.Month == DateTime.UtcNow.Date.Month);
            var disabled = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.INACTIVE && u.DisabledAt.HasValue && u.DisabledAt.Value.Date.Month == DateTime.UtcNow.Date.Month);

            var creGrouped = created.AsEnumerable().GroupBy(c => c.CreatedAt.Day);
            var actGrouped = active.AsEnumerable().GroupBy(c => c.ActivatedAt?.Day);
            var disGrouped = disabled.AsEnumerable().GroupBy(c => c.DisabledAt?.Day);

            var daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);

            var seriesToReturn = new List<UsersByDateSeriesResponse>();
            for (int i = 1; i <= daysInMonth; i++)
            {
                var serieResponse = new UsersByDateSeriesResponse();
                serieResponse.Name = i.ToString();
                var series = new List<BasicSerieResponse>();

                var act = actGrouped.Where(ag => ag.Key.HasValue && ag.Key.Value == i).FirstOrDefault();
                if (act != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Activo";
                    simpleSerie.Value = act.Count();
                    if (act.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                var pend = creGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (pend != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Por Activar";
                    simpleSerie.Value = pend.Count();
                    if (pend.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                var dis = disGrouped.Where(ag => ag.Key.HasValue && ag.Key.Value == i).FirstOrDefault();
                if (dis != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Deshabilitado";
                    simpleSerie.Value = dis.Count();
                    if (dis.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                if (series.Count > 0)
                {
                    serieResponse.Series = series;
                    seriesToReturn.Add(serieResponse);
                }
            }

            return seriesToReturn;
        }

        private IEnumerable<UsersByDateSeriesResponse> GetUserStatsFromYear()
        {
            var created = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.PENDING && u.CreatedAt.Date.Year == DateTime.UtcNow.Date.Year);
            var active = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.ACTIVE && u.ActivatedAt.HasValue && u.ActivatedAt.Value.Date.Year == DateTime.UtcNow.Date.Year);
            var disabled = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.INACTIVE && u.DisabledAt.HasValue && u.DisabledAt.Value.Date.Year == DateTime.UtcNow.Date.Year);

            var creGrouped = created.AsEnumerable().GroupBy(c => c.CreatedAt.Month);
            var actGrouped = active.AsEnumerable().GroupBy(c => c.ActivatedAt?.Month);
            var disGrouped = disabled.AsEnumerable().GroupBy(c => c.DisabledAt?.Month);

            var seriesToReturn = new List<UsersByDateSeriesResponse>();
            for (int i = 1; i <= 12; i++)
            {
                var serieResponse = new UsersByDateSeriesResponse();
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i);
                serieResponse.Name = monthName;
                var series = new List<BasicSerieResponse>();

                var act = actGrouped.Where(ag => ag.Key.HasValue && ag.Key.Value == i).FirstOrDefault();
                if (act != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Activo";
                    simpleSerie.Value = act.Count();
                    if (act.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                var pend = creGrouped.Where(ag => ag.Key == i).FirstOrDefault();
                if (pend != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Por Activar";
                    simpleSerie.Value = pend.Count();
                    if (pend.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }

                var dis = disGrouped.Where(ag => ag.Key.HasValue && ag.Key.Value == i).FirstOrDefault();
                if (dis != null)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = "Deshabilitado";
                    simpleSerie.Value = dis.Count();
                    if (dis.Count() > 0)
                    {
                        series.Add(simpleSerie);
                    }
                }
                if (series.Count > 0)
                {
                    serieResponse.Series = series;
                    seriesToReturn.Add(serieResponse);
                }
            }

            return seriesToReturn;
        }

        public async Task<User> EnableUserAsync(int loggedUser, int id)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user.Role == RoleEnum.NORMAL)
            {
                throw new NotAllowedException("User");
            }
            var userToUpdate = await _uow.UserRepository.GetAsync(id);
            if (userToUpdate == null)
            {
                throw new NotFoundException("User");
            }
            userToUpdate.Status = StatusEnum.ACTIVE;
            userToUpdate.DisabledAt = null;
            _uow.UserRepository.Update(userToUpdate);
            await _uow.CommitAsync();
            return user;
        }

        public async Task<User> DisableUserAsync(int loggedUser, int id)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user.Role == RoleEnum.NORMAL)
            {
                throw new NotAllowedException("User");
            }
            var userToUpdate = await _uow.UserRepository.GetAsync(id);
            if (userToUpdate == null)
            {
                throw new NotFoundException("User");
            }
            userToUpdate.Status = StatusEnum.INACTIVE;
            userToUpdate.DisabledAt = DateTime.UtcNow;
            _uow.UserRepository.Update(userToUpdate);
            await _uow.CommitAsync();
            return user;
        }

        public async Task SendUserNotificationAsync(int loggedUser, int id, UserNotificationRequest notif)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user.Role == RoleEnum.NORMAL)
            {
                throw new NotAllowedException("User");
            }
            var userToUpdate = await _uow.UserRepository.GetAll()
                .Include(u => u.Devices)
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
            if (userToUpdate == null)
            {
                throw new NotFoundException("User");
            }
            var serverKey = _config["Firebase:ServerKey"];
            var senderId = _config["Firebase:SenderId"];

            foreach (var dev in userToUpdate.Devices)
            {
                using (var fcm = new FcmSender(serverKey, senderId))
                {
                    Message message = new Message()
                    {
                        Notification = new Notification
                        {
                            Title = notif.Title,
                            Body = notif.Body
                        },
                        Token = dev.Token
                    };
                    try
                    {
                        var response = await fcm.SendAsync(dev.Token, message);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public async Task<string> GetUserLanguageFromUserIdAsync(int loggedUser)
        {
            var setting = await _uow.SettingRepository.GetAll().Where(s => s.Name == SettingsConstants.LANGUAGE).FirstOrDefaultAsync();
            if (setting != null)
            {
                var us = await _uow.UserSettingRepository.GetAll().Where(us => us.SettingId == setting.Id && us.UserId == loggedUser).FirstOrDefaultAsync();
                if (us != null)
                {
                    return string.IsNullOrWhiteSpace(us.Value) ? "ES" : us.Value;
                }
            }
            return "ES";
        }

        public async Task<int> GetEatsCountAsync(int loggedUser)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user.Role == RoleEnum.NORMAL)
            {
                throw new NotAllowedException("User");
            }

            return await _uow.EatRepository.CountAsync();
        }

        /// <summary>
        /// PlanCreatedAt is in UTC so make sure that today is in the user's current timezone
        /// </summary>
        /// <param name="date"></param>
        /// <returns>Users with plans</returns>
        public async Task<IEnumerable<User>> GetUsersWithPlanAsync(DateTime date)
        {
            var usersWithPlans = await _uow.UserRepository.GetAll()
                .Include(u => u.UserStatistics)
                .Include(u => u.Eats)
                .Include(u => u.Devices)
                .Include(u => u.UserSettings)
                    .ThenInclude(s => s.Setting)
                .Where(u => u.Eats.Any(e => e.PlanCreatedAt.HasValue && e.PlanCreatedAt.Value.Date == date.Date))
                .ToListAsync();

            return usersWithPlans;
        }

        /// <summary>
        /// PlanCreatedAt is in UTC so make sure that today is in the user's current timezone
        /// </summary>
        /// <param name="date"></param>
        /// <returns>Users that have no plans</returns>
        public async Task<IEnumerable<User>> GetUsersWithoutPlanAsync(DateTime date)
        {
            var usersWithoutPlans = await _uow.UserRepository.GetAll()
                .Include(u => u.UserStatistics)
                .Include(u => u.Eats)
                .Include(u => u.Devices)
                .Include(u => u.UserSettings)
                    .ThenInclude(s => s.Setting)
                .Where(u => !u.Eats.Any(e => e.PlanCreatedAt.HasValue && e.PlanCreatedAt.Value.Date == date.Date))
                .ToListAsync();

            return usersWithoutPlans;
        }

        public async Task<User> GetUserDevicesAsync(int userId)
        {
            var user = await _uow.UserRepository.GetAll()
                .Include(u => u.Devices)
                .Include(u => u.Group)
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            var user = await _uow.UserRepository.GetAll()
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.Subscription)
                .Include(u => u.Devices)
                .Include(u => u.UserSettings)
                .Include(u => u.UserStatistics)
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new NotFoundException("User");

            return user;
        }

        public async Task<bool> GetUserOptInNotificationAsync(int userId, string settingConstant)
        {
            if (settingConstant != SettingsConstants.PREPARE_EAT_REMINDER && settingConstant != SettingsConstants.DRINK_WATER_REMINDER)
                return false;

            var setting = await _uow.SettingRepository.GetAll().Where(s => s.Name == settingConstant).FirstOrDefaultAsync();
            if (setting != null)
            {
                var us = await _uow.UserSettingRepository.GetAll().Where(us => us.SettingId == setting.Id && us.UserId == userId).FirstOrDefaultAsync();
                if (us != null && !string.IsNullOrWhiteSpace(us.Value))
                {
                    return us.Value == "true";
                }
            }
            return false;
        }

        public async Task SendManualEmailAsync(SendEmailRequest request)
        {
            if (request.UserIds.Count() < 1)
            {
                throw new InvalidDataException("Users");
            }

            var query = _uow.UserRepository.GetAll()
                .Include(u => u.Devices)
                .Include(u => u.UserSettings)
                .AsQueryable();

            if (request.UserIds.FirstOrDefault() != -1)
                query = query.Where(u => request.UserIds.Contains(u.Id));

            var users = await query.ToListAsync();

            var setting = await _uow.SettingRepository.GetAll().Where(s => s.Name == SettingsConstants.LANGUAGE).FirstOrDefaultAsync();
            var userES = new List<User>();
            var userEN = new List<User>();
            var userIT = new List<User>();

            if (setting != null)
            {
                foreach (var user in users)
                {
                    var lang = "ES";
                    if (user.UserSettings != null)
                    {
                        var userSetting = user.UserSettings.FirstOrDefault(us => us.SettingId == setting.Id);
                        if (userSetting != null && !string.IsNullOrWhiteSpace(userSetting.Value))
                        {
                            lang = userSetting.Value;
                        }
                    }

                    switch (lang)
                    {
                        case "ES":
                            userES.Add(user);
                            break;

                        case "EN":
                            userEN.Add(user);
                            break;

                        case "IT":
                            userIT.Add(user);
                            break;

                        default:
                            break;
                    }
                }
            }
            else
            {
                userES.AddRange(users);
            }

            IEnumerable<string> emails;

            if (string.IsNullOrEmpty(request.SubjectEN) && string.IsNullOrEmpty(request.SubjectIT))
            {
                emails = users.Select(u => u.Email);
                await _emailService.SendEmailResponseAsync(request.Subject, request.Body, emails);
            }
            else
            {
                if (!string.IsNullOrEmpty(request.SubjectEN) && userEN.Count() > 0)
                {
                    emails = userEN.Select(u => u.Email);
                    await _emailService.SendEmailResponseAsync(request.SubjectEN, request.BodyEN, emails);
                }

                if (!string.IsNullOrEmpty(request.SubjectIT) && userIT.Count() > 0)
                {
                    emails = userIT.Select(u => u.Email);
                    await _emailService.SendEmailResponseAsync(request.SubjectIT, request.BodyIT, emails);
                }

                if (userES.Count() > 0)
                {
                    emails = userES.Select(u => u.Email);
                    await _emailService.SendEmailResponseAsync(request.Subject, request.Body, emails);
                }
            }
        }

        public async Task SetUserLatestAccessAsync(int userId)
        {
            var user = await _uow.UserRepository.GetAll().FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                user.LastAccessAt = DateTime.UtcNow;

                await _uow.UserRepository.UpdateAsync(user, userId);
                await _uow.CommitAsync();
            }
        }

        public async Task<UserDataSummary> GetUsersSummaryAsync()
        {
            var result = new UserDataSummary();

            var ageRange1 = GetUserByAgeRangeQuery(18, 24);
            var ageRange2 = GetUserByAgeRangeQuery(25, 34);
            var ageRange3 = GetUserByAgeRangeQuery(35, 44);
            var ageRange4 = GetUserByAgeRangeQuery(45, 54);
            var ageRange5 = GetUserByAgeRangeQuery(55, 64);
            var ageRange6 = GetUserByAgeRangeQuery(65, null);

            var countAgeRange1 = await ageRange1.CountAsync();
            var countAgeRange2 = await ageRange2.CountAsync();
            var countAgeRange3 = await ageRange3.CountAsync();
            var countAgeRange4 = await ageRange4.CountAsync();
            var countAgeRange5 = await ageRange5.CountAsync();
            var countAgeRange6 = await ageRange6.CountAsync();

            result.TotalActiveUsers = await _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.ACTIVE).CountAsync();
            result.TotalUsers = await _uow.UserRepository.GetAll().CountAsync();

            result.AgeRange = new AgeRanges
            {
                CountRange18To24 = countAgeRange1,
                CountRange25To34 = countAgeRange2,
                CountRange35To44 = countAgeRange3,
                CountRange45To54 = countAgeRange4,
                CountRange55To64 = countAgeRange5,
                CountRangeMin65 = countAgeRange6,
                PercentageRange18To24 = GetPorciento(result.TotalActiveUsers, countAgeRange1),
                PercentageRange25To34 = GetPorciento(result.TotalActiveUsers, countAgeRange2),
                PercentageRange35To44 = GetPorciento(result.TotalActiveUsers, countAgeRange3),
                PercentageRange45To54 = GetPorciento(result.TotalActiveUsers, countAgeRange4),
                PercentageRange55To64 = GetPorciento(result.TotalActiveUsers, countAgeRange5),
                PercentageRangeMin65 = GetPorciento(result.TotalActiveUsers, countAgeRange6),
            };

            return result;
        }

        private IQueryable<User> GetUserByAgeRangeQuery(int? minAge, int? maxAge)
        {
            var query = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.ACTIVE).AsQueryable();

            if (minAge.HasValue)
            {
                query = query.Where(u => u.Age >= minAge.Value);
            }

            if (maxAge.HasValue)
            {
                query = query.Where(u => u.Age <= maxAge.Value);
            }

            return query;
        }

        private int GetPorciento(int total, int part)
        {
            return part * 100 / total;
        }
    }
}
