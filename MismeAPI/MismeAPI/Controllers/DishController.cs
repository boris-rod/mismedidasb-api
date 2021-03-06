﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Dish;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
using MismeAPI.Services;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/dish")]
    public class DishController : Controller
    {
        private readonly IDishService _dishService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public DishController(IDishService dishService, IUserService userService, IMapper mapper, IAccountService accountService)
        {
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        /// <summary>
        /// Get all dishes. Requires authentication.
        /// </summary>
        /// <param name="search">Search param.</param>
        /// <param name="tags">Tags id for filtering.</param>
        /// <param name="page">Page to be listed. If null all the dishes will be returned.</param>
        /// <param name="perPage">By defaul 10 but only will take effect if the page param is specified.</param>
        /// <param name="harvardFilter">0- proteic, 1- caloric, 2- fruitVegetable.</param>
        /// <param name="sort">Sort diches</param>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<DishResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll(string search, List<int> tags, int? page, int? perPage, int? harvardFilter, string sort)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var user = await _userService.GetUserAsync(loggedUser);

            var result = await _dishService.GetDishesAsync(search, tags, page, perPage, harvardFilter, sort);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<DishResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });

            if (user.Role == Data.Entities.Enums.RoleEnum.NORMAL)
            {
                // THIS IS TRASH BUT I CAN"T FIND A BETTER WAY TO DEAL WITH THIS
                var sex = await _accountService.GetSexAsync(loggedUser);
                var height = await _accountService.GetHeightAsync(loggedUser);
                var factor = 1.0;
                foreach (var item in mapped)
                {
                    switch (item.HandCode)
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
                    item.Calcium = item.Calcium * factor;
                    item.Calories = item.Calories * factor;
                    item.Carbohydrates = item.Carbohydrates * factor;
                    item.Cholesterol = item.Cholesterol * factor;
                    item.Fat = item.Fat * factor;
                    item.Fiber = item.Fiber * factor;
                    item.Iron = item.Iron * factor;
                    item.MonoUnsaturatedFat = item.MonoUnsaturatedFat * factor;
                    item.Phosphorus = item.Phosphorus * factor;
                    item.PolyUnsaturatedFat = item.PolyUnsaturatedFat * factor;
                    item.Potassium = item.Potassium * factor;
                    item.Proteins = item.Proteins * factor;
                    item.SaturatedFat = item.SaturatedFat * factor;
                    item.Sodium = item.Sodium * factor;
                    item.VitaminA = item.VitaminA * factor;
                    item.VitaminB12 = item.VitaminB12 * factor;
                    item.VitaminB1Thiamin = item.VitaminB1Thiamin * factor;
                    item.VitaminB2Riboflavin = item.VitaminB2Riboflavin * factor;
                    item.VitaminB3Niacin = item.VitaminB3Niacin * factor;
                    item.VitaminB6 = item.VitaminB6 * factor;
                    item.VitaminB9Folate = item.VitaminB9Folate * factor;
                    item.VitaminC = item.VitaminC * factor;
                    item.VitaminD = item.VitaminD * factor;
                    item.VitaminE = item.VitaminE * factor;
                    item.VitaminK = item.VitaminK * factor;
                    item.Zinc = item.Zinc * factor;
                }
            }
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get all favorite dishes of the logged user. Requires authentication.
        /// </summary>
        /// <param name="search">Search param.</param>
        /// <param name="tags">Tags id for filtering.</param>
        /// <param name="page">Page to be listed. If null all the dishes will be returned.</param>
        /// <param name="perPage">By defaul 10 but only will take effect if the page param is specified.</param>
        /// <param name="harvardFilter">0- proteic, 1- caloric, 2- fruitVegetable.</param>
        [HttpGet("favorites")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<DishResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetFavorites(string search, List<int> tags, int? page, int? perPage, int? harvardFilter)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            var result = await _dishService.GetFavoriteDishesAsync(loggedUser, search, tags, page, perPage, harvardFilter);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<DishResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get all lack self control dishes of the logged user. Requires authentication.
        /// </summary>
        /// <param name="search">Search param.</param>
        /// <param name="tags">Tags id for filtering.</param>
        /// <param name="page">Page to be listed. If null all the dishes will be returned.</param>
        /// <param name="perPage">By defaul 10 but only will take effect if the page param is specified.</param>
        /// <param name="harvardFilter">0- proteic, 1- caloric, 2- fruitVegetable.</param>
        [HttpGet("lack-self-control-dishes")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<DishResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetLackSelfControlDishes(string search, List<int> tags, int? page, int? perPage, int? harvardFilter)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            var result = await _dishService.GetLackSelfControlDishesAsync(loggedUser, search, tags, page, perPage, harvardFilter);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<DishResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get dish by id. Requires authentication.
        /// </summary>
        /// <param name="id">Dish id.</param>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(DishResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            var result = await _dishService.GetDishByIdAsync(id);
            var mapped = _mapper.Map<DishResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Add a dish. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="dish">Dish request object.</param>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(DishResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> AddDish(CreateDishRequest dish)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _dishService.CreateDishAsync(loggedUser, dish);

            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var mapped = _mapper.Map<DishResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Update a dish. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="dish">Dish request object.</param>
        [HttpPost("update")]
        [Authorize]
        [ProducesResponseType(typeof(DishResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> UpdateDish(UpdateDishRequest dish)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _dishService.UpdateDishAsync(loggedUser, dish);
            //var mapped = _mapper.Map<DishResponse>(result);
            //return Ok(new ApiOkResponse(mapped));
            return Ok();
        }

        /// <summary>
        /// Delete a dish. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Dish id.</param>
        [HttpPost("delete/{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> DeleteDish([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _dishService.DeleteDishAsync(loggedUser, id);
            return Ok();
        }

        /// <summary>
        /// Get dishes for translate. Only an admin can do this operation. Requires authentication.
        /// </summary>
        [HttpGet("admin")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<DishAdminResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<ApiResponse>), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetDishAdmin()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _dishService.GetDishesAdminAsync(loggedUser);
            var mapped = _mapper.Map<IEnumerable<DishAdminResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Change a dish i18n. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="dishTranslationRequest">Dish translation request object.</param>
        /// <param name="id">Dish id.</param>
        [HttpPost("{id}/define-translation")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> QuestionTranslation([FromRoute] int id, [FromBody] DishTranslationRequest dishTranslationRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _dishService.ChangeDishTranslationAsync(loggedUser, dishTranslationRequest, id);
            return Ok();
        }

        /// <summary>
        /// Add a dish to current user favorite dishes. Requires authentication.
        /// </summary>
        /// <param name="dishId">Dish to add to favorite</param>
        [HttpPost("favorite/create")]
        [Authorize]
        [ProducesResponseType(typeof(DishResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddFavoriteDish(int dishId)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _dishService.AddFavoriteDishAsync(loggedUser, dishId);

            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var mapped = _mapper.Map<DishResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a dish from current user favorite dishes. Requires authentication.
        /// </summary>
        /// <param name="dishId">Dish to remove from favorites</param>
        [HttpDelete("favorite/delete")]
        [Authorize]
        [ProducesResponseType(typeof(DishResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteFavoriteDish(int dishId)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _dishService.RemoveFavoriteDishAsync(loggedUser, dishId);

            return Ok();
        }

        /// <summary>
        /// Add a dish to current user lack-self-control dishes. Requires authentication.
        /// </summary>
        /// <param name="request">Request object - contains dish and intensity</param>
        [HttpPost("lack-self-control/create")]
        [Authorize]
        [ProducesResponseType(typeof(DishResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddLackSelfControlDish(CreateUpdateLackControlDishRequest request)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _dishService.AddOrUpdateLackselfControlDishAsync(loggedUser, request.DishId, request.Intensity);

            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var mapped = _mapper.Map<DishResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Update a lack-self-control dishe intensity. Requires authentication.
        /// </summary>
        /// <param name="request">Request object - contains dish and intensity</param>
        [HttpPut("lack-self-control/update")]
        [Authorize]
        [ProducesResponseType(typeof(DishResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateLackSelfControlDish(CreateUpdateLackControlDishRequest request)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _dishService.AddOrUpdateLackselfControlDishAsync(loggedUser, request.DishId, request.Intensity);

            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var mapped = _mapper.Map<DishResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a dish from current user lack-self-control dishes. Requires authentication.
        /// </summary>
        /// <param name="dishId">Dish to remove from lack-self-control dishes</param>
        [HttpDelete("lack-self-control/delete")]
        [Authorize]
        [ProducesResponseType(typeof(DishResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteLackSelfControlDish(int dishId)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _dishService.RemoveLackselfControlDishAsync(loggedUser, dishId);

            return Ok();
        }
    }
}