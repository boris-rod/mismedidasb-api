using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
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
    public class GeneralContentService : IGeneralContentService
    {
        private readonly IUnitOfWork _uow;

        public GeneralContentService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task AcceptTermsAndConditionsAsync(int loggedUser)
        {
            var account = await _uow.UserRepository.GetAsync(loggedUser);
            if (account == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            account.TermsAndConditionsAccepted = true;
            _uow.UserRepository.Update(account);
            await _uow.CommitAsync();
        }

        public async Task ChangeContentTranslationAsync(int loggedUser, GeneralContentTranslationRequest contentTranslationRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var content = await _uow.GeneralContentRepository.GetAll().Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            if (content == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Content");
            }

            switch (contentTranslationRequest.Lang)
            {
                case "en":
                    content.ContentEN = contentTranslationRequest.Content;
                    break;

                case "it":
                    content.ContentIT = contentTranslationRequest.Content;
                    break;

                default:
                    content.Content = contentTranslationRequest.Content;
                    break;
            }

            _uow.GeneralContentRepository.Update(content);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<GeneralContent>> GetGeneralContentsAdminAsync(int loggedUser)
        {
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var contents = await _uow.GeneralContentRepository.GetAll().ToListAsync();
            return contents;
        }

        public async Task<GeneralContent> GetGeneralContentsByTypeAsync(int contentType)
        {
            var content = await _uow.GeneralContentRepository.GetAll().Where(c => c.ContentType == (ContentTypeEnum)contentType)
                           .FirstOrDefaultAsync();
            if (content == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Content");
            }

            return content;
        }
    }
}