using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Reward;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.PersonalData;
using MismeAPI.Common.DTO.Response.User;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Service;
using MismeAPI.Service.Utils;
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
        private readonly IProfileHelthHelper _profileHelthHelper;
        private readonly IMapper _mapper;
        private readonly IPollService _pollService;
        private readonly INotificationService _notificationService;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly IPersonalDataService _personalDataService;
        private IWebHostEnvironment _env;

        public UserController(IUserService userService, IMapper mapper, IProfileHelthHelper profileHelthHelper, IPollService pollService,
            INotificationService notificationService, IUserStatisticsService userStatisticsService, IWebHostEnvironment env,
            IPersonalDataService personalDataService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _profileHelthHelper = profileHelthHelper ?? throw new ArgumentNullException(nameof(profileHelthHelper));
            _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userStatisticsService = userStatisticsService ?? throw new ArgumentNullException(nameof(userStatisticsService));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _personalDataService = personalDataService ?? throw new ArgumentNullException(nameof(personalDataService));
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
        /// Give a reward of coins to one or many users. Requires authentication. Only Admin access
        /// </summary>
        [HttpPut("give-coins-rewards")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GiveCoinsReward([FromBody] RewardManualCoinsRequest request)
        {
            await _userStatisticsService.RewardCoinsToUsersAsync(request);

            return Ok();
        }

        /// <summary>
        /// Send an email to one or many users. Requires authentication. Only Admin access
        /// </summary>
        [HttpPut("send-email")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
        {
            request = PrepareEmailBody(request);
            await _userService.SendManualEmailAsync(request);

            return Ok();
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
            var body = "Ver más detalles";
            var externalUrl = "https://metriri.com/blog/te-invitamos-a-participar-en-el-grupo-planifive";
            await _notificationService.SendFirebaseNotificationAsync(title, body, user.Devices, externalUrl);

            return Ok();
        }

        [HttpGet("{id}/personal-data")]
        [ProducesResponseType(typeof(ICollection<PersonalDataResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetUserPersonalData([FromRoute] int id, int? page, int? perPage, string sortOrder, ICollection<PersonalDataEnum> keys)
        {
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _personalDataService.GetUserPersonalDataAsync(id, pag, perPag, sortOrder, keys);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<ICollection<PersonalDataResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        [HttpGet("statistics-summary")]
        [ProducesResponseType(typeof(UserDataSummary), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetUsersSummary()
        {
            var result = await _userService.GetUsersSummaryAsync();
            return Ok(new ApiOkResponse(result));
        }

        private SendEmailRequest PrepareEmailBody(SendEmailRequest request)
        {
            var emailBody = EmailTemplateHelper.GetEmailTemplateString("ManualEmail.html", request.Subject, _env);
            var emailBodyIT = EmailTemplateHelper.GetEmailTemplateString("ManualEmail.html", request.SubjectIT, _env);
            var emailBodyEN = EmailTemplateHelper.GetEmailTemplateString("ManualEmail.html", request.SubjectEN, _env);

            request.Body = emailBody.ToManualEmail(request.Subject, request.Body);
            request.BodyIT = emailBodyIT.ToManualEmail(request.SubjectIT, request.BodyIT);
            request.BodyEN = emailBodyEN.ToManualEmail(request.SubjectEN, request.BodyEN);

            return request;
        }
    }
}
