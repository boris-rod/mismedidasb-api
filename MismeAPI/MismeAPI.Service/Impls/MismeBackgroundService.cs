using CorePush.Google;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class MismeBackgroundService : IMismeBackgroundService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public MismeBackgroundService(IUnitOfWork uow, IConfiguration config)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task CleanExpiredTokensAsync()
        {
            var expiredTokens = await _uow.UserTokenRepository.GetAll().Where(t => DateTime.UtcNow > t.RefreshTokenExpiresDateTime).ToListAsync();
            foreach (var t in expiredTokens)
            {
                _uow.UserTokenRepository.Delete(t);
            }
            await _uow.CommitAsync();
        }

        public async Task RemoveDisabledAccountsAsync()
        {
            var dateToCompare = DateTime.UtcNow.AddDays(-30);
            var disabledUsers = await _uow.UserRepository.GetAll().Where(t => t.MarkedForDeletion == true &&
                                            t.DisabledAt.HasValue &&
                                            dateToCompare > t.DisabledAt.Value)
                                            .ToListAsync();
            foreach (var u in disabledUsers)
            {
                _uow.UserRepository.Delete(u);
            }
            await _uow.CommitAsync();
        }

        public async Task SendFireBaseNotificationsAsync()
        {
            var serverKey = _config["Firebase:ServerKey"];
            var senderId = _config["Firebase:SenderId"];

            var tokens = await _uow.DeviceRepository.GetAllAsync();
            foreach (var device in tokens)
            {
                using (var fcm = new FcmSender(serverKey, senderId))
                {
                    //if (device.Type == Data.Entities.Enums.DeviceTypeEnum.ANDROID)
                    //{
                    //var googleNot = new GoogleNotification();
                    //googleNot.Data = new GoogleNotification.DataPayload
                    //{
                    //    Message = "Testing"
                    //};

                    Message message = new Message()
                    {
                        Notification = new Notification
                        {
                            Title = "My push notification title",
                            Body = "Content for this push notification"
                        }
                        ,
                        //           Data = new Dictionary<string, string>()
                        //{
                        //    { "AdditionalData1", "data 1" },
                        //    { "AdditionalData2", "data 2" },
                        //    { "AdditionalData3", "data 3" },
                        //},
                        Token = device.Token
                    };

                    var response = await fcm.SendAsync(device.Token, message);
                    //}
                    //else
                    //{
                    //    var appleNot = new AppleNotification();
                    //    appleNot.Aps = new AppleNotification.ApsPayload
                    //    {
                    //        AlertBody = "Testing"
                    //    };
                    //    await fcm.SendAsync(device.Token, appleNot);
                    //}
                }
            }
        }

        public async Task SendFireBaseNotificationsRemindersAsync()
        {
            var serverKey = _config["Firebase:ServerKey"];
            var senderId = _config["Firebase:SenderId"];

            var tomorrow = DateTime.UtcNow.AddDays(1);
            var usersWithoutPlans = await _uow.UserRepository.GetAll()
                .Include(u => u.Eats)
                .Include(u => u.Devices)
                .Include(u => u.UserSettings)
                    .ThenInclude(s => s.Setting)
                .Where(u => !u.Eats.Any(e => e.CreatedAt.Date == tomorrow.Date))
                .ToListAsync();

            var reminder = await _uow.ReminderRepository.GetAll().Where(r => r.CodeName == RemindersConstants.NO_EAT_PLANNED_FOR_TOMORROW).FirstOrDefaultAsync();

            if (reminder != null)
            {
                foreach (var us in usersWithoutPlans)
                {
                    var lang = us.UserSettings.Where(us => us.Setting.Name == SettingsConstants.LANGUAGE).FirstOrDefault();
                    var language = "";

                    if (lang == null || string.IsNullOrWhiteSpace(lang.Value))
                    {
                        language = "ES";
                    }
                    else
                    {
                        language = lang.Value.ToUpper();
                    }

                    foreach (var device in us.Devices)
                    {
                        using (var fcm = new FcmSender(serverKey, senderId))
                        {
                            Message message = new Message()
                            {
                                Notification = new Notification
                                {
                                    Title = language == "EN" && !string.IsNullOrWhiteSpace(reminder.TitleEN) ? reminder.TitleEN :
                                    (language == "IT" && !string.IsNullOrWhiteSpace(reminder.TitleIT) ? reminder.TitleIT : reminder.Title),
                                    Body = language == "EN" && !string.IsNullOrWhiteSpace(reminder.BodyEN) ? reminder.BodyEN :
                                    (language == "IT" && !string.IsNullOrWhiteSpace(reminder.BodyIT) ? reminder.BodyIT : reminder.Body),
                                },
                                Token = device.Token
                            };
                            try
                            {
                                var response = await fcm.SendAsync(device.Token, message);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
        }
    }
}
