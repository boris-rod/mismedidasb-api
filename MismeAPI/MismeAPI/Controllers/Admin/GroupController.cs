using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Group;
using MismeAPI.Service;
using MismeAPI.Services;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers.Admin
{
    [Route("api/admin/group")]
    [Authorize(Roles = "ADMIN")]
    public class GroupController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IGroupService _groupService;
        private readonly IEmailService _emailService;
        private IWebHostEnvironment _env;
        private IConfiguration _configuration;

        public GroupController(IMapper mapper, IGroupService groupService, IEmailService emailService, IWebHostEnvironment env, IConfiguration configuration)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get groups. Requires Admin authentication
        /// </summary>
        /// <param name="page">Page for pagination purposes.</param>
        /// <param name="perPage">How many items per page.</param>
        /// <param name="sortOrder">For sortering purposes.</param>
        /// <param name="search">Search groups by name or admin email</param>
        /// <param name="isActive">Filter groups by its active status</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ICollection<GroupResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Index(int? page, int? perPage, string sortOrder, string search, bool? isActive)
        {
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _groupService.GetGroupsAsync(pag, perPag, sortOrder, search, isActive);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<ICollection<GroupResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get a group. Require authentication. Admin access requires
        /// </summary>
        /// <returns>Group</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GroupExtendedResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _groupService.GetGroupAsync(id);

            var mapped = _mapper.Map<GroupExtendedResponse>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Create a group. Requires authentication. Only Admin access
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(GroupExtendedResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
        {
            var result = await _groupService.CreateGroupAsync(request);

            var subject = "Bienvenido al grupo " + result.Group?.Name;
            var adminUrl = _configuration.GetValue<string>("CustomSetting:AdminUrl");
            var isNewUser = !string.IsNullOrEmpty(result.GeneratedPassword);
            var emailString = "";
            var to = new List<string> { request.AdminEmail };

            if (isNewUser)
            {
                emailString = EmailTemplateHelper.GetEmailTemplateString("SendInvitationGroupAdmin.html", subject, _env);
            }
            else
            {
                emailString = EmailTemplateHelper.GetEmailTemplateString("SendInvitationGroupAdminExistingUser.html", subject, _env);
            }

            emailString = emailString.ToGroupAdminInviteEmail(isNewUser, adminUrl, request.AdminEmail, result.GeneratedPassword);
            await _emailService.SendEmailResponseAsync(subject, emailString, to);

            var mapped = _mapper.Map<GroupExtendedResponse>(result.Group);

            return Created("Group", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Update a group. Requires authentication. Only Admin access
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(GroupExtendedResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateGroupRequest request)
        {
            var newAdmin = request.AdminEmail;
            var result = await _groupService.UpdateGroupAsync(id, request);
            var isAdminUpdate = result.Group?.AdminEmail != newAdmin;

            if (isAdminUpdate)
            {
                var subject = "Bienvenido al grupo " + result.Group?.Name;
                var adminUrl = _configuration.GetValue<string>("CustomSetting:AdminUrl");
                var isNewUser = string.IsNullOrEmpty(result.GeneratedPassword);
                var emailString = "";
                var to = new List<string> { request.AdminEmail };

                if (isNewUser)
                {
                    emailString = EmailTemplateHelper.GetEmailTemplateString("SendInvitationGroupAdmin.html", subject, _env);
                }
                else
                {
                    emailString = EmailTemplateHelper.GetEmailTemplateString("SendInvitationGroupAdminExistingUser.html", subject, _env);
                }

                emailString = emailString.ToGroupAdminInviteEmail(isNewUser, adminUrl, request.AdminEmail, result.GeneratedPassword);
                await _emailService.SendEmailResponseAsync(subject, emailString, to);
            }

            var mapped = _mapper.Map<GroupExtendedResponse>(result.Group);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Activate a group. Requires authentication. Only Admin access
        /// </summary>
        [HttpPatch("{id}/active")]
        [ProducesResponseType(typeof(GroupExtendedResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Active(int id)
        {
            var result = await _groupService.UpdateGroupActiveStatusAsync(id, true);

            var mapped = _mapper.Map<GroupExtendedResponse>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Deactivate a group. Requires authentication. Only Admin access
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(typeof(GroupExtendedResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _groupService.UpdateGroupActiveStatusAsync(id, false);

            var mapped = _mapper.Map<GroupExtendedResponse>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a group. Requires authentication. Only Admin access
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(GroupExtendedResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            await _groupService.DeleteGroupAsync(id);

            return NoContent();
        }
    }
}
