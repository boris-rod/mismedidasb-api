﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request.CompoundDish;
using MismeAPI.Common.DTO.Response.CompoundDish;
using MismeAPI.Service;
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

        public CompoundDishController(ICompoundDishService compoundDishService, IMapper mapper, IUserService userService)
        {
            _compoundDishService = compoundDishService ?? throw new ArgumentNullException(nameof(compoundDishService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Delete a compound dish. Only the owner can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Dish id.</param>
        [HttpPost("delete/{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteDish([FromRoute]int id)
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
        public async Task<IActionResult> GetDishesAdmin(string search)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _compoundDishService.GetAllCompoundDishesAsync(loggedUser, search);
            var mapped = _mapper.Map<IEnumerable<AdminCompoundDishResponse>>(result);
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
        public async Task<IActionResult> AddCompoundDish([FromBody] CreateCompoundDishRequest dish)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _compoundDishService.CreateCompoundDishAsync(loggedUser, dish);
            var mapped = _mapper.Map<CompoundDishResponse>(result);
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
        public async Task<IActionResult> UpdateCompoundDish(int id, [FromBody] UpdateCompoundDishRequest dish)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _compoundDishService.UpdateCompoundDishAsync(loggedUser, id, dish);
            var mapped = _mapper.Map<CompoundDishResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }
    }
}