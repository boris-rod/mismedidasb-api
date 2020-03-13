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
                    return GetUserStatsFromTodayAsync();

                default:
                    return null;
            }
        }

        private IEnumerable<UsersByDateSeriesResponse> GetUserStatsFromTodayAsync()
        {
            var created = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.PENDING && u.CreatedAt.Date == DateTime.UtcNow.Date);
            var active = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.ACTIVE && u.ActivatedAt.HasValue && u.ActivatedAt.Value.Date == DateTime.UtcNow.Date);
            var disabled = _uow.UserRepository.GetAll().Where(u => u.Status == StatusEnum.INACTIVE && u.DisabledAt.HasValue && u.DisabledAt.Value.Date == DateTime.UtcNow.Date);

            var creGrouped = created.AsEnumerable().GroupBy(c => c.CreatedAt.Hour);
            var actGrouped = active.AsEnumerable().GroupBy(c => c.ActivatedAt?.Hour);
            var disGrouped = disabled.AsEnumerable().GroupBy(c => c.DisabledAt?.Hour);

            var seriesToReturn = new List<UsersByDateSeriesResponse>();

            // active
            var serieResponse = new UsersByDateSeriesResponse();
            serieResponse.Name = "Activo";
            var series = new List<BasicSerieResponse>();
            foreach (var item in actGrouped)
            {
                if (item.Key.HasValue)
                {
                    var simpleSerie = new BasicSerieResponse();
                    simpleSerie.Name = item.Key.Value.ToString();
                    simpleSerie.Value = item.Count();
                    series.Add(simpleSerie);
                }
            }
            serieResponse.Series = series;
            seriesToReturn.Add(serieResponse);
            // end active

            //created
            serieResponse = new UsersByDateSeriesResponse();

            serieResponse.Name = "Por Activar";
            series = new List<BasicSerieResponse>();
            foreach (var item in creGrouped)
            {
                var simpleSerie = new BasicSerieResponse();
                simpleSerie.Name = item.Key.ToString();
                simpleSerie.Value = item.Count();
                series.Add(simpleSerie);
            }
            serieResponse.Series = series;
            seriesToReturn.Add(serieResponse);
            //end created

            //created
            serieResponse = new UsersByDateSeriesResponse();

            serieResponse.Name = "Deshabilitado";
            series = new List<BasicSerieResponse>();
            foreach (var item in disGrouped)
            {
                var simpleSerie = new BasicSerieResponse();
                simpleSerie.Name = item.Key.ToString();
                simpleSerie.Value = item.Count();
                series.Add(simpleSerie);
            }
            serieResponse.Series = series;
            seriesToReturn.Add(serieResponse);
            //end created

            return seriesToReturn;
        }
    }
}