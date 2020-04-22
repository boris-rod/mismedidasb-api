using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class SettingService : ISettingService
    {
        private readonly IUnitOfWork _uow;

        public SettingService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<IEnumerable<Setting>> GetSettingsAsync()
        {
            return await _uow.SettingRepository.GetAllAsync();
        }
    }
}