using Microsoft.AspNetCore.Http;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Data.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MismeAPI.Services
{
    public interface IAccountService
    {
        Task<User> SignUpAsync(SignUpRequest suRequest);

        Task<(User user, string accessToken, string refreshToken)> LoginAsync(LoginRequest loginRequest);

        Task LogoutAsync(int userIdValue, string accessToken);

        Task<ClaimsPrincipal> GetPrincipalFromExpiredTokenAsync(string token);

        Task GetRefreshTokenAsync(RefreshTokenRequest refreshToken, string userId);

        Task<(string accessToken, string refreshToken)> GenerateNewTokensAsync(string token, string refreshToken);

        Task ChangePasswordAsync(ChangePasswordRequest changePassword, int userId);

        Task GlobalLogoutAsync(int userId);

        Task<bool> ValidateTokenAsync(string token);

        Task<User> GetUserAsync(int userId);

        Task ChangeAccountStatusAsync(ChangeAccountStatusRequest changeAccountStatus, int userId);

        Task<User> UploadAvatarAsync(IFormFile file, int currentUser);

        Task<string> ForgotPasswordAsync(string email);

        Task<User> ActivationAccountAsync(ActivationAccountRequest activation);

        Task<int> ResendVerificationCodeAsync(string email);
    }
}