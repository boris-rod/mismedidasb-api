﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
using MismeAPI.Utils;
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
        private readonly IMapper _mapper;

        public DishController(IDishService dishService, IMapper mapper)
        {
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all dishes. Requires authentication.
        /// </summary>
        /// <param name="search">Search param.</param>
        /// <param name="tags">Tags id for filtering.</param>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<DishResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll(string search, List<int> tags)
        {
            var result = await _dishService.GetDishesAsync(search, tags);
            var mapped = _mapper.Map<IEnumerable<DishResponse>>(result);
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
        public async Task<IActionResult> GetById([FromRoute]int id)
        {
            var result = await _dishService.GetDishByIdAsync(id);
            var mapped = _mapper.Map<DishResponse>(result);
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
            var mapped = _mapper.Map<DishResponse>(result);
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
            var mapped = _mapper.Map<DishResponse>(result);
            return Ok(new ApiOkResponse(mapped));
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
        public async Task<IActionResult> DeleteDish([FromRoute]int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _dishService.DeleteDishAsync(loggedUser, id);
            return Ok();
        }
    }
}