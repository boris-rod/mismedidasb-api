using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
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
            var mapped = _mapper.Map<UserPersonalDataResponse>(result);
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
            var mapped = _mapper.Map<IEnumerable<UserPersonalDataResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get user's current personal datas. Requires authentication.
        /// </summary>
        /// <param name="userId">Selected user id. If null, then the logged in user is used.</param>
        [HttpGet("current-datas")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<UserPersonalDataResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserCurrentPersonalDatas(int? userId)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _pDataService.GetUserCurrentPersonalDatasAsync(userId ?? loggedUser);
            var mapped = _mapper.Map<IEnumerable<UserPersonalDataResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Create a new personal data variable. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="personalData">Personal data request object.</param>
        [HttpPost]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreatePersonalDataRequest personalData)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _pDataService.CreatePersonalDataAsync(loggedUser, personalData);
            var mapped = _mapper.Map<PersonalDataResponse>(result);
            return Created("", new ApiOkResponse(mapped));
        }
    }
}