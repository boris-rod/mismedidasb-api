﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Service.Hubs;
using MismeAPI.Services;
using MismeAPI.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace APITaxi.API.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private IHubContext<UserHub> _hub;

        public AccountController(IAccountService accountService, IMapper mapper, IEmailService emailService, IWebHostEnvironment env, IHubContext<UserHub> hub)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        }

        /// <summary>
        /// Register a user.
        /// </summary>
        /// <param name="register">
        /// Register request object. Include email used as username, password, full name and
        /// birthday. Valid password should have: 1- Non alphanumeric characters 2- Uppercase
        /// letters 3- Six characters minimun
        /// </param>
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Register([FromBody] SignUpRequest register)
        {
            var user = await _accountService.SignUpAsync(register);

            var resource = _env.ContentRootPath
                       + Path.DirectorySeparatorChar.ToString()
                       + "Templates"
                       + Path.DirectorySeparatorChar.ToString()
                       + "AccountActivation.html";
            var reader = new StreamReader(resource);
            var emailBody = reader.ReadToEnd().ToActivationAccountEmail(user.VerificationCode.ToString());
            reader.Dispose();

            var subject = "Activation Account";

            await _emailService.SendEmailResponseAsync(subject, emailBody, user.Email);
            var mapped = _mapper.Map<UserResponse>(user);
            await _hub.Clients.All.SendAsync(HubConstants.USER_REGISTERED, mapped);

            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Login a user.
        /// </summary>
        /// <param name="loginRequest">
        /// Login request object. Include email used as username and password.
        /// </param>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(UserResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Login([FromBody]  LoginRequest loginRequest)
        {
            var result = await _accountService.LoginAsync(loginRequest);
            HttpContext.Response.Headers["Authorization"] = "Bearer " + result.accessToken;
            HttpContext.Response.Headers["RefreshToken"] = result.refreshToken;
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "Authorization, RefreshToken";
            var user = _mapper.Map<UserResponse>(result.user);
            user.KCal = result.kcal;
            user.IMC = result.IMC;
            return Ok(new ApiOkResponse(user));
        }

        /// <summary>
        /// Logout a user. Requires authentication.
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Logout()
        {
            var accessToken = Request.Headers["Authorization"];
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
            await _accountService.LogoutAsync(int.Parse(userId), accessToken.ToString().Split("Bearer")[1].Trim());

            return Ok(new ApiOkResponse(null));
        }

        /// <summary>
        /// Logout a user. This will logout the user from all the devices by deleting all his
        /// tokens. This is triggered by the user using one of his tokens (the used in the device
        /// that triggers this action). Requires authentication.
        /// </summary>
        [Authorize]
        [HttpPost("global-logout")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GlobalLogout()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
            await _accountService.GlobalLogoutAsync(int.Parse(userId));

            return Ok(new ApiOkResponse(null));
        }

        /// <summary>
        /// Refresh token.
        /// </summary>
        /// <param name="refreshToken">
        /// Refresh token request object. Include old token and refresh token. This info will be
        /// used to validate the info against our database.
        /// </param>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refreshToken)
        {
            var principal = await _accountService.GetPrincipalFromExpiredTokenAsync(refreshToken.Token);

            await _accountService.GetRefreshTokenAsync(refreshToken, principal.Claims.Where(c => c.Type == ClaimTypes.UserData).First().Value); //retrieve the refresh token from a data store

            var result = await _accountService.GenerateNewTokensAsync(refreshToken.Token, refreshToken.RefreshToken);
            HttpContext.Response.Headers["Authorization"] = "Bearer " + result.accessToken;
            HttpContext.Response.Headers["RefreshToken"] = result.refreshToken;
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "Authorization, RefreshToken";

            return Ok(new ApiOkResponse(null));
        }

        /// <summary>
        /// Change Password. Requires authentication.
        /// </summary>
        /// <param name="changePassword">
        /// Change password request object. Include old password, new password, and confirm password.
        /// </param>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePassword)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
            await _accountService.ChangePasswordAsync(changePassword, int.Parse(userId));

            return Ok();
        }

        /// <summary>
        /// Validate token.
        /// </summary>
        /// <param name="validateToken">
        /// Refresh token request object. Include old token and refresh token. This info will be
        /// used to validate the info against our database.
        /// </param>
        [HttpPost("validate-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest validateToken)
        {
            var isValid = await _accountService.ValidateTokenAsync(validateToken.Token);
            if (isValid)
            {
                var principal = await _accountService.GetPrincipalFromExpiredTokenAsync(validateToken.Token);
                var userId = principal.Claims.Where(c => c.Type == ClaimTypes.UserData).First().Value;
                var user = await _accountService.GetUserAsync(int.Parse(userId));
                var mapped = _mapper.Map<UserResponse>(user);
                return Ok(new ApiOkResponse(mapped));
            }
            else
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Change Account Status. Requires authentication.
        /// </summary>
        /// <param name="changeAccountStatus">
        /// Change account status request object. Include identity and status to change.
        /// </param>
        [Authorize]
        [HttpPost("change-status")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ChangeAccountStatus([FromBody] ChangeAccountStatusRequest changeAccountStatus)
        {
            var loggedUser = User.GetUserIdFromToken();
            var user = await _accountService.ChangeAccountStatusAsync(changeAccountStatus, loggedUser);
            var mapped = _mapper.Map<UserResponse>(user);
            if (user.Status == StatusEnum.ACTIVE)
            {
                await _hub.Clients.All.SendAsync(HubConstants.USER_ACTIVATED, mapped);
            }
            else if (user.Status == StatusEnum.INACTIVE)
            {
                await _hub.Clients.All.SendAsync(HubConstants.USER_DISABLED, mapped);
            }

            return Ok();
        }

        /// <summary>
        /// Upload Avatar. Requires authentication.
        /// </summary>
        /// <param name="file">Avatar file.</param>
        [Authorize]
        [HttpPost("upload-avatar")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _accountService.UploadAvatarAsync(file, loggedUser);
            var user = _mapper.Map<UserResponse>(result);

            return Ok(new ApiOkResponse(user));
        }

        /// <summary>
        /// Forgot password. The user receive an email with a new password.
        /// </summary>
        /// <param name="email">User email.</param>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ForgotPassword([FromQuery]string email)
        {
            var newPass = await _accountService.ForgotPasswordAsync(email);

            var resource = _env.ContentRootPath
                       + Path.DirectorySeparatorChar.ToString()
                       + "Templates"
                       + Path.DirectorySeparatorChar.ToString()
                       + "ForgotPassword.html";
            var reader = new StreamReader(resource);
            var emailBody = reader.ReadToEnd().ToForgotPasswordEmail(newPass);
            reader.Dispose();

            var subject = "Password Reset";

            await _emailService.SendEmailResponseAsync(subject, emailBody, email);

            return Ok();
        }

        /// <summary>
        /// Activation Account.
        /// </summary>
        /// <param name="activation">
        /// Activation Account request object. Include email and verification code.
        /// </param>
        [AllowAnonymous]
        [HttpPost("activate-account")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ActivationAccount([FromBody] ActivationAccountRequest activation)
        {
            var result = await _accountService.ActivationAccountAsync(activation);
            var mapped = _mapper.Map<UserResponse>(result);
            await _hub.Clients.All.SendAsync(HubConstants.USER_ACTIVATED, mapped);
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Resend verification code.
        /// </summary>
        /// <param name="email">User email.</param>
        [AllowAnonymous]
        [HttpPost("resend-code")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResendVerificationCode(string email)
        {
            var code = await _accountService.ResendVerificationCodeAsync(email);
            var resource = _env.ContentRootPath
                       + Path.DirectorySeparatorChar.ToString()
                       + "Templates"
                       + Path.DirectorySeparatorChar.ToString()
                       + "AccountActivation.html";
            var reader = new StreamReader(resource);
            var emailBody = reader.ReadToEnd().ToActivationAccountEmail(code.ToString());
            reader.Dispose();

            var subject = "Resend Verification Code";

            await _emailService.SendEmailResponseAsync(subject, emailBody, email);

            return Ok();
        }

        /// <summary>
        /// Remove logged in user avatar. Requires authentication.
        /// </summary>

        [Authorize]
        [HttpPost("remove-avatar")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> RemoveAvatar()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _accountService.RemoveAvatarAsync(loggedUser);
            var user = _mapper.Map<UserResponse>(result);

            return Ok(new ApiOkResponse(user));
        }

        /// <summary>
        /// Get user profile. Requires authentication.
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserProfile()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _accountService.GetUserProfileUseAsync(loggedUser);
            var user = _mapper.Map<UserResponse>(result);

            return Ok(new ApiOkResponse(user));
        }
    }
}