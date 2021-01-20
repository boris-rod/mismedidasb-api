using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.CompoundDish;
using MismeAPI.Common.DTO.Request.Dish;
using MismeAPI.Common.DTO.Response.CompoundDish;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Service;
using MismeAPI.Service.Utils;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/compound-dish")]
    public class CompoundDishController : Controller
    {
        private readonly ICompoundDishService _compoundDishService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IRewardHelper _rewardHelper;

        public CompoundDishController(ICompoundDishService compoundDishService, IMapper mapper, IUserService userService, IRewardHelper rewardHelper)
        {
            _compoundDishService = compoundDishService ?? throw new ArgumentNullException(nameof(compoundDishService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _rewardHelper = rewardHelper ?? throw new ArgumentNullException(nameof(rewardHelper));
        }

        /// <summary>
        /// Delete a compound dish. Only the owner can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Dish id.</param>
        [HttpPost("delete/{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteDish([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _compoundDishService.DeleteCompoundDishAsync(loggedUser, id);
            return Ok();
        }

        /// <summary>
        /// Get all user compound dishes. Requires authentication.
        /// </summary>
        /// <param name="search">Search param.</param>
        /// <param name="favorites">Filter by favorite check</param>
        /// <param name="lackSelfControl">Filter by self lack control check</param>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<CompoundDishResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUser(string search, bool? favorites, bool? lackSelfControl)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            var result = await _compoundDishService.GetUserCompoundDishesAsync(loggedUser, search, favorites, lackSelfControl);

            var mapped = _mapper.Map<IEnumerable<CompoundDishResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get all compound dishes. Only an admin can do this operation. Requires authentication.
        /// </summary>
        [HttpGet("admin")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<AdminCompoundDishResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<ApiResponse>), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetDishesAdmin(string search, int filter, int? page, int? perPage, string sort)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _compoundDishService.GetAllCompoundDishesAsync(loggedUser, search, filter, page ?? 1, perPage ?? 10, sort);
            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<AdminCompoundDishResponse>>(result, opt =>
            {
                opt.Items["lang"] = "es";
            });
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Add a compound dish. Requires authentication.
        /// </summary>
        /// <param name="dish">Dish request object.</param>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(CompoundDishResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> AddCompoundDish(CreateCompoundDishRequest dish)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _compoundDishService.CreateCompoundDishAsync(loggedUser, dish);
            var mapped = _mapper.Map<AdminCompoundDishResponse>(result, opt =>
            {
                opt.Items["lang"] = "es";
            });
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Update a compound dish. Requires authentication.
        /// </summary>
        /// <param name="dish">Dish request object.</param>
        /// <param name="id">Dish id.</param>
        [HttpPut("{id}/update")]
        [Authorize]
        [ProducesResponseType(typeof(CompoundDishResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> UpdateCompoundDish(int id, UpdateCompoundDishRequest dish)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _compoundDishService.UpdateCompoundDishAsync(loggedUser, id, dish);
            var mapped = _mapper.Map<CompoundDishResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Mark a compound dish as reviewed. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Dish id.</param>
        [HttpPost("{id}/reviewed")]
        [Authorize]
        [ProducesResponseType(typeof(CompoundDishResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> MarkCompoundDishReviewed([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _compoundDishService.MarkCompoundDishAsReviewedAsync(loggedUser, id);
            //var mapped = _mapper.Map<AdminCompoundDishResponse>(result, opt =>
            //{
            //    opt.Items["lang"] = "es";
            //});
            return Ok();
        }

        /// <summary>
        /// Convert a user dish in a general dish. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="dish">Dish request object.</param>
        [HttpPost("create-dish")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> AddDish(UpdateDishRequest dish)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _compoundDishService.ConvertUserDishAsync(loggedUser, dish);

            var user = await _userService.GetUserDevicesAsync(loggedUser);
            /*Reward section*/
            await _rewardHelper.HandleRewardAsync(RewardCategoryEnum.DISH_BUILT, dish.UserId, true, dish, null, NotificationTypeEnum.FIREBASE, user.Devices);
            /*#end reward section*/

            return Created("", null);
        }

        /// <summary>
        /// Add a compund dish to current user favorite dishes. Requires authentication.
        /// </summary>
        /// <param name="dishId">Dish to add to favorite</param>
        [HttpPost("favorite/create")]
        [Authorize]
        [ProducesResponseType(typeof(CompoundDishResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddFavoriteDish(int dishId)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _compoundDishService.AddFavoriteAsync(loggedUser, dishId);

            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var mapped = _mapper.Map<CompoundDishResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a compund dish from current user favorite dishes. Requires authentication.
        /// </summary>
        /// <param name="dishId">Dish to remove from favorites</param>
        [HttpDelete("favorite/delete")]
        [Authorize]
        [ProducesResponseType(typeof(CompoundDishResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteFavoriteDish(int dishId)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _compoundDishService.RemoveFavoriteDishAsync(loggedUser, dishId);

            return Ok();
        }

        /// <summary>
        /// Add/Update a compund dish to current user lack-self-control dishes. Requires authentication.
        /// </summary>
        /// <param name="request">Request object - contains dish and intensity</param>
        [HttpPost("lack-self-control/create-update")]
        [Authorize]
        [ProducesResponseType(typeof(CompoundDishResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddLackSelfControlDish(CreateUpdateLackControlDishRequest request)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _compoundDishService.AddOrUpdateLackselfControlDishAsync(loggedUser, request.DishId, request.Intensity);

            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var mapped = _mapper.Map<DishCompoundDishResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a compound dish from current user lack-self-control dishes. Requires authentication.
        /// </summary>
        /// <param name="dishId">Dish to remove from lack-self-control dishes</param>
        [HttpDelete("lack-self-control/delete")]
        [Authorize]
        [ProducesResponseType(typeof(CompoundDishResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteLackSelfControlDish(int dishId)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _compoundDishService.RemoveLackselfControlDishAsync(loggedUser, dishId);

            return Ok();
        }
    }
}