﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Eat>> GetAllUserEatsByDateAsync(int userId, DateTime date, int eatTyp)
        {
            var results = _uow.EatRepository.GetAll().Where(e => e.UserId == userId && e.CreatedAt.Date == date.Date)
               .Include(e => e.User)
               .Include(e => e.EatDishes)
                   .ThenInclude(ed => ed.Dish)
                       .ThenInclude(d => d.DishTags)
                           .ThenInclude(dt => dt.Tag)
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
                .AsQueryable();

            //filter by type if not -1(null equivalent)
            if (eatTyp != -1)
            {
                results = results.Where(e => e.EatType == (EatTypeEnum)eatTyp);
            }
            return await PaginatedList<Eat>.CreateAsync(results, pag, perPag);
        }
    }
}