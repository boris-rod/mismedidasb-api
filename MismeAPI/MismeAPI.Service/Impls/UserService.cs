using Microsoft.EntityFrameworkCore;
using MismeAPI.Common.DTO.Response;
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
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;

        public UserService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
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
            var results = await _uow.UserRepository.GetAll().GroupBy(u => u.Status)
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
                case 0:
                    return GetUserStatsFromToday();

                default:
                    return null;
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
    }
}