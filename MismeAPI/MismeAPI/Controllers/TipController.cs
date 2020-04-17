using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request.Tip;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
using MismeAPI.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/tip")]
    public class TipController : Controller
    {
        private readonly ITipService _tipService;
        private readonly IMapper _mapper;

        public TipController(ITipService tipService, IMapper mapper)
        {
            _tipService = tipService ?? throw new ArgumentNullException(nameof(tipService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get tips. Only an admin can do this operation. Requires authentication.
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<TipAdminResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTipsAdmin()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _tipService.GetTipsAdminAsync(loggedUser);
            var mapped = _mapper.Map<IEnumerable<TipAdminResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Change a tip i18n. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="tipTranslationRequest">Tip translation request object.</param>
        /// <param name="id">Tip id.</param>
        [HttpPost("{id}/define-translation")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> TipTranslation([FromRoute] int id, [FromBody]TipTranslationRequest tipTranslationRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _tipService.ChangeTipTranslationAsync(loggedUser, tipTranslationRequest, id);
            return Ok();
        }
    }
}