﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.User;
using MismeAPI.Service;
using MismeAPI.Services;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers.Admin
{
    [Route("api/admin/user")]
    [Authorize(Roles = "ADMIN")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly IPollService _pollService;

        public UserController(IUserService userService, IMapper mapper, IAccountService accountService, IPollService pollService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
        }

        /// <summary>
        /// Get user. Requires Admin authentication. Includes subscriptions
        /// </summary>
        /// <param name="id">User's id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserAdminResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserProfile(int id)
        {
            var result = await _accountService.GetUserProfileUseAsync(id);
            var info = await _pollService.GetUserPollsInfoAsync(id);

            var user = _mapper.Map<UserAdminResponse>(result.user);
            user.KCal = result.kcal;
            user.IMC = result.IMC;

            user.Age = info.age;
            user.Sex = info.sex;
            user.Height = info.height;
            user.Weight = info.weight;
            user.HealthMeasuresLastUpdate = info.HealthMeasuresLastUpdate;
            user.ValueMeasuresLastUpdate = info.ValueMeasuresLastUpdate;
            user.WellnessMeasuresLastUpdate = info.WellnessMeasuresLastUpdate;
            user.LastPlanedEat = info.LastPlanedEat;

            return Ok(new ApiOkResponse(user));
        }

        /// <summary>
        /// Get user latest answer of a poll (value or wellness only). Require authentication. Admin
        /// access requires
        /// </summary>
        /// <returns>Active cut points</returns>
        [HttpGet("{id}/question-answers")]
        [ProducesResponseType(typeof(IEnumerable<QuestionResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index(int id, string conceptName)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var dict = await _pollService.GetLastAnsweredDictAsync(loggedUser);

            var result = await _pollService.GetLatestPollAnswerByUser(conceptName, id);

            var mapped = _mapper.Map<IEnumerable<QuestionResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
                opt.Items["dict"] = dict;
            });

            return Ok(new ApiOkResponse(mapped));
        }
    }
}
