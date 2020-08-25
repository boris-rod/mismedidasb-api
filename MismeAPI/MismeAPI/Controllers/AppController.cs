using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/app")]
    public class AppController : Controller
    {
        private readonly IAppService _appService;
        private readonly IMapper _mapper;

        public AppController(IAppService appService, IMapper mapper)
        {
            _appService = appService ?? throw new ArgumentNullException(nameof(appService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get app info.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AppResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAppInfo()
        {
            var result = await _appService.GetAppInfoAsync();

            var mapped = _mapper.Map<AppResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }
    }
}