using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/concept")]
    public class ConceptController : Controller
    {
        private readonly IConceptService _conceptService;
        private readonly IPollService _pollService;
        private readonly IMapper _mapper;

        public ConceptController(IConceptService conceptService, IPollService pollService, IMapper mapper)
        {
            _conceptService = conceptService ?? throw new ArgumentNullException(nameof(conceptService));
            _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get concepts. Requires authentication.
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ConceptResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetConcepts()
        {
            var result = await _conceptService.GetConceptsAsync();
            var mapped = _mapper.Map<IEnumerable<ConceptResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get all polls definition by concept id. Requires authentication.
        /// </summary>
        [HttpGet("{id}/polls")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PollResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll([FromRoute]int id)
        {
            var result = await _pollService.GetAllPollsByConceptAsync(id);
            var mapped = _mapper.Map<IEnumerable<PollResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }
    }
}