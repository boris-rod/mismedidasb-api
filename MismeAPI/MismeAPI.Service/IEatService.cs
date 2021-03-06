﻿using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IEatService
    {
        Task<PaginatedList<Eat>> GetPaggeableAllUserEatsAsync(int userId, int pag, int perPag, int eatTyp, string sortOrder = "");

        Task<List<Eat>> GetAllUserEatsByDateAsync(int userId, DateTime date, DateTime endDate, int eatTyp);

        Task<PaginatedList<Eat>> GetAdminAllUserEatsAsync(int pag, int perPag, int userId, DateTime? date, int eatTyp);

        Task<Eat> CreateEatAsync(int loggedUser, CreateEatRequest eat);

        Task<Eat> UpdateEatAsync(int loggedUser, UpdateEatRequest eat);

        Task CreateBulkEatFromMenuAsync(int loggedUser, int userId, int menuId, DateTime dateInUtc);

        Task CreateBulkEatAsync(int loggedUser, CreateBulkEatRequest eat);

        Task<(double imc, double kcal)> GetKCalImcAsync(int userId, DateTime date);

        Task<bool> AlreadyHavePlanByDateAsync(int userId, DateTime date);

        Task AddOrUpdateEatAsync(int loggedUser, CreateEatRequest eat);

        Task<IEnumerable<Eat>> GetUserPlanPerDateAsync(int loggedUser, DateTime dateInUtc);

        Task<List<PlanSummaryResponse>> GetPlanSummaryAsync(IEnumerable<Eat> eats);
    }
}
