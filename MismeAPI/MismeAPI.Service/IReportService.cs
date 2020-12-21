using Hangfire;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IReportService
    {
        Task GetNutritionalReportAsync(int userId);

        Task GetFeedReportAsync(int userId);

        Task SendReportsAsync();
    }
}
