using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request.Result;
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
    public class ResultService : IResultService
    {
        private readonly IUnitOfWork _uow;

        public ResultService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task ChangeResultTranslationAsync(int loggedUser, ResultTranslationRequest resultTranslationRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var result = await _uow.ResultRepository.GetAll().Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            if (result == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Result");
            }

            switch (resultTranslationRequest.Lang)
            {
                case "en":
                    result.TextEN = resultTranslationRequest.Text;
                    break;

                case "it":
                    result.TextIT = resultTranslationRequest.Text;
                    break;

                default:
                    result.Text = resultTranslationRequest.Text;
                    break;
            }

            _uow.ResultRepository.Update(result);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<Result>> GetResultsAdminAsync(int loggedUser)
        {
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var results = await _uow.ResultRepository.GetAll().ToListAsync();
            return results;
        }
    }
}