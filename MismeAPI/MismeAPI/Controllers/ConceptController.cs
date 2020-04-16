using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request.Concept;
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
        /// Get concepts. Only an admin can do this operation. Requires authentication.
        /// </summary>
        [HttpGet("admin")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ConceptAdminResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetConceptsAdmin()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _conceptService.GetConceptsAdminAsync(loggedUser);
            var mapped = _mapper.Map<IEnumerable<ConceptAdminResponse>>(result);
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
            mapped = mapped.OrderBy(p => p.Order);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Add a concept. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="concept">Concept request object.</param>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ConceptResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddConcept([FromBody]AddConceptRequest concept)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _conceptService.AddConceptAsync(loggedUser, concept);
            var mapped = _mapper.Map<ConceptResponse>(result);
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a concept. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Concept id to delete.</param>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteConcept([FromRoute]int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _conceptService.DeleteConceptAsync(loggedUser, id);
            return Ok();
        }

        /// <summary>
        /// Edit a concept. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="concept">Concept request object.</param>
        /// <param name="id">Concept id to update.</param>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ConceptResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> EditConcept([FromRoute]int id, UpdateConceptRequest concept)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _conceptService.EditConceptAsync(loggedUser, concept, id);
            var mapped = _mapper.Map<ConceptResponse>(result);
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Change a concept polls order. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="pollOrderRequest">Concept poll order request object.</param>
        /// <param name="id">Concept id.</param>
        [HttpPost("{id}/polls-order")]
        [Authorize]
        [ProducesResponseType(typeof(ConceptResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> ChangeConceptPollOrder([FromRoute] int id, [FromBody]PollOrderRequest pollOrderRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _conceptService.ChangeConceptPollOrderAsync(loggedUser, pollOrderRequest, id);
            return Ok();
        }

        /// <summary>
        /// Change a concept i18n. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="concetpTranslationRequest">Concept ttranslation request object.</param>
        /// <param name="id">Concept id.</param>
        [HttpPost("{id}/define-translation")]
        [Authorize]
        [ProducesResponseType(typeof(ConceptResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ConceptTranslation([FromRoute] int id, [FromBody]ConceptTranslationRequest concetpTranslationRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _conceptService.ChangeConceptTranslationAsync(loggedUser, concetpTranslationRequest, id);
            return Ok();
        }
    }
}