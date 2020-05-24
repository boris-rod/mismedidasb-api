using CorePush.Google;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public NotificationService(IUnitOfWork uow, IUserService userService, IConfiguration config)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task NotifyEatReminderAsync(int eatId)
        {
            var eat = await _uow.EatRepository.GetAll().Where(e => e.Id == eatId)
                .Include(e => e.User)
                    .ThenInclude(u => u.Devices)
                .FirstOrDefaultAsync();

            if (eat != null)
            {
                var devices = eat.User?.Devices;
                if (devices != null)
                {
                    var wantNotification = await GetUserOptInNotificationAsync(eat.User.Id, SettingsConstants.PREPARE_EAT_REMINDER);
                    if (wantNotification)
                    {
                        var lang = await _userService.GetUserLanguageFromUserIdAsync(eat.User.Id);
                        var title = (lang == "EN") ? "Remember to make your " : "Recuarda preparar tu ";
                        title = title + eat.EatType;

                        var body = (lang == "EN") ? "You have an eat planned in 10 minutes, remember to prepare it" : "Tienes una comida planificada dentro de 10 minutos, no olvides preparala ya";

                        await SendFirebaseNotificationAsync(title, body, devices);
                    }
                }
            }
        }

        public async Task SendFirebaseNotificationAsync(string title, string body, IEnumerable<Device> devices)
        {
            var serverKey = _config["Firebase:ServerKey"];
            var senderId = _config["Firebase:SenderId"];

            foreach (var device in devices)
            {
                using (var fcm = new FcmSender(serverKey, senderId))
                {
                    Message message = new Message()
                    {
                        Notification = new Notification
                        {
                            Title = title,
                            Body = body,
                        },
                        Token = device.Token
                    };

                    var response = await fcm.SendAsync(device.Token, message);
                }
            }
        }

        private async Task<bool> GetUserOptInNotificationAsync(int userId, string settingConstant)
        {
            if (settingConstant != SettingsConstants.PREPARE_EAT_REMINDER)
                return false;

            var setting = await _uow.SettingRepository.GetAll().Where(s => s.Name == settingConstant).FirstOrDefaultAsync();
            if (setting != null)
            {
                var us = await _uow.UserSettingRepository.GetAll().Where(us => us.SettingId == setting.Id && us.UserId == userId).FirstOrDefaultAsync();
                if (us != null && !string.IsNullOrWhiteSpace(us.Value))
                {
                    return us.Value == "true";
                }
            }
            return false;
        }
    }
}
