using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IUserService
    {
        Task<PaginatedList<User>> GetUsersAsync(int loggedUser, int pag, int perPag, string sortOrder, int statusFilter, string search);

        Task<dynamic> GetUsersStatsAsync(int loggedUser);

        Task<IEnumerable<UsersByDateSeriesResponse>> GetUsersStatsByDateAsync(int loggedUser, int type);

        Task<User> EnableUserAsync(int loggedUser, int id);

        Task<User> DisableUserAsync(int loggedUser, int id);

        Task SendUserNotificationAsync(int loggedUser, int id, UserNotificationRequest notif);

        Task<string> GetUserLanguageFromUserIdAsync(int loggedUser);

        Task<IEnumerable<EatsByDateSeriesResponse>> GetEatsStatsByDateAsync(int loggedUser, int type);

        Task<int> GetEatsCountAsync(int loggedUser);

        Task<IEnumerable<User>> GetUsersWithPlanAsync(DateTime date);

        Task<IEnumerable<User>> GetUsersWithoutPlanAsync(DateTime date);

        Task<User> GetUserDevicesAsync(int userId);

        Task<User> GetUserAsync(int userId);

        Task<bool> GetUserOptInNotificationAsync(int userId, string settingConstant);

        Task SendManualEmailAsync(SendEmailRequest request);
    }
}
