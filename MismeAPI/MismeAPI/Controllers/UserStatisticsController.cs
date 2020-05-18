using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.UserStatistics;
using MismeAPI.Service;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/user-statistics")]
    [Authorize]
    public class UserStatisticsController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IUserStatisticsService _userStatisticsService;

        public UserStatisticsController(IUserService userService, IMapper mapper, IUserStatisticsService userStatisticsService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userStatisticsService = userStatisticsService ?? throw new ArgumentNullException(nameof(userStatisticsService));
        }

        /// <summary>
        /// Get all user statistics, it is allowed to use filtering. Requires authentication.
        /// </summary>
        /// <param name="page">Page for pagination purposes.</param>
        /// <param name="perPage">How many issues per page.</param>
        /// <param name="sortOrder">For sortering purposes.</param>
        [HttpGet()]
        [ProducesResponseType(typeof(IEnumerable<UserStatisticsResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetUserStatistics(int? page, int? perPage, string sortOrder)
        {
            var loggedUser = User.GetUserIdFromToken();
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _userStatisticsService.GetUserStatisticsAsync(loggedUser, pag, perPag, sortOrder);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<UserStatisticsResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get users statistics by user id. Requires authentication
        /// </summary>
        [HttpGet("by-user-id")]
        [ProducesResponseType(typeof(IEnumerable<UserStatisticsResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetUsersStatisticsByUserId(int userId)
        {
            var result = await _userStatisticsService.GetUserStatisticsByUserAsync(userId);

            var mapped = _mapper.Map<UserStatisticsResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }
    }
}
