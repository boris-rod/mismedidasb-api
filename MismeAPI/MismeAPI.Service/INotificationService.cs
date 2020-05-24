using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface INotificationService
    {
        Task NotifyEatReminderAsync(int eatId);

        Task SendFirebaseNotificationAsync(string title, string body, IEnumerable<Device> devices);
    }
}
