using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request.ContactUs;
using MismeAPI.Common.DTO.Response.ContactUs;
using MismeAPI.Service;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/contact-us")]
    public class ContactUsController : Controller
    {
        private readonly IContactUsService _contactUsService;
        private readonly IMapper _mapper;

        public ContactUsController(IContactUsService contactUsService, IMapper mapper)
        {
            _contactUsService = contactUsService ?? throw new ArgumentNullException(nameof(contactUsService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Add a contact us. Requires authentication.
        /// </summary>
        /// <param name="contactUs">Contact us request object.</param>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ContactUsResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddContactUs([FromBody] ContactUsRequest contactUs)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _contactUsService.CreateContactUsAsync(loggedUser, contactUs);
            var mapped = _mapper.Map<ContactUsResponse>(result);
            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Mark as read-unread. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Contact us id.</param>
        /// <param name="read">Read param.</param>
        [HttpPost("{id}/read-status")]
        [Authorize]
        [ProducesResponseType(typeof(ContactUsResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ReadStatusContactUs([FromRoute] int id, [FromQuery] bool read)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _contactUsService.ChangeReadStatusAsync(loggedUser, id, read);
            var mapped = _mapper.Map<ContactUsResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Mark as important. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">Contact us id.</param>
        /// <param name="important">Important param.</param>
        [HttpPost("{id}/important-status")]
        [Authorize]
        [ProducesResponseType(typeof(ContactUsResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ImportantStatusContactUs([FromRoute] int id, [FromQuery] bool important)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _contactUsService.ChangeImportantStatusAsync(loggedUser, id, important);
            var mapped = _mapper.Map<ContactUsResponse>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get all contact us, it is allowed to use filtering. Requires authentication. Admin access.
        /// </summary>
        /// <param name="page">Page for pagination purposes.</param>
        /// <param name="perPage">How many issues per page.</param>
        /// <param name="sortOrder">For sortering purposes.</param>
        /// <param name="priorityFilter">For filtering by priority. 0- Normal, 1- Important</param>
        /// <param name="readFilter">For filtering by read status. 0- Unread, 1- Read</param>
        /// <param name="search">For searching purposes.</param>
        [HttpGet()]
        [ProducesResponseType(typeof(IEnumerable<ContactUsResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetUsers(int? page, int? perPage, string sortOrder, string search, int? priorityFilter, int? readFilter)
        {
            var loggedUser = User.GetUserIdFromToken();
            var pag = page ?? 1;
            var perPag = perPage ?? 10;
            var prioriF = priorityFilter ?? -1;
            var readF = readFilter ?? -1;

            var result = await _contactUsService.GetContactsAsync(loggedUser, pag, perPag, sortOrder, prioriF, readF, search);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<ContactUsResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }
    }
}