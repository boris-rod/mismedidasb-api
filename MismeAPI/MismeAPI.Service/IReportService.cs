using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IReportService
    {
        Task GetNutritionalReport(int userId);

        Task GetFeedReportAsync();
    }
}