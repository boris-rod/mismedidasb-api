using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/user")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all users, it is allowed to use filtering. Requires authentication. Admin access.
        /// </summary>
        /// <param name="page">Page for pagination purposes.</param>
        /// <param name="perPage">How many issues per page.</param>
        /// <param name="sortOrder">For sortering purposes.</param>
        /// <param name="statusFilter">For filtering for status.</param>
        /// <param name="search">For searching purposes.</param>
        [HttpGet()]
        [ProducesResponseType(typeof(IEnumerable<UserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetUsers(int? page, int? perPage, string sortOrder, string search, int? statusFilter)
        {
            var loggedUser = User.GetUserIdFromToken();
            var pag = page ?? 1;
            var perPag = perPage ?? 10;
            var statusF = statusFilter ?? -1;

            var result = await _userService.GetUsersAsync(loggedUser, pag, perPag, sortOrder, statusF, search);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<UserResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get users stats. Requires authentication. Admin access.
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(IEnumerable<dynamic>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetUsersStats()
        {
            var loggedUser = User.GetUserIdFromToken();

            var result = await _userService.GetUsersStatsAsync(loggedUser);

            return Ok(new ApiOkResponse(result));
        }

        /// <summary>
        /// Get users stats by dates. Requires authentication. Admin access. <param
        /// name="dateType">Date type. 0- Today, 1- Week, 2- Month, 3- Year</param>
        /// </summary>
        [HttpGet("stats-by-date")]
        [ProducesResponseType(typeof(IEnumerable<UsersByDateSeriesResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetUsersStatsByDates([FromQuery] int? dateType)
        {
            var type = dateType ?? 0;
            var loggedUser = User.GetUserIdFromToken();

            var result = await _userService.GetUsersStatsByDateAsync(loggedUser, type);

            return Ok(new ApiOkResponse(result));
        }
    }
}