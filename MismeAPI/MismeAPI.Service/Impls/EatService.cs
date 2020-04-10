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

        public async Task<Eat> UpdateEatAsync(int loggedUser, UpdateEatRequest eat)
        {
            var e = await _uow.EatRepository.GetAll().Where(e => e.Id == eat.Id)
                        .Include(e => e.User)
                        .Include(e => e.EatDishes)
                            .ThenInclude(ed => ed.Dish)
                                .ThenInclude(d => d.DishTags)
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
            var userEats = await _uow.EatRepository.GetAll().Where(e => e.UserId == loggedUser && e.CreatedAt.Date == eat.DateInUtc.Date).ToListAsync();

            //this days not eat yet
            if (userEats.Count == 0)
            {
                foreach (var item in eat.Eats)
                {
                    var e = new Eat();
                    e.CreatedAt = DateTime.UtcNow;
                    e.ModifiedAt = DateTime.UtcNow;
                    e.EatType = (EatTypeEnum)item.EatType;
                    e.UserId = loggedUser;
                    await _uow.EatRepository.AddAsync(e);
                    foreach (var d in item.Dishes)
                    {
                        var ed = new EatDish();
                        ed.DishId = d.DishId;
                        ed.Eat = e;
                        ed.Qty = d.Qty;

                        await _uow.EatDishRepository.AddAsync(ed);
                    }
                }
            }
            //needs to update or add
            else
            {
                foreach (var item in eat.Eats)
                {
                    var ue = userEats.Where(u => u.EatType == (EatTypeEnum)item.EatType).FirstOrDefault();
                    if (ue != null)
                    {
                        ue.ModifiedAt = DateTime.UtcNow;
                        foreach (var ud in ue.EatDishes)
                        {
                            _uow.EatDishRepository.Delete(ud);
                        }

                        foreach (var ud in item.Dishes)
                        {
                            var ed = new EatDish();
                            ed.DishId = ud.DishId;
                            ed.EatId = ue.Id;
                            ed.Qty = ud.Qty;

                            await _uow.EatDishRepository.AddAsync(ed);
                        }
                    }
                    _uow.EatRepository.Update(ue);
                }
            }
            await _uow.CommitAsync();
        }
    }
}