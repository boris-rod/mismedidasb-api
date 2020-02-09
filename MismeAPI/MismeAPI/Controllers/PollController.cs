using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
using MismeAPI.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/poll")]
    public class PollController : Controller
    {
        private readonly IPollService _pollService;
        private readonly IMapper _mapper;

        public PollController(IPollService pollService, IMapper mapper)
        {
            _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all polls definition. Requires authentication.
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PollResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _pollService.GetAllPollsAsync();
            var mapped = _mapper.Map<IEnumerable<PollResponse>>(result);
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
        public async Task<IActionResult> GetById([FromRoute]int id)
        {
            var result = await _pollService.GetPollByIdAsync(id);
            var mapped = _mapper.Map<PollResponse>(result);
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
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _pollService.DeletePollAsync(loggedUser, id);
            return Ok();
        }
    }
}