using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.Common.DTO.Request.Device;
using MismeAPI.Service;
using MismeAPI.Utils;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/device")]
    public class DeviceController : Controller
    {
        private readonly IDeviceService _deviceService;
        private readonly IMapper _mapper;

        public DeviceController(IDeviceService deviceService, IMapper mapper)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Set a device token. Requires authentication.
        /// </summary>
        /// <param name="device">Device request object.</param>
        [HttpPost]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<IActionResult> SetDevice([FromBody]AddDeviceRequest device)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _deviceService.CreateOrUpdateDeviceAsync(loggedUser, device);
            return Ok();
        }
    }
}