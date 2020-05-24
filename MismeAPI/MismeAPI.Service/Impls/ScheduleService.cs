using Hangfire;
using Microsoft.Extensions.Configuration;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using MismeAPI.Service.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public ScheduleService(IUnitOfWork uow, IUserService userService, IConfiguration config)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<Schedule> ScheduleEatReminderNotificationAsync(Eat eat, DateTime utcDeliverTime, bool commitChanges = false)
        {
            var timeOffset = utcDeliverTime.ToDateTimeOffset(TimeZoneInfo.Utc);

            var jobId = BackgroundJob.Schedule<INotificationService>(x => x.NotifyEatReminderAsync(eat.Id), timeOffset);

            var schedule = new Schedule
            {
                JobId = jobId,
                IsProcessed = false
            };

            await _uow.ScheduleRepository.AddAsync(schedule);

            if (commitChanges)
                await _uow.CommitAsync();

            return schedule;
        }

        public async Task RemoveJobIfExistIfExistAsync(string jobId, bool commitChanges = false)
        {
            BackgroundJob.Delete(jobId);

            var schedule = await _uow.ScheduleRepository.FindAsync(s => s.JobId == jobId);

            if (schedule != null)
                _uow.ScheduleRepository.Delete(schedule);

            if (commitChanges)
                await _uow.CommitAsync();
        }
    }
}
