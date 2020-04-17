using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request.Tip;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class TipService : ITipService
    {
        private readonly IUnitOfWork _uow;

        public TipService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task ChangeTipTranslationAsync(int loggedUser, TipTranslationRequest tipTranslationRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var tip = await _uow.TipRepository.GetAll().Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            if (tip == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Tip");
            }

            switch (tipTranslationRequest.Lang)
            {
                case "en":
                    tip.ContentEN = tipTranslationRequest.Content;
                    break;

                case "it":
                    tip.ContentIT = tipTranslationRequest.Content;
                    break;

                default:
                    tip.Content = tipTranslationRequest.Content;
                    break;
            }

            _uow.TipRepository.Update(tip);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<Tip>> GetTipsAdminAsync(int loggedUser)
        {
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var tips = await _uow.TipRepository.GetAll().Include(t => t.Poll).ToListAsync();
            return tips;
        }
    }
}