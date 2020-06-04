using MismeAPI.Common.DTO.Response;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Services.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IScheduleService
    {
        Task<Schedule> ScheduleEatReminderNotificationAsync(Eat eat, DateTime utcDeliverTime, bool commitChanges = false);

        Task<Schedule> ScheduleDisableSubscriptionAsync(UserSubscription userSubscription, bool commitChanges = false);

        Task RemoveJobIfExistIfExistAsync(string jobId, bool commitChanges = false);

        Task UserRecurringJobsSchedulerAsync(int userId, User user = null);
    }
}
