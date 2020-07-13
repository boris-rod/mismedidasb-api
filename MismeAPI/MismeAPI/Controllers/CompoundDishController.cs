using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.CompoundDish;
using MismeAPI.Common.DTO.Response.CompoundDish;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Service;
using MismeAPI.Service.Utils;
using MismeAPI.Utils;
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
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<CompoundDishResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUser(string search)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            var result = await _compoundDishService.GetUserCompoundDishesAsync(loggedUser, search);

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
        public async Task<IActionResult> GetDishesAdmin(string search, int filter)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _compoundDishService.GetAllCompoundDishesAsync(loggedUser, search, filter);
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
    }
}
