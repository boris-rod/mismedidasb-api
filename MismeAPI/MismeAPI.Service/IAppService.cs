using MismeAPI.Data.Entities;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IAppService
    {
        Task<App> GetAppInfoAsync();
    }
}