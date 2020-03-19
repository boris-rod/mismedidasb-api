using CorePush.Google;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Data.UoW;
using System;
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
    }
}