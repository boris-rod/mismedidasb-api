using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
using MismeAPI.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
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
    }
}