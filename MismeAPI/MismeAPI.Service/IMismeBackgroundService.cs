using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IMismeBackgroundService
    {
        Task CleanExpiredTokensAsync();
    }
}