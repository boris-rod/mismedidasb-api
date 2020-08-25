using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Request.CutPoints;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.CutPoint;
using MismeAPI.Service;
using MismeAPI.Services;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/user-referral")]
    [Authorize]
    public class UserReferralController : Controller
    {
        private readonly IUserReferralService _userReferralService;
        private readonly IMapper _mapper;
        private IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UserReferralController(IUserReferralService userReferralService, IMapper mapper, IEmailService emailService, IWebHostEnvironment env, IConfiguration configuration)
        {
            _userReferralService = userReferralService ?? throw new ArgumentNullException(nameof(userReferralService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Add a list of referral by sending an email invitation. Requires authentication.
        /// </summary>
        /// <param name="request">List of referral to be invited.</param>
        [HttpPost]
        [ProducesResponseType(typeof(UserReferralResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddReferrals([FromBody]IEnumerable<CreateUserReferralRequest> request)
        {
            var loggedUser = User.GetUserIdFromToken();

            var result = await _userReferralService.CreateReferralsAsync(loggedUser, request);

            if (result.Count() > 0)
            {
                var to = new List<string>();
                var fullName = result.First().User.FullName;

                foreach (var item in result)
                {
                    to.Add(item.Email);
                }
                var resource = _env.ContentRootPath
                           + Path.DirectorySeparatorChar.ToString()
                           + "Templates"
                           + Path.DirectorySeparatorChar.ToString()
                           + "SendInvitation.html";
                var reader = new StreamReader(resource);
                var iOSLink = _configuration.GetValue<string>("CustomSetting:ApkLinkIOS");
                var androidLink = _configuration.GetValue<string>("CustomSetting:ApkLinkAndroid");

                var emailBody = reader.ReadToEnd().ToSendInvitationEmail(fullName, iOSLink, androidLink);
                reader.Dispose();

                var subject = "PlaniFive Invitation";

                /*Commeting the try catch due to there my be different exceptions that doesnt mean that all users where not notified*/
                //try
                //{
                await _emailService.SendEmailResponseAsync(subject, emailBody, to);
                //}
                //catch (Exception e)
                //{
                //await _userReferralService.RemoveReferralsAsync(loggedUser, result);
                //throw e;
                //}
            }

            var mapped = _mapper.Map<List<UserReferralResponse>>(result);

            return Created("", new ApiOkResponse(mapped));
        }
    }
}
