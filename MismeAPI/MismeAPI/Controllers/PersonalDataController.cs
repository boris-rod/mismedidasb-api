using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Service;
using MismeAPI.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/personal-data")]
    public class PersonalDataController : Controller
    {
        private readonly IPersonalDataService _pDataService;
        private readonly IMapper _mapper;

        public PersonalDataController(IPersonalDataService pDataService, IMapper mapper)
        {
            _pDataService = pDataService ?? throw new ArgumentNullException(nameof(pDataService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all personal data variables. Requires authentication.
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PersonalDataResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _pDataService.GetPersonalDataVariablesAsync();
            var mapped = _mapper.Map<IEnumerable<PersonalDataResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get personal data variable by id. Requires authentication.
        /// </summary>
        /// <param name="id">Personal data id.</param>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(PersonalDataResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById([FromRoute]int id)
        {
            var result = await _pDataService.GetPersonalDataByIdAsync(id);
            var mapped = _mapper.Map<PersonalDataResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get user's current personal data variable by id. Requires authentication.
        /// </summary>
        /// <param name="id">Personal data id.</param>
        /// <param name="userId">Selected user id. If null, then the logged in user is used.</param>
        [HttpGet("{id}/current")]
        [Authorize]
        [ProducesResponseType(typeof(UserPersonalDataResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetUserPersonalDataById([FromRoute]int id, int? userId)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _pDataService.GetUserPersonalDataByIdAsync(id, userId ?? loggedUser);
            var mapped = _mapper.Map<PersonalDataResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get user's historical personal data variable by id. Requires authentication.
        /// </summary>
        /// <param name="id">Personal data id.</param>
        /// <param name="userId">Selected user id. If null, then the logged in user is used.</param>
        [HttpGet("{id}/historical")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<UserPersonalDataResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetHistoricalUserPersonalDataById([FromRoute]int id, int? userId)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _pDataService.GetHistoricalUserPersonalDataByIdAsync(id, userId ?? loggedUser);
            var mapped = _mapper.Map<IEnumerable<PersonalDataResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }
    }
}