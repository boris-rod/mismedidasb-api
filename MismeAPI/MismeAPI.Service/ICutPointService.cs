using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.CutPoints;
using MismeAPI.Common.DTO.Request.Dish;
using MismeAPI.Common.DTO.Request.Reward;
using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface ICutPointService
    {
        Task<PaginatedList<CutPoint>> GetCutPointsAsync(int pag, int perPag, string sortOrder, bool? isActive, string search);

        Task<CutPoint> GetCutPointAsync(int id);

        Task<CutPoint> CreateCutPointAsync(CreateCutPointRequest request);

        Task<CutPoint> UpdateCutPointAsync(int id, UpdateCutPointRequest request);

        Task DeleteCutPointAsync(int id);

        Task<IList<CutPoint>> GetNextCutPointsAsync(int points, int qty);
    }
}
