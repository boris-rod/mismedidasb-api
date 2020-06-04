using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using MismeAPI.Service.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<Schedule> ScheduleDisableSubscriptionAsync(UserSubscription userSubscription,
            bool commitChanges = false)
        {
            var excecutionTime = userSubscription.ValidAt;
            var timeOffset = excecutionTime.ToDateTimeOffset(TimeZoneInfo.Utc);

            var jobId = BackgroundJob.Schedule<ISubscriptionService>(x => x.DisableUserSubscriptionAsync(userSubscription.Id), timeOffset);

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

        public async Task UserRecurringJobsSchedulerAsync(int userId, User user = null)
        {
            if (user == null)
                user = await _uow.UserRepository.GetAll()
                    .Include(u => u.UserSchedules)
                        .ThenInclude(us => us.Schedule)
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync();
            if (user == null)
                return;

            /*JobConstants.DRINK_WATER_RECURRING_JOB*/
            var userScheduleForDrinkWater = user.UserSchedules.Where(us => us.JobConstant == JobConstants.DRINK_WATER_RECURRING_JOB).FirstOrDefault();
            var existJobForDrinkWater = userScheduleForDrinkWater?.Schedule;
            var userWantDrinkWaterNotification = await _userService.GetUserOptInNotificationAsync(userId, SettingsConstants.DRINK_WATER_REMINDER);

            if (userScheduleForDrinkWater != null && existJobForDrinkWater != null)
            {
                if (userWantDrinkWaterNotification && userScheduleForDrinkWater.UserTimeZone != user.TimeZone) //!=
                {
                    AddOrUpdateRecurringJob(existJobForDrinkWater.JobId, user.TimeZone, userId, JobConstants.DRINK_WATER_RECURRING_JOB);
                    userScheduleForDrinkWater.UserTimeZone = user.TimeZone;
                    await _uow.UserScheduleRepository.UpdateAsync(userScheduleForDrinkWater, userScheduleForDrinkWater.Id);
                }
                if (!userWantDrinkWaterNotification)
                {
                    RecurringJob.RemoveIfExists(existJobForDrinkWater.JobId);
                    _uow.ScheduleRepository.Delete(existJobForDrinkWater);
                }
            }
            else
            {
                if (userWantDrinkWaterNotification)
                {
                    var jobId = GenerateIdForRecurringJob(userId, "DRINK_WATER_RECURRING_JOB");
                    AddOrUpdateRecurringJob(jobId, user.TimeZone, userId, JobConstants.DRINK_WATER_RECURRING_JOB);

                    await CreateUserScheduleAsync(userId, jobId, user.TimeZone, JobConstants.DRINK_WATER_RECURRING_JOB);
                }
            }
            /*JobConstants.DRINK_WATER_RECURRING_JOB*/

            await _uow.CommitAsync();
        }

        private async Task<bool> ExistJobAsync(string jobId)
        {
            var existJob = await _uow.ScheduleRepository.FindAsync(s => s.JobId == jobId);
            return existJob != null;
        }

        private void AddOrUpdateRecurringJob(string jobId, string timeZoneId, int userId, string jobConstant)
        {
            var cronExp = "0 0 12,18 * * ?";
            if (String.IsNullOrEmpty(timeZoneId))
                timeZoneId = TimeZoneInfo.Local.Id;

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            if (jobConstant == JobConstants.DRINK_WATER_RECURRING_JOB)
                RecurringJob.AddOrUpdate<INotificationService>(jobId, x => x.NotifyDrinWaterReminderAsync(userId), cronExp, timeZoneInfo);
        }

        private string GenerateIdForRecurringJob(int userId, string action)
        {
            return "user-" + userId.ToString() + "-" + action;
        }

        private async Task CreateUserScheduleAsync(int userId, string jobId, string timeZone, string jobConstant)
        {
            if (!await ExistJobAsync(jobId))
            {
                var job = new Schedule
                {
                    JobId = jobId
                };

                var userSchedule = new UserSchedule
                {
                    UserId = userId,
                    Schedule = job,
                    JobConstant = jobConstant,
                    UserTimeZone = timeZone
                };

                await _uow.UserScheduleRepository.AddAsync(userSchedule);
            }
        }
    }
}
