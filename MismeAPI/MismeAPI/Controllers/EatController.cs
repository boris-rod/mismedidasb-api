using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
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
    [Route("api/eat")]
    public class EatController : Controller
    {
        private readonly IEatService _eatService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IRewardHelper _rewardHelper;
        private readonly IAccountService _accountService;
        private readonly IDishService _dishService;

        public EatController(IEatService eatService, IUserService userService, IMapper mapper, IRewardHelper rewardHelper, IAccountService accountService, IDishService dishService)
        {
            _eatService = eatService ?? throw new ArgumentNullException(nameof(eatService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _rewardHelper = rewardHelper ?? throw new ArgumentNullException(nameof(rewardHelper));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
        }

        /// <summary>
        /// Get all logged user eats. Requires authentication.
        /// </summary>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of eats to be displayed per page. 10 by default.</param>
        /// <param name="eatType">
        /// Eat by type: 0- Breakfast, 1- Snack1, 2- Lunch, 3- Snack2, 4- Dinner.
        /// </param>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<EatResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll(int? page, int? perPage, int? eatType)
        {
            var userId = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(userId);

            var pag = page ?? 1;
            var perPag = perPage ?? 10;
            var eatTyp = eatType ?? -1;

            var result = await _eatService.GetPaggeableAllUserEatsAsync(userId, pag, perPag, eatTyp);
            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<EatResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get all logged user eats by date. Requires authentication.
        /// </summary>
        /// <param name="date">Specific date to filter. Must be in UTC format.</param>
        /// <param name="endDate">Specific end date to filter. Must be in UTC format.</param>
        /// <param name="eatType">
        /// Eat by type: 0- Breakfast, 1- Snack1, 2- Lunch, 3- Snack2, 4- Dinner.
        /// </param>
        [HttpGet("by-date")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<EatResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllEatsByDate(DateTime date, DateTime endDate, int? eatType)
        {
            var userId = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(userId);

            var eatTyp = eatType ?? -1;

            var result = await _eatService.GetAllUserEatsByDateAsync(userId, date, endDate, eatTyp);

            var mapped = _mapper.Map<IEnumerable<EatResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });

            var planSummaries = await _eatService.GetPlanSummaryAsync(result);

            return Ok(new ApiPlanResponse(mapped, planSummaries));
        }

        /// <summary>
        /// Get all user eats. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="userId">Specific user id to filter.</param>
        /// <param name="date">Specific date to filter. Must be in UTC format.</param>
        /// <param name="eatType">
        /// Eat by type: 0- Breakfast, 1- Snack1, 2- Lunch, 3- Snack2, 4- Dinner.
        /// </param>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of eats to be displayed per page. 10 by default.</param>
        [HttpGet("user-eats")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<EatResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllEatsByDate(int userId, DateTime? date, int? page, int? perPage, int? eatType)
        {
            var adminId = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(adminId);

            var eatTyp = eatType ?? -1;
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _eatService.GetAdminAllUserEatsAsync(adminId, pag, perPag, userId, date, eatTyp);
            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<EatResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get all user eats. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="userId">User's id.</param>
        /// <param name="eatType">
        /// Eat by type: 0- Breakfast, 1- Snack1, 2- Lunch, 3- Snack2, 4- Dinner.
        /// </param>
        /// <param name="sortOrder">Sort eats</param>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of eats to be displayed per page. 10 by default.</param>
        [HttpGet("admin-user-eats")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(IEnumerable<EatResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllEatsByUser(int userId, int? page, int? perPage, int? eatType, string sortOrder)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var eatTyp = eatType ?? -1;
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _eatService.GetPaggeableAllUserEatsAsync(userId, pag, perPag, eatTyp, sortOrder);
            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<EatResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Add an eat. Requires authentication.
        /// </summary>
        /// <param name="eat">Eat request object.</param>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(EatResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddDish([FromBody] CreateEatRequest eat)
        {
            /*This endpoint is not in use anymore - you should disable it*/
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            var result = await _eatService.CreateEatAsync(loggedUser, eat);

            var mapped = _mapper.Map<EatResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });

            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Add or update an eat. If the eat exists is updated, if not is created. Requires authentication.
        /// </summary>
        /// <param name="eat">Eat request object.</param>
        [HttpPost("add-or-update")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddOrUpdateEat([FromBody] CreateEatRequest eat)
        {
            var loggedUser = User.GetUserIdFromToken();

            await _eatService.AddOrUpdateEatAsync(loggedUser, eat);

            return Ok();
        }

        /// <summary>
        /// Update an eat. Requires authentication.
        /// </summary>
        /// <param name="eat">Eat request object.</param>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(EatResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(EatResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateDish([FromBody] UpdateEatRequest eat)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            var result = await _eatService.UpdateEatAsync(loggedUser, eat);

            var mapped = _mapper.Map<EatResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Add or update bulk eats. Requires authentication.
        /// </summary>
        /// <param name="eat">Eat request object.</param>
        [HttpPost("bulk-eats")]
        [Authorize]
        [ProducesResponseType(typeof(EatResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddEats([FromBody] CreateBulkEatRequest eat)
        {
            var loggedUser = User.GetUserIdFromToken();

            await _eatService.CreateBulkEatAsync(loggedUser, eat);

            return Ok();
        }

        /// <summary>
        /// Return is valanced summary of a current user plan on a given date. Requires authentication.
        /// </summary>
        /// <param name="dateInUtc">Date of the plan to evaluate</param>
        [HttpGet("is-balanced-plan")]
        [Authorize]
        [ProducesResponseType(typeof(EatBalancedSummaryResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> IsValancedPlan([FromQuery] DateTime dateInUtc)
        {
            var loggedUser = User.GetUserIdFromToken();
            var user = await _userService.GetUserDevicesAsync(loggedUser);
            var plan = await _eatService.GetUserPlanPerDateAsync(loggedUser, dateInUtc);
            var userImcKcal = await _eatService.GetKCalImcAsync(loggedUser, dateInUtc);

            IHealthyHelper healthyHelper = new HealthyHelper(userImcKcal.imc, userImcKcal.kcal, _accountService, _dishService);
            var result = healthyHelper.IsBalancedPlan(user, plan);

            return Ok(new ApiOkResponse(result));
        }
    }
}