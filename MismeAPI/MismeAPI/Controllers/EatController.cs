using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities;
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
    [Route("api/eat")]
    public class EatController : Controller
    {
        private readonly IEatService _eatService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IRewardHelper _rewardHelper;
        private readonly IAccountService _accountService;
        private readonly IDishService _dishService;
        private readonly IAuthorizationService _authorizationService;

        public EatController(IEatService eatService, IUserService userService, IMapper mapper, IRewardHelper rewardHelper, IAccountService accountService,
            IDishService dishService, IAuthorizationService authorizationService)
        {
            _eatService = eatService ?? throw new ArgumentNullException(nameof(eatService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _rewardHelper = rewardHelper ?? throw new ArgumentNullException(nameof(rewardHelper));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
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

            var user = await _userService.GetUserAsync(userId);

            var eatTyp = eatType ?? -1;

            var result = await _eatService.GetAllUserEatsByDateAsync(userId, date, endDate, eatTyp);

            var mapped = _mapper.Map<IEnumerable<EatResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });

            if (user.Role == Data.Entities.Enums.RoleEnum.NORMAL)
            {
                // THIS IS TRASH BUT I CAN"T FIND A BETTER WAY TO DEAL WITH THIS
                var sex = await _accountService.GetSexAsync(userId);
                var height = await _accountService.GetHeightAsync(userId);
                var factor = 1.0;
                foreach (var item in mapped)
                {
                    foreach (var item1 in item.EatDishResponse)
                    {
                        switch (item1.Dish.HandCode)
                        {
                            case 3:
                                factor = await _dishService.GetConversionFactorAsync(height, sex, 3);
                                break;

                            case 6:
                                factor = await _dishService.GetConversionFactorAsync(height, sex, 6);
                                break;

                            case 10:
                                factor = await _dishService.GetConversionFactorAsync(height, sex, 10);
                                break;

                            case 11:
                                factor = await _dishService.GetConversionFactorAsync(height, sex, 11);
                                break;

                            case 19:
                                factor = await _dishService.GetConversionFactorAsync(height, sex, 19);
                                break;

                            case 4:
                                factor = await _dishService.GetConversionFactorAsync(height, sex, 4);
                                break;

                            default:
                                break;
                        }
                        item1.Dish.Calcium = item1.Dish.Calcium * factor;
                        item1.Dish.Calories = item1.Dish.Calories * factor;
                        item1.Dish.Carbohydrates = item1.Dish.Carbohydrates * factor;
                        item1.Dish.Cholesterol = item1.Dish.Cholesterol * factor;
                        item1.Dish.Fat = item1.Dish.Fat * factor;
                        item1.Dish.Fiber = item1.Dish.Fiber * factor;
                        item1.Dish.Iron = item1.Dish.Iron * factor;
                        item1.Dish.MonoUnsaturatedFat = item1.Dish.MonoUnsaturatedFat * factor;
                        item1.Dish.Phosphorus = item1.Dish.Phosphorus * factor;
                        item1.Dish.PolyUnsaturatedFat = item1.Dish.PolyUnsaturatedFat * factor;
                        item1.Dish.Potassium = item1.Dish.Potassium * factor;
                        item1.Dish.Proteins = item1.Dish.Proteins * factor;
                        item1.Dish.SaturatedFat = item1.Dish.SaturatedFat * factor;
                        item1.Dish.Sodium = item1.Dish.Sodium * factor;
                        item1.Dish.VitaminA = item1.Dish.VitaminA * factor;
                        item1.Dish.VitaminB12 = item1.Dish.VitaminB12 * factor;
                        item1.Dish.VitaminB1Thiamin = item1.Dish.VitaminB1Thiamin * factor;
                        item1.Dish.VitaminB2Riboflavin = item1.Dish.VitaminB2Riboflavin * factor;
                        item1.Dish.VitaminB3Niacin = item1.Dish.VitaminB3Niacin * factor;
                        item1.Dish.VitaminB6 = item1.Dish.VitaminB6 * factor;
                        item1.Dish.VitaminB9Folate = item1.Dish.VitaminB9Folate * factor;
                        item1.Dish.VitaminC = item1.Dish.VitaminC * factor;
                        item1.Dish.VitaminD = item1.Dish.VitaminD * factor;
                        item1.Dish.VitaminE = item1.Dish.VitaminE * factor;
                        item1.Dish.VitaminK = item1.Dish.VitaminK * factor;
                        item1.Dish.Zinc = item1.Dish.Zinc * factor;
                    }
                }
            }
            var planSummaries = await _eatService.GetPlanSummaryAsync(result);

            return Ok(new ApiPlanResponse(mapped, planSummaries));
        }

        /// <summary>
        /// Get user eats by date. Requires authentication. Only group admins and admins can access
        /// </summary>
        /// <param name="id">Id of the user to get the plan summaries</param>
        /// <param name="date">Specific date to filter. Must be in UTC format.</param>
        /// <param name="endDate">Specific end date to filter. Must be in UTC format.</param>
        [HttpGet("users/{id}/plan-summaries")]
        [Authorize(Roles = "GROUP_ADMIN,ADMIN")]
        [ProducesResponseType(typeof(IEnumerable<PlanSummaryResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserPlanSummariesByDateRange([FromRoute] int id, DateTime date, DateTime endDate)
        {
            var user = await _userService.GetUserAsync(id);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, user, Operations.ManagePlans);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            var eatTyp = -1;
            var eats = await _eatService.GetAllUserEatsByDateAsync(id, date, endDate, eatTyp);

            var planSummaries = await _eatService.GetPlanSummaryAsync(eats);

            return Ok(new ApiOkResponse(planSummaries));
        }

        /// <summary>
        /// Get all user eats. Only an admin or group admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="userId">Specific user id to filter.</param>
        /// <param name="date">Specific date to filter. Must be in UTC format.</param>
        /// <param name="eatType">
        /// Eat by type: 0- Breakfast, 1- Snack1, 2- Lunch, 3- Snack2, 4- Dinner.
        /// </param>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of eats to be displayed per page. 10 by default.</param>
        [HttpGet("user-eats")]
        [Authorize(Roles = "GROUP_ADMIN,ADMIN")]
        [ProducesResponseType(typeof(IEnumerable<EatResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllEatsByDate(int userId, DateTime? date, int? page, int? perPage, int? eatType)
        {
            var user = await _userService.GetUserAsync(userId);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, user, Operations.ManagePlans);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            var adminId = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(adminId);
            var eatTyp = eatType ?? -1;
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _eatService.GetAdminAllUserEatsAsync(pag, perPag, userId, date, eatTyp);
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
        /// Add or update bulk eats to an user. Requires authentication. Only admin or group admin
        /// of the target user can do this operation
        /// </summary>
        /// <param name="userId">User id to update plan</param>
        /// <param name="eat">Eat request object.</param>
        [HttpPost("users/{userId}/bulk-eats")]
        [Authorize]
        [ProducesResponseType(typeof(EatResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddEatsToUser([FromRoute] int userId, [FromBody] CreateBulkEatRequest eat)
        {
            var user = await _userService.GetUserDevicesAsync(userId);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, user, Operations.ManagePlans);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            await _eatService.CreateBulkEatAsync(userId, eat);

            return Ok();
        }

        /// <summary>
        /// Apply menu to user day plan - Add bulk eats by menu. Requires authentication.
        /// </summary>
        /// <param name="userId">User to wich the menue will be applied</param>
        /// <param name="menuId">Menu to be applied</param>
        /// <param name="dateInUtc">Date in wich the menu will be applied to users plan</param>
        [HttpPost("users/{userId}/menues/{menuId}/bulk-eats")]
        [Authorize(Roles = "GROUP_ADMIN,ADMIN")]
        [ProducesResponseType(typeof(OkResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddEatsFromMenue([FromRoute] int userId, [FromRoute] int menuId, DateTime dateInUtc)
        {
            var loggedUser = User.GetUserIdFromToken();
            var user = await _userService.GetUserDevicesAsync(userId);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, user, Operations.ManagePlans);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            await _eatService.CreateBulkEatFromMenuAsync(loggedUser, userId, menuId, dateInUtc);

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

        /// <summary>
        /// Return is valanced summary of any user on a given date. Requires authentication.
        /// </summary>
        /// <param name="userId">Id of the user to know the plan summary</param>
        /// <param name="dateInUtc">Date of the plan to evaluate</param>
        [HttpGet("users/{userId}/is-balanced-plan")]
        [Authorize(Roles = "GROUP_ADMIN,ADMIN")]
        [ProducesResponseType(typeof(EatBalancedSummaryResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> IsValancedPlanAdmin([FromRoute] int userId, [FromQuery] DateTime dateInUtc)
        {
            var user = await _userService.GetUserDevicesAsync(userId);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, user, Operations.ManagePlans);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            var plan = await _eatService.GetUserPlanPerDateAsync(userId, dateInUtc);
            var userImcKcal = await _eatService.GetKCalImcAsync(userId, dateInUtc);

            IHealthyHelper healthyHelper = new HealthyHelper(userImcKcal.imc, userImcKcal.kcal, _accountService, _dishService);
            var result = healthyHelper.IsBalancedPlan(user, plan);

            return Ok(new ApiOkResponse(result));
        }

        /// <summary>
        /// Return is valanced summary of a given plan and user. Requires authentication.
        /// </summary>
        /// <param name="userId">Id of the user to know the plan summary</param>
        /// <param name="plan">Plan to calculate the is balanced parameters</param>
        [HttpPut("users/{userId}/is-balanced-plan")]
        [Authorize(Roles = "GROUP_ADMIN,ADMIN")]
        [ProducesResponseType(typeof(EatBalancedSummaryResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> IsValancedByPlan([FromRoute] int userId, [FromBody] IEnumerable<EatResponse> plan)
        {
            var user = await _userService.GetUserDevicesAsync(userId);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, user, Operations.ManagePlans);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            var planMapped = _mapper.Map<IEnumerable<Eat>>(plan);
            var userImcKcal = await _eatService.GetKCalImcAsync(userId, DateTime.UtcNow);

            IHealthyHelper healthyHelper = new HealthyHelper(userImcKcal.imc, userImcKcal.kcal, _accountService, _dishService);
            var result = healthyHelper.IsBalancedPlan(user, planMapped);

            return Ok(new ApiOkResponse(result));
        }
    }
}
