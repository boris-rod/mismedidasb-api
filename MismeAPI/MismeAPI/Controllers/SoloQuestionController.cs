using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Answer;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.SoloQuestion;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Service;
using MismeAPI.Utils;
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
    }
}
