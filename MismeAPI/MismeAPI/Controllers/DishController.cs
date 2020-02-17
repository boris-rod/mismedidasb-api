using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
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
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<DishResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll(string search)
        {
            var result = await _dishService.GetDishesAsync(search);
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
    }
}