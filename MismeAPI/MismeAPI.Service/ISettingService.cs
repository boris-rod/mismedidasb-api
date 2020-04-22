using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface ISettingService
    {
        Task<IEnumerable<Setting>> GetSettingsAsync();
    }
}