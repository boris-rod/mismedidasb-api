using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Response.Settings;
using MismeAPI.Service;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/setting")]
    public class SettingController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly IMapper _mapper;

        public SettingController(ISettingService settingService, IMapper mapper)
        {
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get settings. Requires authentication.
        /// </summary>
        [Authorize]
        [HttpGet()]
        [ProducesResponseType(typeof(IEnumerable<ListSettingResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSettings()
        {
            var result = await _settingService.GetSettingsAsync();
            var mapped = _mapper.Map<IEnumerable<ListSettingResponse>>(result);

            return Ok(new ApiOkResponse(mapped));
        }
    }
}