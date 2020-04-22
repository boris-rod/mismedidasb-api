using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request.Reminder;
using MismeAPI.Common.DTO.Response.Reminder;
using MismeAPI.Service;
using MismeAPI.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/reminder")]
    public class ReminderController : Controller
    {
        private readonly IReminderService _reminderService;
        private readonly IMapper _mapper;

        public ReminderController(IReminderService reminderService, IMapper mapper)
        {
            _reminderService = reminderService ?? throw new ArgumentNullException(nameof(reminderService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get reminders. Only an admin can do this operation. Requires authentication.
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ReminderAdminResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRemindersAdmin()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _reminderService.GetRemindersAdminAsync(loggedUser);
            var mapped = _mapper.Map<IEnumerable<ReminderAdminResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Change a reminider i18n. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="reminderTranslationRequest">Reminder translation request object.</param>
        /// <param name="id">Reminder id.</param>
        [HttpPost("{id}/define-translation")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ReminderTranslation([FromRoute] int id, [FromBody]ReminderTranslationRequest reminderTranslationRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _reminderService.ChangeReminderTranslationAsync(loggedUser, reminderTranslationRequest, id);
            return Ok();
        }
    }
}