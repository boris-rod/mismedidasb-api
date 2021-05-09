using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Group;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Middlewares.Security;
using MismeAPI.Service;
using MismeAPI.Services;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/group")]
    public class GroupController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IGroupService _groupService;
        private readonly IEmailService _emailService;
        private IWebHostEnvironment _env;
        private IConfiguration _configuration;
        private IUserService _userService;
        private readonly IAuthorizationService _authorizationService;

        public GroupController(IMapper mapper, IGroupService groupService, IEmailService emailService, IWebHostEnvironment env, IConfiguration configuration,
            IUserService userService, IAuthorizationService authorizationService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        /// <summary>
        /// Get current user group. Require authentication
        /// </summary>
        /// <returns>Group</returns>
        [HttpGet]
        [Authorize(Roles = "GROUP_ADMIN,ADMIN")]
        [ProducesResponseType(typeof(GroupExtendedResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _groupService.GetCurrentUserGroupAsync(loggedUser);

            var mapped = _mapper.Map<GroupExtendedResponse>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Update current user group. Require authentication
        /// </summary>
        /// <returns>Group</returns>
        [HttpPut]
        [Authorize(Roles = "GROUP_ADMIN,ADMIN")]
        [ProducesResponseType(typeof(GroupExtendedResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update([FromBody] UpdateGroupRequest request)
        {
            var loggedUser = User.GetUserIdFromToken();
            var group = await _groupService.GetCurrentUserGroupAsync(loggedUser);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, group, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            var result = await _groupService.UpdateGroupLimitedAsync(group.Id, request);

            var mapped = _mapper.Map<GroupExtendedResponse>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Invite users to a group. Require authentication. Only the group admin cand perform this action
        /// </summary>
        /// <returns>Group</returns>
        [HttpPost("{id}/group-invitations")]
        [Authorize(Roles = "GROUP_ADMIN,ADMIN")]
        [ProducesResponseType(typeof(ICollection<GroupInviteActionResponse>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> InviteUsers([FromRoute] int id, [FromBody] GroupInviteActionRequest request)
        {
            var loggedUser = User.GetUserIdFromToken();
            var currentUser = await _userService.GetUserDevicesAsync(loggedUser);
            var group = await _groupService.GetGroupAsync(id);
            var groupName = group.Name;

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, group, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            var result = await _groupService.InviteUsersToGroupAsync(id, request.Emails);

            var fullName = currentUser.FullName;
            var iOSLink = _configuration.GetValue<string>("CustomSetting:ApkLinkIOS");
            var androidLink = _configuration.GetValue<string>("CustomSetting:ApkLinkAndroid");
            var adminUrl = _configuration.GetValue<string>("CustomSetting:AdminUrl");
            var acceptPath = _configuration.GetValue<string>("CustomSetting:AcceptGroupInvitationPath");
            var declinePath = _configuration.GetValue<string>("CustomSetting:DeclineInvitationPath");
            var acceptUrl = adminUrl + acceptPath + "/";
            var declineUrl = adminUrl + declinePath + "/";

            List<string> to;

            foreach (var invite in result.invitations)
            {
                var subject = "Invitación para unirte a Planifive";
                var emailBody = "";

                if (invite.User == null)
                {
                    to = new List<string> { invite.UserEmail };

                    emailBody = EmailTemplateHelper.GetEmailTemplateString("SendInvitation.html", subject, _env);
                    emailBody = emailBody.ToSendInvitationEmail(fullName, iOSLink, androidLink);

                    await _emailService.SendEmailResponseAsync(subject, emailBody, to);
                }
                else
                {
                    to = new List<string> { invite.User.Email };
                }

                subject = "Únete a un grupo en Planifive";
                var acceptUrlToken = acceptUrl + invite.SecurityToken;
                var declineUrlToken = declineUrl + invite.SecurityToken;

                emailBody = EmailTemplateHelper.GetEmailTemplateString("SendInvitationGroup.html", subject, _env);
                emailBody = emailBody.ToSendInvitationGroupEmail(fullName, groupName, acceptUrlToken, declineUrlToken);

                await _emailService.SendEmailResponseAsync(subject, emailBody, to);
            }

            return Created("Invitations", new ApiOkResponse(result.result));
        }

        /// <summary>
        /// Get group invitations. Requires authentication, only group admin can do it.
        /// </summary>
        /// <param name="id">group id</param>
        /// <param name="page">Page for pagination purposes.</param>
        /// <param name="perPage">How many items per page.</param>
        /// <param name="sortOrder">For sortering purposes.</param>
        /// <param name="search">Search by email</param>
        /// <param name="statuses">Filter result by status</param>
        /// <returns></returns>
        [HttpGet("{id}/group-invitations")]
        [Authorize(Roles = "GROUP_ADMIN,ADMIN")]
        [ProducesResponseType(typeof(ICollection<GroupInvitationResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetInvitations(int id, int? page, int? perPage, string sortOrder, string search, ICollection<StatusInvitationEnum> statuses)
        {
            // Resource permision handler
            var group = await _groupService.GetGroupAsync(id);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, group, Operations.Read);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _groupService.GetInvitationsAsync(id, pag, perPag, sortOrder, search, statuses);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<ICollection<GroupInvitationResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Remove an invitation of a group. Require authentication. Only the group admin cand
        /// perform this action
        /// </summary>
        /// <returns>Group</returns>
        [HttpDelete("{id}/group-invitations/{inviteId}")]
        [Authorize(Roles = "GROUP_ADMIN,ADMIN")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteInvitation([FromRoute] int id, [FromRoute] int inviteId)
        {
            // Resource permision handler
            var group = await _groupService.GetGroupAsync(id);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, group, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            await _groupService.DeleteGroupInvitationAsync(inviteId);

            return NoContent();
        }

        /// <summary>
        /// Get active users in a group. Requires authentication, only group admin can do it.
        /// </summary>
        /// <param name="id">group id</param>
        /// <param name="page">Page for pagination purposes.</param>
        /// <param name="perPage">How many items per page.</param>
        /// <param name="sortOrder">For sortering purposes.</param>
        /// <param name="search">Search string</param>
        /// <param name="statuses"></param>
        /// <returns></returns>
        [HttpGet("{id}/users")]
        [Authorize(Roles = "GROUP_ADMIN,ADMIN")]
        [ProducesResponseType(typeof(ICollection<UserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetUsers(int id, int? page, int? perPage, string sortOrder, string search, ICollection<StatusInvitationEnum> statuses)
        {
            // Resource permision handler
            var group = await _groupService.GetGroupAsync(id);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, group, Operations.Read);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _groupService.GetUsersAsync(id, pag, perPag, sortOrder, search);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<ICollection<UserResponse>>(result);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Accept an invitation to a group.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Group Invitation</returns>
        [AllowAnonymous]
        [HttpPatch("group-invitations/accept")]
        [ProducesResponseType(typeof(GroupInviteActionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AcceptInvitation(string token)
        {
            var result = await _groupService.UpdateGroupInvitationAsync(StatusInvitationEnum.ACCEPTED, token);
            var mapped = _mapper.Map<GroupInvitationResponse>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Decline an invitation to a group.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Group Invitation</returns>
        [AllowAnonymous]
        [HttpPatch("group-invitations/decline")]
        [ProducesResponseType(typeof(GroupInviteActionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeclineInvitation(string token)
        {
            var result = await _groupService.UpdateGroupInvitationAsync(StatusInvitationEnum.REJECTED, token);
            var mapped = _mapper.Map<GroupInvitationResponse>(result);

            return Ok(new ApiOkResponse(mapped));
        }
    }
}
