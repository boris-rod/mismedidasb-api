using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request.Answer;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
using MismeAPI.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/answer")]
    public class AnswerController : Controller
    {
        private readonly IAnswerService _answerService;
        private readonly IMapper _mapper;

        public AnswerController(IAnswerService answerService, IMapper mapper)
        {
            _answerService = answerService ?? throw new ArgumentNullException(nameof(answerService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get answers by question. Requires authentication.
        /// </summary>
        /// <param name="questionId">Question id.</param>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<AnswerResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAnswersByQuestion([FromQuery]int questionId)
        {
            var result = await _answerService.GetAnswersByQuestionIdAsync(questionId);
            var mapped = _mapper.Map<IEnumerable<AnswerResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Answer a question. Requires authentication.
        /// </summary>
        /// <param name="id">Selected answer id.</param>
        [HttpPost("{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> SetUserAnswer([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _answerService.AnswerAQuestionAsync(loggedUser, id);
            return Ok(new ApiOkResponse(null));
        }

        /// <summary>
        /// Get answers for translate. Only an admin can do this operation. Requires authentication.
        /// </summary>
        [HttpGet("admin")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<AnswerAdminResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAnswerAdmin()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _answerService.GetAnswersAdminAsync(loggedUser);
            var mapped = _mapper.Map<IEnumerable<AnswerAdminResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Change a answer i18n. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="answerTranslationRequest">Answer translation request object.</param>
        /// <param name="id">Answer id.</param>
        [HttpPost("{id}/define-translation")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> QuestionTranslation([FromRoute] int id, [FromBody]AnswerTranslationRequest answerTranslationRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _answerService.ChangeAnswerTranslationAsync(loggedUser, answerTranslationRequest, id);
            return Ok();
        }
    }
}