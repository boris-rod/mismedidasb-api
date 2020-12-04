using CorePush.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using MismeAPI.Service.Notifications.Firebase;
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
                .Include(e => e.EatSchedule)
                    .ThenInclude(es => es.Schedule)
                .Include(e => e.User)
                    .ThenInclude(u => u.Devices)
                .FirstOrDefaultAsync();

            var schedule = eat?.EatSchedule?.Schedule;
            var isProcessed = schedule != null ? schedule.IsProcessed : false;

            if (!isProcessed && eat != null)
            {
                var devices = eat.User?.Devices;

                if (devices != null)
                {
                    var wantNotification = await _userService.GetUserOptInNotificationAsync(eat.User.Id, SettingsConstants.PREPARE_EAT_REMINDER);
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

            if (!isProcessed && schedule != null)
            {
                schedule.IsProcessed = true;
                await _uow.ScheduleRepository.UpdateAsync(schedule, schedule.Id);
                await _uow.CommitAsync();
            }
        }

        public async Task NotifyDrinWaterReminderAsync(int userId)
        {
            var user = await _userService.GetUserDevicesAsync(userId);
            var wantNotification = await _userService.GetUserOptInNotificationAsync(userId, SettingsConstants.DRINK_WATER_REMINDER);

            if (wantNotification && user?.Devices != null)
            {
                var lang = await _userService.GetUserLanguageFromUserIdAsync(userId);
                var title = (lang == "EN") ? "Remember to drink enough water" : "Recuarda tomar suficiente agua";
                var body = (lang == "EN") ? "Dr.PlaniFive recomends 2L of water every day between eats" : "El Dr.PlaniFive recomienda tomar 2L de agua cada dia entre las comidas";

                await SendFirebaseNotificationAsync(title, body, user.Devices);
            }
        }

        public async Task SendFirebaseNotificationAsync(string title, string body, IEnumerable<Device> devices, string externalUrl = "")
        {
            var serverKey = _config["Firebase:ServerKey"];
            var senderId = _config["Firebase:SenderId"];

            var data = new Dictionary<string, string>()
            {
                { "notiTitle", title},
                { "notiBody", body},
                { "externalUrl", externalUrl}
            };

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
                            Sound = "default"
                        },
                        Data = data,
                        Token = device.Token
                    };

                    var response = await fcm.SendAsync(device.Token, message);
                }
            }
        }
    }
}
