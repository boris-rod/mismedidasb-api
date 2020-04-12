using CorePush.Google;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common.DTO.Request;
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
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public UserService(IUnitOfWork uow, IConfiguration config)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<PaginatedList<User>> GetUsersAsync(int loggedUser, int pag, int perPag, string sortOrder, int statusFilter, string search)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user.Role == RoleEnum.NORMAL)
            {
                throw new NotAllowedException("User");
            }

            var result = _uow.UserRepository.GetAll()
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
                        result = result.OrderByDescending(i => i.Status.ToString());
                        break;

                    case "status_asc":
                        result = result.OrderBy(i => i.Status.ToString());
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<User>.CreateAsync(result, pag, perPag);
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
    }
}