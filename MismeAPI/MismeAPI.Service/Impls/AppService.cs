using Microsoft.EntityFrameworkCore;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using System;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class AppService : IAppService
    {
        private readonly IUnitOfWork _uow;

        public AppService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<App> GetAppInfoAsync()
        {
            return await _uow.AppRepository.GetAll().FirstOrDefaultAsync();
        }
    }
}