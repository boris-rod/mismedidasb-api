using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IReportService
    {
        void GetNutritionalReport();

        Task GetFeedReportAsync();
    }
}