﻿using Microsoft.AspNetCore.Http;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Data.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MismeAPI.Services
{
    public interface IAccountService
    {
        Task<User> SignUpAsync(SignUpRequest suRequest);

        Task<(User user, string accessToken, string refreshToken, double kcal, double IMC, DateTime? firstHealtMeasured)> LoginAsync(LoginRequest loginRequest);

        Task LogoutAsync(int userIdValue, string accessToken);

        Task<ClaimsPrincipal> GetPrincipalFromExpiredTokenAsync(string token);

        Task GetRefreshTokenAsync(RefreshTokenRequest refreshToken, string userId);

        Task<(string accessToken, string refreshToken)> GenerateNewTokensAsync(string token, string refreshToken);

        Task ChangePasswordAsync(ChangePasswordRequest changePassword, int userId);

        Task GlobalLogoutAsync(int userId);

        Task<bool> ValidateTokenAsync(string token);

        Task<User> GetUserAsync(int userId);

        Task<User> ChangeAccountStatusAsync(ChangeAccountStatusRequest changeAccountStatus, int userId);

        Task<User> UploadAvatarAsync(IFormFile file, int currentUser);

        Task<string> ForgotPasswordAsync(string email);

        Task<User> ActivationAccountAsync(ActivationAccountRequest activation);

        Task<int> ResendVerificationCodeAsync(string email);

        Task<User> RemoveAvatarAsync(int loggedUser);

        Task<IEnumerable<UserSetting>> GetUserSettingsAsync(int loggedUser);

        Task UpdateUserSettingsAsync(int loggedUser, List<UpdateSettingRequest> request);

        Task DisableAccountAsync(int loggedUser, bool softDeletion);

        Task UpdateProfileAsync(int loggedUser, UpdateUserProfileRequest userProfileRequest);

        Task<(bool IsValid, ICollection<string> Suggestions)> ValidateUsernameAsync(string username, string email, string fullName, int userId = -1);

        Task<double> GetKCalAsync(int userId);

        Task<double> GetIMCAsync(int userId);

        Task<int> GetHeightAsync(int userId);

        Task<int> GetSexAsync(int userId);

        Task<bool> ValidateGroupAdminFirstLoginAsync(User user);

        Task UnsubscribeEmailsAsync(string token);
    }
}
