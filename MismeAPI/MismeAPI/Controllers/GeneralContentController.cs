using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response.GeneralContent;
using MismeAPI.Service;
using MismeAPI.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/general-content")]
    public class GeneralContentController : Controller
    {
        private readonly IGeneralContentService _generalContentService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GeneralContentController(IGeneralContentService generalContentService, IMapper mapper, IUserService userService)
        {
            _generalContentService = generalContentService ?? throw new ArgumentNullException(nameof(generalContentService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get general content by type. Requires authentication.
        /// </summary>
        /// <param name="contentType">Content type.</param>
        [HttpGet("by-type")]
        [Authorize]
        [ProducesResponseType(typeof(GeneralContentResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetContent([FromQuery] int contentType)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var result = await _generalContentService.GetGeneralContentsByTypeAsync(contentType);
            var mapped = _mapper.Map<GeneralContentResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get general contents. Only an admin can do this operation. Requires authentication.
        /// </summary>
        [HttpGet("admin")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<GeneralContentAdminResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetContentAdmin()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _generalContentService.GetGeneralContentsAdminAsync(loggedUser);
            var mapped = _mapper.Map<IEnumerable<GeneralContentAdminResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Change a content i18n. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="contentTranslationRequest">General content translation request object.</param>
        /// <param name="id">Content id.</param>
        [HttpPost("{id}/define-translation")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> TipTranslation([FromRoute] int id, [FromBody]GeneralContentTranslationRequest contentTranslationRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _generalContentService.ChangeContentTranslationAsync(loggedUser, contentTranslationRequest, id);
            return Ok();
        }

        /// <summary>
        /// Accept terms and conditions.
        /// </summary>
        [HttpPost("accept-terms-conditions")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> TermsAndConditions()
        {
            var loggedUser = User.GetUserIdFromToken();
            await _generalContentService.AcceptTermsAndConditionsAsync(loggedUser);
            return Ok();
        }
    }
}