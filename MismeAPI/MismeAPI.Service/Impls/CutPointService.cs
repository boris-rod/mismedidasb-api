using CorePush.Google;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.CutPoints;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class CutPointService : ICutPointService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public CutPointService(IUnitOfWork uow, IConfiguration config)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<PaginatedList<CutPoint>> GetCutPointsAsync(int pag, int perPag, string sortOrder, bool? isActive, string search)
        {
            var result = _uow.CutPointRepository.GetAll().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(i => i.Description.ToLower().Contains(search.ToLower()) || i.Points.ToString().Contains(search));
            }

            // define status filter
            if (isActive.HasValue)
            {
                result = result.Where(i => i.IsActive == isActive.Value);
            }

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "points_desc":
                        result = result.OrderByDescending(i => i.Points);
                        break;

                    case "points_asc":
                        result = result.OrderBy(i => i.Points);
                        break;

                    case "isActive_desc":
                        result = result.OrderByDescending(i => i.IsActive);
                        break;

                    case "isActive_asc":
                        result = result.OrderBy(i => i.IsActive);
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<CutPoint>.CreateAsync(result, pag, perPag);
        }

        public async Task<CutPoint> GetCutPointAsync(int id)
        {
            var cutPoint = await _uow.CutPointRepository.GetAsync(id);
            if (cutPoint == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Cut point");

            return cutPoint;
        }

        public async Task<CutPoint> CreateCutPointAsync(CreateCutPointRequest request)
        {
            var cutPoint = await _uow.CutPointRepository.FindAsync(c => c.Points == request.Points);
            if (cutPoint != null)
                throw new AlreadyExistsException("Cut point already exists");

            cutPoint = new CutPoint
            {
                Points = request.Points,
                Description = request.Description,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            await _uow.CutPointRepository.AddAsync(cutPoint);
            await _uow.CommitAsync();

            return cutPoint;
        }

        public async Task<CutPoint> UpdateCutPointAsync(int id, UpdateCutPointRequest request)
        {
            var cutPoint = await GetCutPointAsync(id);

            var alreadyExist = await _uow.CutPointRepository.FindAsync(c => c.Points == request.Points && c.Id != id);
            if (alreadyExist != null)
                throw new AlreadyExistsException("Cut point already exists");

            cutPoint.Points = request.Points;
            cutPoint.Description = request.Description;
            cutPoint.IsActive = request.IsActive;
            cutPoint.ModifiedAt = DateTime.UtcNow;

            await _uow.CutPointRepository.UpdateAsync(cutPoint, id);
            await _uow.CommitAsync();

            return cutPoint;
        }

        public async Task DeleteCutPointAsync(int id)
        {
            var cutPoint = await GetCutPointAsync(id);

            _uow.CutPointRepository.Delete(cutPoint);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<CutPoint>> GetNextCutPointsAsync(int points, int qty)
        {
            var result = await _uow.CutPointRepository.GetAll()
                .Where(c => c.Points > points)
                .OrderBy(c => c.Points)
                .Take(qty)
                .ToListAsync();

            return result;
        }
    }
}
