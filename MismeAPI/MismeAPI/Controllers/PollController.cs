using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Poll;
using MismeAPI.Common.DTO.Request.Tip;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.Reward;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Service;
using MismeAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/poll")]
    public class PollController : Controller
    {
        private readonly IPollService _pollService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IRewardHelper _rewardHelper;

        public PollController(IPollService pollService, IMapper mapper, IUserService userService, IRewardHelper rewardHelper)
        {
            _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _rewardHelper = rewardHelper ?? throw new ArgumentNullException(nameof(rewardHelper));
        }

        /// <summary>
        /// Get all polls definition. Requires authentication.
        /// </summary>
        /// <param name="conceptId">Concept id filter.</param>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PollResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll(int? conceptId)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            var concept = conceptId ?? -1;
            var result = await _pollService.GetAllPollsAsync(concept);
            var dict = await _pollService.GetLastAnsweredDictAsync(loggedUser);

            var mapped = _mapper.Map<IEnumerable<PollResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
                opt.Items["dict"] = dict;
            });
            mapped = mapped.OrderBy(m => m.Order);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get by id. Requires authentication.
        /// </summary>
        /// <param name="id">Poll id.</param>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(PollResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var result = await _pollService.GetPollByIdAsync(id);
            var mapped = _mapper.Map<PollResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Create a new poll. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="poll">Poll request object.</param>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(PollResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreatePollRequest poll)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _pollService.CreatePollAsync(loggedUser, poll);
            var mapped = _mapper.Map<PollResponse>(result);
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Update a poll. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="poll">Poll request object.</param>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(PollResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdatePollRequest poll)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _pollService.UpdatePollDataAsync(loggedUser, poll);
            var mapped = _mapper.Map<PollResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a poll. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Id of the poll.</param>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _pollService.DeletePollAsync(loggedUser, id);
            return Ok();
        }

        ///// <summary>
        ///// Set a poll result. Requires authentication.
        ///// </summary>
        ///// <param name="pollResult">Poll result request object.</param>
        //[HttpPost("set-result")]
        //[Authorize]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        //public async Task<IActionResult> SetPollResult([FromBody] SetPollResultRequest pollResult)
        //{
        //    var loggedUser = User.GetUserIdFromToken();
        //    await _pollService.SetPollResultAsync(loggedUser, pollResult);
        //    return Ok();
        //}

        /// <summary>
        /// Update a poll title. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Poll id.</param>
        /// <param name="title">Poll title.</param>
        [HttpPatch("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(PollResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromQuery] string title)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _pollService.UpdatePollTitleAsync(loggedUser, title, id);
            var mapped = _mapper.Map<PollResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Set a poll result based on question answers. Requires authentication.
        /// </summary>
        /// <param name="result">Poll result request object.</param>
        [HttpPost("set-poll-result")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> SetPollResultBasedRequest([FromBody] ListOfPollResultsRequest result)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            // Only 10 points per concept
            var alreadyAnswered = await _pollService.HasAnsweredConceptBeforeAsync(loggedUser, result);

            var message = await _pollService.SetPollResultByQuestionsAsync(loggedUser, result, language);

            /*Reward section*/
            RewardResponse mapped = null;
            if (!alreadyAnswered)
            {
                var answeredPolls = _pollService.GetAnsweredPolls(result);

                var pollId = answeredPolls.FirstOrDefault();
                mapped = await _rewardHelper.HandleRewardAsync(RewardCategoryEnum.POLL_ANSWERED, loggedUser, true, message, pollId);
            }
            /*#end reward section*/

            return Ok(new ApiOkRewardResponse(message, mapped));
        }

        /// <summary>
        /// Change a poll questions order. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="questionOrderRequest">Poll question order request object.</param>
        /// <param name="id">Poll id.</param>
        [HttpPost("{id}/questions-order")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> ChangePollQuestionsOrder([FromRoute] int id, [FromBody] QuestionOrderRequest questionOrderRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _pollService.ChangePollQuestionOrderAsync(loggedUser, questionOrderRequest, id);
            return Ok();
        }

        /// <summary>
        /// Change a poll to read-only and viceversa. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="pollReadOnlyRequest">Poll readonly request object.</param>
        /// <param name="id">Poll id.</param>
        [HttpPost("{id}/read-only")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ReadOnlyPoll([FromRoute] int id, [FromBody] PollReadOnlyRequest pollReadOnlyRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _pollService.ChangePollReadOnlyAsync(loggedUser, pollReadOnlyRequest, id);
            return Ok();
        }

        /// <summary>
        /// Add poll's tip. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="tipRequest">Tip request object.</param>
        [HttpPost("tips")]
        [Authorize]
        [ProducesResponseType(typeof(TipResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> AddTip([FromBody] AddTipRequest tipRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            var tip = await _pollService.AddTipRequestAsync(loggedUser, tipRequest);
            var mapped = _mapper.Map<TipResponse>(tip);
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a tip. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Id of the tip.</param>
        [HttpDelete("tips/delete/{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteTip([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _pollService.DeleteTipAsync(loggedUser, id);
            return Ok();
        }

        /// <summary>
        /// Update tip content. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Tip id.</param>
        /// <param name="content">Tip content.</param>
        [HttpPatch("tips/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(TipResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateTipContent([FromRoute] int id, [FromQuery] string content)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _pollService.UpdateTipContentAsync(loggedUser, content, id);
            var mapped = _mapper.Map<TipResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Activate a tip. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Tip id.</param>
        /// <param name="pollId">Poll id.</param>
        /// <param name="position">Position.</param>
        [HttpPost("tips/activate/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(TipResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> ActivateTipContent([FromRoute] int id, [FromQuery] int pollId, [FromQuery] int position)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _pollService.ActivateTipAsync(loggedUser, id, pollId, position);
            return Ok();
        }

        /// <summary>
        /// Get all polls definition for translation. Only an admin can do this operation. Requires authentication.
        /// </summary>
        [HttpGet("admin")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PollAdminResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<ApiResponse>), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetAllPollsForTranslate(int? conceptId)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _pollService.GetAllPollsAdminAsync(loggedUser);

            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            //var mapped = _mapper.Map<IEnumerable<PollAdminResponse>>(result);

            var mapped = _mapper.Map<IEnumerable<PollAdminResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });

            mapped = mapped.OrderBy(m => m.Order);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Change a poll i18n. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="pollTranslationRequest">Poll translation request object.</param>
        /// <param name="id">Poll id.</param>
        [HttpPost("{id}/define-translation")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> PollTranslation([FromRoute] int id, [FromBody] PollTranslationRequest pollTranslationRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _pollService.ChangePollTranslationAsync(loggedUser, pollTranslationRequest, id);
            return Ok();
        }
    }
}
