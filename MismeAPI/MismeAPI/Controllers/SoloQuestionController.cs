using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response.SoloQuestion;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.Entities.NonDatabase;
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
    [Route("api/solo-question")]
    public class SoloQuestionController : Controller
    {
        private readonly ISoloQuestionService _soloQuestionService;
        private readonly IMapper _mapper;
        private IRewardHelper _rewardHelper;

        public SoloQuestionController(ISoloQuestionService soloQuestionService, IUserService userService,
            IMapper mapper, IRewardHelper rewardHelper)
        {
            _soloQuestionService = soloQuestionService ?? throw new ArgumentNullException(nameof(soloQuestionService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _rewardHelper = rewardHelper ?? throw new ArgumentNullException(nameof(rewardHelper));
        }

        /// <summary>
        /// Get current user available questions and possible answers for today. Require authentication
        /// </summary>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of questions to be displayed per page. 10 by default.</param>
        /// <returns>Active cut points</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SoloQuestionResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index(int? page, int? perPage)
        {
            var loggedUser = User.GetUserIdFromToken();

            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _soloQuestionService.GetUserQuestionsForTodayAsync(loggedUser, pag, perPag);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";
            var mapped = _mapper.Map<IEnumerable<SoloQuestionResponse>>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Answer a question. Requires authentication.
        /// </summary>
        /// <param name="answer">User answer request</param>
        [HttpPost("user-answer")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> SetUserAnswer([FromBody] CreateUserSoloAnswerRequest answer)
        {
            var loggedUser = User.GetUserIdFromToken();
            var userAnswer = await _soloQuestionService.SetUserAnswerAsync(loggedUser, answer);

            var mapped = _mapper.Map<UserSoloAnswerResponse>(userAnswer);

            var rewardResponse = await _rewardHelper.HandleRewardAsync(RewardCategoryEnum.SOLO_QUESTION_ANSWERED, loggedUser, true, mapped, null);

            return Ok(new ApiOkRewardResponse(null, rewardResponse));
        }

        /// <summary>
        /// Get answer summary-statistics by user id. Requires authentication
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="lastNDays">
        /// Get the last N days of summary. If no value is provided returns the last 7 days
        /// </param>
        [HttpGet("{userId}/extended")]
        [ProducesResponseType(typeof(ExtendedUserStatistics), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetExtendedUsersStatisticsByUserId([FromRoute]int userId, int lastNDays)
        {
            if (lastNDays == 0)
                lastNDays = 7;

            var result = await _soloQuestionService.GetuserSumaryAsync(userId, lastNDays);

            return Ok(new ApiOkResponse(result));
        }
    }
}
