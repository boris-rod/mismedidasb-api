using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Menu;
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
    public class MenuService : IMenuService
    {
        private readonly IUnitOfWork _uow;

        public MenuService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<PaginatedList<Menu>> GetMenuesAsync(int? groupId, int pag, int perPag, bool? active, int currentUser, string sortOrder)
        {
            var result = _uow.MenuRepository.GetAll()
              .Include(e => e.Group)
              .AsQueryable();

            if (groupId.HasValue)
            {
                result = result.Where(e => e.GroupId.HasValue && e.GroupId.Value == groupId.Value);
            }
            else
            {
                var user = await _uow.UserRepository.GetAll()
                    .Include(u => u.Group)
                    .Where(u => u.Id == currentUser)
                    .FirstOrDefaultAsync();
                if (user == null)
                    throw new NotFoundException("User");

                if (user.Role != RoleEnum.ADMIN)
                {
                    if (user.GroupId.HasValue)
                        result = result.Where(e => !e.GroupId.HasValue || (e.GroupId.HasValue && e.GroupId.Value == user.GroupId.Value));
                    else
                        result = result.Where(e => !e.GroupId.HasValue);
                }
            }

            //filter by type if not -1(null equivalent)
            if (active.HasValue)
            {
                result = result.Where(e => e.Active == active.Value);
            }

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "name_desc":
                        result = result.OrderByDescending(i => i.Name);
                        break;

                    case "name_asc":
                        result = result.OrderBy(i => i.Name);
                        break;

                    case "description_desc":
                        result = result.OrderByDescending(i => i.Description);
                        break;

                    case "description_asc":
                        result = result.OrderBy(i => i.Description);
                        break;

                    case "createdAt_desc":
                        result = result.OrderByDescending(i => i.CreatedAt);
                        break;

                    case "createdAt_asc":
                        result = result.OrderBy(i => i.CreatedAt);
                        break;

                    case "active_desc":
                        result = result.OrderByDescending(i => i.Active);
                        break;

                    case "active_asc":
                        result = result.OrderBy(i => i.Active);
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<Menu>.CreateAsync(result, pag, perPag);
        }

        public async Task<Menu> GetMenuAsync(int menuId)
        {
            var menu = await _uow.MenuRepository.GetAll().Where(e => e.Id == menuId)
                .FirstOrDefaultAsync();

            if (menu == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Menu");

            return menu;
        }

        public async Task<Menu> GetMenuWithEatsAsync(int menuId)
        {
            var menu = await _uow.MenuRepository.GetAll().Where(e => e.Id == menuId)
                .Include(m => m.Group)
                .Include(m => m.Eats)
                    .ThenInclude(e => e.EatDishes)
                      .ThenInclude(d => d.Dish)
                .Include(m => m.Eats)
                    .ThenInclude(e => e.EatCompoundDishes)
                      .ThenInclude(d => d.CompoundDish)
                .FirstOrDefaultAsync();

            if (menu == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Menu");

            return menu;
        }

        public async Task<Menu> CreateMenuAsync(CreateUpdateMenuRequest menuRequest, int loggedUser)
        {
            if (menuRequest.GroupId.HasValue)
            {
                var canCreate = await MenuBelongsToUserGroupAsync(loggedUser, menuRequest.GroupId.Value);
                if (!canCreate)
                    throw new ForbiddenException("No tiene permiso para crear menues en este grupo.");
            }

            var menu = new Menu();

            menu.Name = menuRequest.Name;
            menu.NameIT = menuRequest.NameIT;
            menu.NameEN = menuRequest.NameEN;
            menu.Description = menuRequest.Description;
            menu.DescriptionIT = menuRequest.DescriptionIT;
            menu.DescriptionEN = menuRequest.DescriptionEN;
            menu.GroupId = menuRequest.GroupId;
            menu.CreatedById = loggedUser;
            menu.ModifiedAt = DateTime.UtcNow;
            menu.CreatedAt = DateTime.UtcNow;
            menu.Active = false;

            await _uow.MenuRepository.AddAsync(menu);
            await _uow.CommitAsync();

            return menu;
        }

        public async Task<Menu> PatchMenuStatusAsync(int menuId, bool activeStatus)
        {
            var menu = await GetMenuAsync(menuId);

            menu.Active = activeStatus;
            menu.ModifiedAt = DateTime.UtcNow;

            await _uow.MenuRepository.UpdateAsync(menu, menuId);
            await _uow.CommitAsync();

            return menu;
        }

        public async Task<Menu> UpdateMenuAsync(int menuId, CreateUpdateMenuRequest menuRequest)
        {
            var menu = await GetMenuAsync(menuId);

            menu.Name = menuRequest.Name;
            menu.NameIT = menuRequest.NameIT;
            menu.NameEN = menuRequest.NameEN;
            menu.Description = menuRequest.Description;
            menu.DescriptionIT = menuRequest.DescriptionIT;
            menu.DescriptionEN = menuRequest.DescriptionEN;
            menu.ModifiedAt = DateTime.UtcNow;

            await _uow.MenuRepository.UpdateAsync(menu, menuId);
            await _uow.CommitAsync();

            return menu;
        }

        public async Task<Menu> UpdateBulkMenuEatsAsync(int menuId, MenuBulkEatsUpdateRequest menuRequest)
        {
            var menu = await GetMenuWithEatsAsync(menuId);

            foreach (var d in menu.Eats)
            {
                _uow.MenuEatRepository.Delete(d);
            }

            var eatResult = new List<Eat>();

            foreach (var item in menuRequest.Eats)
            {
                var e = new MenuEat();
                e.CreatedAt = DateTime.UtcNow;
                e.ModifiedAt = DateTime.UtcNow;
                e.EatType = (EatTypeEnum)item.EatType;
                e.MenuId = menu.Id;

                await _uow.MenuEatRepository.AddAsync(e);

                foreach (var d in item.Dishes)
                {
                    var count = await _uow.DishRepository.GetAll().CountAsync(repo => repo.Id == d.DishId);
                    if (count > 0)
                    {
                        var ed = new MenuEatDish();
                        ed.DishId = d.DishId;
                        ed.MenuEat = e;
                        ed.Qty = d.Qty;
                        await _uow.MenuEatDishRepository.AddAsync(ed);
                    }
                }
                foreach (var d in item.CompoundDishes)
                {
                    var count = await _uow.CompoundDishRepository.GetAll().CountAsync(repo => repo.Id == d.DishId);
                    if (count > 0)
                    {
                        var ed = new MenuEatCompoundDish();
                        ed.CompoundDishId = d.DishId;
                        ed.MenuEat = e;
                        ed.Qty = d.Qty;

                        await _uow.MenuEatCompoundDishRepository.AddAsync(ed);
                    }
                }
            }

            await _uow.CommitAsync();
            return menu;
        }

        public async Task DeleteAsync(int menuId)
        {
            var menu = await GetMenuAsync(menuId);

            _uow.MenuRepository.Delete(menu);
            await _uow.CommitAsync();
        }

        private async Task<bool> MenuBelongsToUserGroupAsync(int userId, int groupId)
        {
            var user = await _uow.UserRepository.GetAll()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new NotFoundException("User");
            }

            var group = await _uow.GroupRepository.GetAll()
                .FirstOrDefaultAsync(u => u.Id == groupId);

            if (group == null)
            {
                throw new NotFoundException("Group");
            }

            if (user.Role != RoleEnum.ADMIN && !user.GroupId.HasValue)
                return false;

            return user.Role == RoleEnum.ADMIN || user.GroupId.Value == groupId;
        }
    }
}
