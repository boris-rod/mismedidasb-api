using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Group;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Service;
using MismeAPI.Services;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers.Admin
{
    [Route("api/admin/group")]
    [Authorize(Roles = "ADMIN")]
    public class GroupController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IGroupService _groupService;
        private readonly IEmailService _emailService;
        private IWebHostEnvironment _env;
        private IConfiguration _configuration;

        public GroupController(IUserService userService, IMapper mapper, INotificationService notificationService,
            IGroupService groupService, IEmailService emailService, IWebHostEnvironment env, IConfiguration configuration)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
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
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(GroupResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Index(int? page, int? perPage, string sortOrder)
        {
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _groupService.GetGroupsAsync(pag, perPag, sortOrder);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<GroupResponse>(result);
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
        /// Give a reward of coins to one or many users. Requires authentication. Only Admin access
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(GroupExtendedResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
        {
            var result = await _groupService.CreateGroupAsync(request);

            var subject = "Welcome to group " + result.Group?.Name;
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

            var mapped = _mapper.Map<GroupExtendedResponse>(result.Group);

            return Created("Group", new ApiOkResponse(mapped));
        }

        private SendEmailRequest PrepareEmailBody(SendEmailRequest request)
        {
            var resource = _env.ContentRootPath
                       + Path.DirectorySeparatorChar.ToString()
                       + "Templates"
                       + Path.DirectorySeparatorChar.ToString()
                       + "ManualEmail.html";
            var reader = new StreamReader(resource);

            var stringTemplate = reader.ReadToEnd();

            request.Body = stringTemplate.ToManualEmail(request.Subject, request.Body);
            request.BodyIT = stringTemplate.ToManualEmail(request.SubjectIT, request.BodyIT);
            request.BodyEN = stringTemplate.ToManualEmail(request.SubjectEN, request.BodyEN);

            reader.Dispose();

            return request;
        }
    }
}
