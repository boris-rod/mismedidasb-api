using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.User;
using MismeAPI.Service;
using MismeAPI.Service.Utils;
using MismeAPI.Services;
using MismeAPI.Utils;
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
        private readonly IProfileHelthHelper _profileHelthHelper;
        private readonly IMapper _mapper;
        private readonly IPollService _pollService;
        private readonly INotificationService _notificationService;

        public UserController(IUserService userService, IMapper mapper, IProfileHelthHelper profileHelthHelper, IPollService pollService, INotificationService notificationService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _profileHelthHelper = profileHelthHelper ?? throw new ArgumentNullException(nameof(profileHelthHelper));
            _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
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
            var result = await _profileHelthHelper.GetUserProfileUseAsync(id);
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

        /// <summary>
        /// TEST
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}/send-notification")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Test(int id)
        {
            var user = await _userService.GetUserDevicesAsync(id);
            var title = "Participa en el Grupo PlaniFive";
            var body = "Ver mas detalles";
            var externalUrl = "https://metriri.com/blog/te-invitamos-a-participar-en-el-grupo-planifive";
            await _notificationService.SendFirebaseNotificationAsync(title, body, user.Devices, externalUrl);

            return Ok();
        }
    }
}
