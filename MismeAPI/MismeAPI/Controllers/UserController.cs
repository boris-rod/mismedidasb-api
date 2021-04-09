using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.User;
using MismeAPI.Middlewares.Security;
using MismeAPI.Service;
using MismeAPI.Service.Utils;
using MismeAPI.Services;
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
        private readonly IEatService _eatService;
        private readonly IAccountService _accountService;
        private readonly IDishService _dishService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IGroupService _groupService;

        public UserController(IUserService userService, IMapper mapper, IEatService eatService, IAccountService accountService,
            IDishService dishService, IAuthorizationService authorizationService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _eatService = eatService ?? throw new ArgumentNullException(nameof(eatService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        /// <summary>
        /// Get all users, it is allowed to use filtering. Requires authentication. Admin access.
        /// </summary>
        /// <param name="page">Page for pagination purposes.</param>
        /// <param name="perPage">How many issues per page.</param>
        /// <param name="sortOrder">For sortering purposes.</param>
        /// <param name="statusFilter">For filtering for status.</param>
        /// <param name="minPlannedEats">
        /// Filter users by their amount of planned eats. More than or equal
        /// </param>
        /// <param name="maxPlannedEats">
        /// Filter users by their amount of planned eats. Less than or equal
        /// </param>
        /// <param name="minEmotionMedia">
        /// Filter users by their reported emotion media. More than or equal
        /// </param>
        /// <param name="maxEmotionMedia">
        /// Filter users by their reported emotion media. Less than or equal
        /// </param>
        /// <param name="search">For searching purposes.</param>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(ICollection<UserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetUsers(int? page, int? perPage, string sortOrder, string search,
            int? statusFilter, int? minPlannedEats, int? maxPlannedEats, double? minEmotionMedia, double? maxEmotionMedia)
        {
            var loggedUser = User.GetUserIdFromToken();
            var pag = page ?? 1;
            var perPag = perPage ?? 10;
            var statusF = statusFilter ?? -1;

            var result = await _userService.GetUsersAsync(loggedUser, pag, perPag, sortOrder,
                statusF, search, minPlannedEats, maxPlannedEats, minEmotionMedia, maxEmotionMedia, null);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<ICollection<UserResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get all users in a group, it is allowed to use filtering. Requires authentication. Admin
        /// and GroupAdmin access.
        /// </summary>
        /// <param name="id">Group id</param>
        /// <param name="page">Page for pagination purposes.</param>
        /// <param name="perPage">How many issues per page.</param>
        /// <param name="sortOrder">For sortering purposes.</param>
        /// <param name="statusFilter">For filtering for status.</param>
        /// <param name="minPlannedEats">
        /// Filter users by their amount of planned eats. More than or equal
        /// </param>
        /// <param name="maxPlannedEats">
        /// Filter users by their amount of planned eats. Less than or equal
        /// </param>
        /// <param name="minEmotionMedia">
        /// Filter users by their reported emotion media. More than or equal
        /// </param>
        /// <param name="maxEmotionMedia">
        /// Filter users by their reported emotion media. Less than or equal
        /// </param>
        /// <param name="search">For searching purposes.</param>
        [HttpGet("groups/{id}")]
        [Authorize(Roles = "ADMIN,GROUP_ADMIN")]
        [ProducesResponseType(typeof(ICollection<UserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetGroupUsers([FromRoute] int id, int? page, int? perPage, string sortOrder, string search,
            int? statusFilter, int? minPlannedEats, int? maxPlannedEats, double? minEmotionMedia, double? maxEmotionMedia)
        {
            var loggedUser = User.GetUserIdFromToken();
            var pag = page ?? 1;
            var perPag = perPage ?? 10;
            var statusF = statusFilter ?? -1;
            var group = _groupService.GetGroupAsync(id);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, group, Operations.Read);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            var result = await _userService.GetUsersAsync(loggedUser, pag, perPag, sortOrder,
                statusF, search, minPlannedEats, maxPlannedEats, minEmotionMedia, maxEmotionMedia, id);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<ICollection<UserResponse>>(result);
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
        /// Get users stats by dates. Requires authentication. Admin access.
        /// </summary>
        ///<param name="dateType">Date type. 0- Today, 1- Week, 2- Month, 3- Year</param>
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

        /// <summary>
        /// Get eats stats by dates. Requires authentication. Admin access.
        /// </summary>
        ///<param name="dateType">Date type. 0- Today, 1- Week, 2- Month, 3- Year</param>
        [HttpGet("eats-by-date")]
        [ProducesResponseType(typeof(IEnumerable<UsersByDateSeriesResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetEatsStatsByDates([FromQuery] int? dateType)
        {
            var type = dateType ?? 0;
            var loggedUser = User.GetUserIdFromToken();

            var result = await _userService.GetEatsStatsByDateAsync(loggedUser, type);

            return Ok(new ApiOkResponse(result));
        }

        /// <summary>
        /// Enable user. Requires authentication. Admin access.
        /// </summary>
        /// <param name="id">User id</param>
        [HttpPost("{id}/enable")]
        [ProducesResponseType(typeof(UserResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> EnableUser([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();

            var result = await _userService.EnableUserAsync(loggedUser, id);

            return Ok(new ApiOkResponse(result));
        }

        /// <summary>
        /// Disable user. Requires authentication. Admin access.
        /// </summary>
        /// <param name="id">User id</param>
        [HttpPost("{id}/disable")]
        [ProducesResponseType(typeof(UserResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DisableUser([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();

            var result = await _userService.DisableUserAsync(loggedUser, id);

            return Ok(new ApiOkResponse(result));
        }

        /// <summary>
        /// Send user notification. Requires authentication. Admin access.
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="notif">Notification request obj</param>
        [HttpPost("{id}/notification")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UserNotification([FromRoute] int id, [FromBody] UserNotificationRequest notif)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _userService.SendUserNotificationAsync(loggedUser, id, notif);

            return Ok();
        }

        /// <summary>
        /// Get eats count. Requires authentication. Admin access.
        /// </summary>
        [HttpGet("eat-count")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetEatsCount()
        {
            var loggedUser = User.GetUserIdFromToken();

            var result = await _userService.GetEatsCountAsync(loggedUser);

            return Ok(new ApiOkResponse(result));
        }

        /// <summary>
        /// Get current user health parameters for eat plan. Requires authentication.
        /// </summary>
        /// <param name="dateInUtc">Date of the plan to evaluate</param>
        [HttpGet("eat-health-parameters")]
        [ProducesResponseType(typeof(UserEatHealtParametersResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetEatHealthParameters([FromQuery] DateTime dateInUtc)
        {
            var loggedUser = User.GetUserIdFromToken();
            var user = await _userService.GetUserDevicesAsync(loggedUser);
            var userImcKcal = await _eatService.GetKCalImcAsync(loggedUser, dateInUtc);

            IHealthyHelper healthyHelper = new HealthyHelper(userImcKcal.imc, userImcKcal.kcal, _accountService, _dishService);
            var result = healthyHelper.GetUserEatHealtParameters(user);

            return Ok(new ApiOkResponse(result));
        }
    }
}
