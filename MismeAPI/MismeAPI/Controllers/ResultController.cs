using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request.Result;
using MismeAPI.Common.DTO.Response.Result;
using MismeAPI.Service;
using MismeAPI.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/result")]
    public class ResultController : Controller
    {
        private readonly IResultService _resultService;
        private readonly IMapper _mapper;

        public ResultController(IResultService resultService, IMapper mapper)
        {
            _resultService = resultService ?? throw new ArgumentNullException(nameof(resultService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get results. Only an admin can do this operation. Requires authentication.
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ResultAdminResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetResultsAdmin()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _resultService.GetResultsAdminAsync(loggedUser);
            var mapped = _mapper.Map<IEnumerable<ResultAdminResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Change a result i18n. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="resultTranslationRequest">Result translation request object.</param>
        /// <param name="id">Result id.</param>
        [HttpPost("{id}/define-translation")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ResultsTranslation([FromRoute] int id, [FromBody]ResultTranslationRequest resultTranslationRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _resultService.ChangeResultTranslationAsync(loggedUser, resultTranslationRequest, id);
            return Ok();
        }
    }
}