using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request.CutPoints;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.CutPoint;
using MismeAPI.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/cut-point")]
    [Authorize]
    public class CutPointController : Controller
    {
        private readonly ICutPointService _cutPointService;
        private readonly IMapper _mapper;

        public CutPointController(ICutPointService cutPointService, IMapper mapper)
        {
            _cutPointService = cutPointService ?? throw new ArgumentNullException(nameof(cutPointService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get active cut points. Require authentication
        /// </summary>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of eats to be displayed per page. 10 by default.</param>
        /// <param name="sortOrder">field_asc or field_desc for ordering</param>
        /// <param name="search">search a text/numer</param>
        /// <returns>Active cut points</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CutPointResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index(int? page, int? perPage, string sortOrder, string search)
        {
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _cutPointService.GetCutPointsAsync(pag, perPag, sortOrder, true, search);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";
            var mapped = _mapper.Map<IEnumerable<CutPointResponse>>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get all cut points. Require admin role
        /// </summary>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of eats to be displayed per page. 10 by default.</param>
        /// <param name="sortOrder">field_asc or field_desc for ordering</param>
        /// <param name="isActive"></param>
        /// <param name="search">search a text/numer</param>
        /// <returns>Active cut points</returns>
        [HttpGet("admin")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(IEnumerable<CutPointResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> IndexAdmin(int? page, int? perPage, string sortOrder, bool? isActive, string search)
        {
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _cutPointService.GetCutPointsAsync(pag, perPag, sortOrder, isActive, search);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";
            var mapped = _mapper.Map<IEnumerable<CutPointResponse>>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get a cut point by id. Requires authentication.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IEnumerable<CutPointResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAll([FromRoute]int id)
        {
            var result = await _cutPointService.GetCutPointAsync(id);
            var mapped = _mapper.Map<IEnumerable<CutPointResponse>>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Add a Cut Point. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="request">Cut Point request object.</param>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(CutPointResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddCutPoint([FromBody]CreateCutPointRequest request)
        {
            var result = await _cutPointService.CreateCutPointAsync(request);
            var mapped = _mapper.Map<CutPointResponse>(result);

            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Edit a CutPoint. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="request">CutPoint request object.</param>
        /// <param name="id">CutPoint id to update.</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(CutPointResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> EditCutPoint([FromRoute]int id, UpdateCutPointRequest request)
        {
            var result = await _cutPointService.UpdateCutPointAsync(id, request);
            var mapped = _mapper.Map<CutPointResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a Cut Point. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Cut Point id to delete.</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteCutPoint([FromRoute]int id)
        {
            await _cutPointService.DeleteCutPointAsync(id);
            return Ok();
        }
    }
}
