using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.Question;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
using MismeAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/question")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IMapper _mapper;

        public QuestionController(IQuestionService questionService, IMapper mapper)
        {
            _questionService = questionService ?? throw new ArgumentNullException(nameof(questionService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get questions by poll. Requires authentication.
        /// </summary>
        /// <param name="pollId">Poll id.</param>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<QuestionResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetQuestionsByPoll([FromQuery]int pollId)
        {
            var result = await _questionService.GetQuestionsByPollIdAsync(pollId);
            var mapped = _mapper.Map<IEnumerable<QuestionResponse>>(result);
            mapped = mapped.OrderBy(m => m.Order);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get by id. Requires authentication.
        /// </summary>
        /// <param name="id">Question id.</param>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(QuestionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById([FromRoute]int id)
        {
            var result = await _questionService.GetQuestionByIdAsync(id);
            var mapped = _mapper.Map<QuestionResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Add a question to a poll. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="question">Question request object.</param>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(QuestionResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> AddQuestion([FromBody] CreateQuestionRequest question)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _questionService.CreateQuestionAsync(loggedUser, question);
            var mapped = _mapper.Map<QuestionResponse>(result);
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Update a question. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="question">Question request object.</param>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(QuestionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdateQuestionRequest question)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _questionService.UpdateQuestionAsync(loggedUser, question);
            var mapped = _mapper.Map<QuestionResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a question. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Id of the question.</param>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _questionService.DeleteQuestionAsync(loggedUser, id);
            return Ok();
        }

        /// <summary>
        /// Update a question title. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="title">Question title.</param>
        /// <param name="id">Question id.</param>
        [HttpPatch("{id}/change-title")]
        [Authorize]
        [ProducesResponseType(typeof(QuestionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateTitle([FromRoute] int id, [FromQuery] string title)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _questionService.UpdateQuestionTitleAsync(loggedUser, id, title);
            var mapped = _mapper.Map<QuestionResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Add or update question with its answers. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="question">Question request object.</param>
        [HttpPost("add-or-update")]
        [Authorize]
        [ProducesResponseType(typeof(QuestionResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> AddQuestionWithAnswers([FromBody] AddOrUpdateQuestionWithAnswersRequest question)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _questionService.AddOrUpdateQuestionWithAnswersAsync(loggedUser, question);
            var mapped = _mapper.Map<QuestionResponse>(result);
            return Created("", new ApiOkResponse(mapped));
        }
    }
}