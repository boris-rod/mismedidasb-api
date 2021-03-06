﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.Settings;
using MismeAPI.Common.DTO.Response.User;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Service;
using MismeAPI.Service.Hubs;
using MismeAPI.Service.Utils;
using MismeAPI.Services;
using MismeAPI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private IHubContext<UserHub> _hub;
        private IUserReferralService _userReferralService;
        private IRewardHelper _rewardHelper;
        private IUserService _userService;
        private IProfileHelthHelper _profileHelthHelper;

        public AccountController(IAccountService accountService, IMapper mapper, IEmailService emailService, IWebHostEnvironment env,
            IHubContext<UserHub> hub, IUserReferralService userReferralService, IRewardHelper rewardHelper, IUserService userService, IProfileHelthHelper profileHelthHelper)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
            _userReferralService = userReferralService ?? throw new ArgumentNullException(nameof(userReferralService));
            _rewardHelper = rewardHelper ?? throw new ArgumentNullException(nameof(rewardHelper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _profileHelthHelper = profileHelthHelper ?? throw new ArgumentNullException(nameof(profileHelthHelper));
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

            var subject = "Activar su Cuenta";
            var emailBody = EmailTemplateHelper.GetEmailTemplateString("AccountActivation.html", "Código de Activación", _env);
            emailBody = emailBody.ToActivationAccountEmail(user.VerificationCode.ToString());
            var to = new List<string> { user.Email };

            await _emailService.SendEmailResponseAsync(subject, emailBody, to);

            var mapped = _mapper.Map<UserResponse>(user);
            await _hub.Clients.All.SendAsync(HubConstants.USER_REGISTERED, mapped);

            var referral = await _userReferralService.SetReferralUserAsync(user);
            if (referral != null)
            {
                var mapped2 = _mapper.Map<UserReferralResponse>(referral);
                var us = await _userService.GetUserDevicesAsync(referral.UserId);
                await _rewardHelper.HandleRewardAsync(RewardCategoryEnum.NEW_REFERAL, referral.UserId, true, mapped2, null, NotificationTypeEnum.FIREBASE, us?.Devices);
            }

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
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var result = await _accountService.LoginAsync(loginRequest);
            HttpContext.Response.Headers["Authorization"] = "Bearer " + result.accessToken;
            HttpContext.Response.Headers["RefreshToken"] = result.refreshToken;
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "Authorization, RefreshToken";
            var user = _mapper.Map<UserResponse>(result.user);
            user.KCal = result.kcal;
            user.IMC = result.IMC;
            user.FirstHealthMeasured = result.firstHealtMeasured;
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
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            var newPass = await _accountService.ForgotPasswordAsync(email);

            var subject = "Resetear Contraseña";
            var emailBody = EmailTemplateHelper.GetEmailTemplateString("ForgotPassword.html", subject, _env);
            emailBody = emailBody.ToForgotPasswordEmail(newPass);

            var to = new List<string> { email };
            await _emailService.SendEmailResponseAsync(subject, emailBody, to);

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

            var subject = "Reenviar Codigo de Verificación";
            var emailBody = EmailTemplateHelper.GetEmailTemplateString("AccountActivation.html", "Codigo de Activación", _env);
            emailBody = emailBody.ToActivationAccountEmail(code.ToString());
            var to = new List<string> { email };

            await _emailService.SendEmailResponseAsync(subject, emailBody, to);

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
        /// Get user profile. Requires authentication. Includes subscriptions
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserWithSubscriptionResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserProfile()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _profileHelthHelper.GetUserProfileUseAsync(loggedUser);
            var user = _mapper.Map<UserWithSubscriptionResponse>(result.user);
            user.KCal = result.kcal;
            user.IMC = result.IMC;
            user.FirstHealthMeasured = result.firstHealtMeasured;

            return Ok(new ApiOkResponse(user));
        }

        /// <summary>
        /// Get user settings. Requires authentication.
        /// </summary>
        [Authorize]
        [HttpGet("settings")]
        [ProducesResponseType(typeof(IEnumerable<BasicSettingResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserSettings()
        {
            var loggedUser = User.GetUserIdFromToken();
            var result = await _accountService.GetUserSettingsAsync(loggedUser);
            var mapped = _mapper.Map<IEnumerable<BasicSettingResponse>>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Update user settings. Requires authentication.
        /// </summary>
        /// <param name="request">Update setting request.</param>
        [Authorize]
        [HttpPost("settings")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateUserSettings([FromBody] List<UpdateSettingRequest> request)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _accountService.UpdateUserSettingsAsync(loggedUser, request);

            return Ok();
        }

        /// <summary>
        /// Disable user account. Requires authentication.
        /// </summary>
        /// <param name="softDeletion">
        /// Indicate if the account will be marked for future deletion after 30 days.
        /// </param>
        [Authorize]
        [HttpPost("remove-account")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveAccount([FromQuery] bool softDeletion)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _accountService.DisableAccountAsync(loggedUser, softDeletion);
            await _hub.Clients.All.SendAsync(HubConstants.USER_DISABLED, null);
            return Ok();
        }

        /// <summary>
        /// Update user profile. Requires authentication.
        /// </summary>
        /// <param name="userProfileRequest">Update user profile request.</param>
        [Authorize]
        [HttpPost("profile")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileRequest userProfileRequest)
        {
            var loggedUser = User.GetUserIdFromToken();
            await _accountService.UpdateProfileAsync(loggedUser, userProfileRequest);
            return Ok();
        }

        /// <summary>
        /// Validate if username exist but also returns a list of username's suggestions.
        /// </summary>
        /// <param name="username">Username to validate</param>
        /// <param name="email">User Email used to elaborate suggestions</param>
        /// <param name="fullName">User FullName used to elaborate sugestions</param>
        /// <param name="userId">
        /// User id who is updating the username. Set null if the user does not exist yet.
        /// </param>
        [HttpGet("username-validation")]
        [ProducesResponseType(typeof(IEnumerable<UsernameValidationResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UsernameValidation(string username, string email, string fullName, int? userId)
        {
            var userIdValue = userId.HasValue ? userId.Value : -1;

            var result = await _accountService.ValidateUsernameAsync(username, email, fullName, userIdValue);
            var mapped = new UsernameValidationResponse
            {
                IsValid = result.IsValid,
                Suggestions = result.Suggestions
            };

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Unsuscribe users from email notifications.
        /// </summary>
        /// <param name="token">Unique token of the user to be unsuscribed from email notifications</param>
        [HttpPatch("email-unsuscribe/{token}")]
        [ProducesResponseType(typeof(IEnumerable<UsernameValidationResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Unsuscribe([FromRoute] string token)
        {
            await _accountService.UnsubscribeEmailsAsync(token);

            return Ok();
        }
    }
}
