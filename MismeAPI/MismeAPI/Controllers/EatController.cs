﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
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

        public EatController(IEatService eatService, IUserService userService, IMapper mapper)
        {
            _eatService = eatService ?? throw new ArgumentNullException(nameof(eatService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

            var dict = new Dictionary<DateTime, FoodValues>();
            foreach (var item in mapped)
            {
                //var val = dict.Keys.FirstOrDefault(k => k.Date == item.CreatedAt.Date);
                FoodValues value;

                var key = dict.ContainsKey(item.CreatedAt.Date);

                if (key == true)
                {
                    dict.TryGetValue(item.CreatedAt.Date, out value);
                    item.IMC = value.IMC;
                    item.KCal = value.KCal;
                }
                else
                {
                    var res = await _eatService.GetKCalImcAsync(userId, item.CreatedAt);
                    dict.Add(item.CreatedAt.Date, new FoodValues
                    {
                        IMC = res.imc,
                        KCal = res.kcal
                    });

                    item.IMC = res.imc;
                    item.KCal = res.kcal;
                }
            }

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get all user eats. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="userId">Specific date to filter. Must be in UTC format.</param>
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

            var eatTyp = eatType ?? -1;
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _eatService.GetAdminAllUserEatsAsync(adminId, pag, perPag, userId, date, eatTyp);
            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<EatResponse>>(result);
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
        public async Task<IActionResult> AddDish([FromBody]CreateEatRequest eat)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _eatService.CreateEatAsync(loggedUser, eat);
            var mapped = _mapper.Map<EatResponse>(result);
            return Created("", new ApiOkResponse(mapped));
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
        public async Task<IActionResult> UpdateDish([FromBody]UpdateEatRequest eat)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _eatService.UpdateEatAsync(loggedUser, eat);
            var mapped = _mapper.Map<EatResponse>(result);
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
        public async Task<IActionResult> AddEats([FromBody]CreateBulkEatRequest eat)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _eatService.CreateBulkEatAsync(loggedUser, eat);
            return Ok();
        }
    }
}