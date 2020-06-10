using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request.Subscription;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.Migrations;
using MismeAPI.Data.UoW;
using MismeAPI.Services.Utils;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly IScheduleService _scheduleService;

        public SubscriptionService(IUnitOfWork uow, IConfiguration config, IUserService userService, IScheduleService scheduleService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
        }

        public async Task<PaginatedList<Subscription>> GetSubscriptionsAsync(int pag, int perPag, string sortOrder, bool? isActive, string search)
        {
            var result = _uow.SubscriptionRepository.GetAll().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(i => i.Name.ToLower().Contains(search.ToLower()) || i.Product.ToString().Contains(search));
            }

            // define status filter
            if (isActive.HasValue)
            {
                result = result.Where(i => i.IsActive == isActive.Value);
            }

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "product_desc":
                        result = result.OrderByDescending(i => i.Product);
                        break;

                    case "product_asc":
                        result = result.OrderBy(i => i.Product);
                        break;

                    case "isActive_desc":
                        result = result.OrderByDescending(i => i.IsActive);
                        break;

                    case "isActive_asc":
                        result = result.OrderBy(i => i.IsActive);
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<Subscription>.CreateAsync(result, pag, perPag);
        }

        public async Task<Subscription> GetSubscriptionAsync(int id)
        {
            var subscription = await _uow.SubscriptionRepository.GetAsync(id);
            if (subscription == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Subscription");

            return subscription;
        }

        public async Task<Subscription> GetSubscriptionByNameAsync(SubscriptionEnum subscriptionType)
        {
            var subscription = await _uow.SubscriptionRepository.FindAsync(s => s.Product == subscriptionType);
            if (subscription == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Subscription");

            return subscription;
        }

        public async Task<Subscription> GetSubscriptionByProductAsync(SubscriptionEnum product)
        {
            var subscription = await _uow.SubscriptionRepository.FindAsync(s => s.Product == product);
            if (subscription == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Subscription");

            return subscription;
        }

        public async Task<Subscription> CreateSubscriptionAsync(CreateSubscriptionRequest request)
        {
            var product = (SubscriptionEnum)request.Product;

            var subscription = await _uow.SubscriptionRepository.FindAsync(c => c.Product == product);
            if (subscription != null)
                throw new AlreadyExistsException("Subscription already exists");

            subscription = new Subscription
            {
                Name = request.Name,
                Product = product,
                IsActive = request.IsActive,
                ValueCoins = request.ValueCoins,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            await _uow.SubscriptionRepository.AddAsync(subscription);
            await _uow.CommitAsync();

            return subscription;
        }

        public async Task<Subscription> UpdateSubscriptionAsync(int id, UpdateSubscriptionRequest request)
        {
            var subscription = await GetSubscriptionAsync(id);

            subscription.Name = request.Name;
            subscription.ValueCoins = request.ValueCoins;
            subscription.IsActive = request.IsActive;
            subscription.ModifiedAt = DateTime.UtcNow;

            await _uow.SubscriptionRepository.UpdateAsync(subscription, id);
            await _uow.CommitAsync();

            return subscription;
        }

        public async Task DeleteSubscriptionAsync(int id)
        {
            var Subscription = await GetSubscriptionAsync(id);

            _uow.SubscriptionRepository.Delete(Subscription);
            await _uow.CommitAsync();
        }

        public async Task<UserSubscription> AssignSubscriptionAsync(int userId, SubscriptionEnum subscriptionType)
        {
            var user = await _userService.GetUserAsync(userId);
            var subscription = await GetSubscriptionByNameAsync(subscriptionType);
            var userSubscription = user.Subscriptions.FirstOrDefault(us => us.Subscription.Product == subscription.Product);

            if (!subscription.IsActive || subscription.Product != SubscriptionEnum.VIRTUAL_ASESSOR)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "subscription.");
            }

            if (userSubscription == null)
            {
                userSubscription = await GetOrInitPlaniSubscriptionAsync(user);
            }
            else
            {
                userSubscription = InitOrIncreaseUserSubscriptionObject(user.Id, subscription.Id, userSubscription);
                await _uow.UserSubscriptionRepository.UpdateAsync(userSubscription, userSubscription.Id);
            }

            var schedule = await ScheduleJobAsync(userSubscription);
            userSubscription.UserSubscriptionSchedule = new UserSubscriptionSchedule
            {
                Schedule = schedule
            };

            await _uow.CommitAsync();

            return userSubscription;
        }

        public async Task<UserSubscription> GetOrInitPlaniSubscriptionAsync(User user)
        {
            if (user == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");

            var existSubscription = await _uow.UserSubscriptionRepository.GetAll()
                .Include(us => us.Subscription)
                .Where(us => us.UserId == user.Id && us.Subscription.Product == SubscriptionEnum.VIRTUAL_ASESSOR)
                .FirstOrDefaultAsync();

            if (existSubscription != null)
                return existSubscription;

            var subscription = await GetSubscriptionByProductAsync(SubscriptionEnum.VIRTUAL_ASESSOR);

            var userSubscription = InitOrIncreaseUserSubscriptionObject(user.Id, subscription.Id);

            await _uow.UserSubscriptionRepository.AddAsync(userSubscription);

            await _uow.CommitAsync();

            var schedule = await ScheduleJobAsync(userSubscription);
            userSubscription.UserSubscriptionSchedule = new UserSubscriptionSchedule
            {
                Schedule = schedule
            };
            await _uow.CommitAsync();

            return userSubscription;
        }

        public async Task<UserSubscription> BuySubscriptionAsync(int loggedUser, int subscriptionId)
        {
            var user = await _userService.GetUserAsync(loggedUser);
            var subscription = await GetSubscriptionAsync(subscriptionId);
            var statistics = user.UserStatistics;
            var userSubscription = user.Subscriptions.FirstOrDefault(us => us.Subscription.Product == subscription.Product);

            if (statistics == null || statistics.Coins < subscription.ValueCoins)
            {
                throw new Exception("Not enough coins to buy subscription");
            }

            if (!subscription.IsActive || subscription.Product != SubscriptionEnum.VIRTUAL_ASESSOR)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "subscription.");
            }

            if (userSubscription == null)
            {
                userSubscription = await GetOrInitPlaniSubscriptionAsync(user);
            }
            else
            {
                userSubscription = InitOrIncreaseUserSubscriptionObject(user.Id, subscription.Id, userSubscription);
                await _uow.UserSubscriptionRepository.UpdateAsync(userSubscription, userSubscription.Id);
            }

            statistics.Coins -= subscription.ValueCoins;
            await _uow.UserStatisticsRepository.UpdateAsync(statistics, statistics.Id);

            await _uow.CommitAsync();

            var schedule = await ScheduleJobAsync(userSubscription);
            userSubscription.UserSubscriptionSchedule = new UserSubscriptionSchedule
            {
                Schedule = schedule
            };

            await _uow.CommitAsync();

            return userSubscription;
        }

        public async Task DisableUserSubscriptionAsync(int userSubscriptionID)
        {
            var userSubscription = await _uow.UserSubscriptionRepository.GetAll()
                .Include(us => us.UserSubscriptionSchedule)
                    .ThenInclude(us => us.Schedule)
                .Where(us => us.Id == userSubscriptionID)
                .FirstOrDefaultAsync();

            if (userSubscription == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User Subscription");

            var now = DateTime.UtcNow;
            var schedule = userSubscription.UserSubscriptionSchedule?.Schedule;

            if (schedule != null && !schedule.IsProcessed && userSubscription.ValidAt < now)
            {
                userSubscription.IsActive = false;
                userSubscription.ModifiedAt = now;
                await _uow.UserSubscriptionRepository.UpdateAsync(userSubscription, userSubscriptionID);

                schedule.IsProcessed = true;
                await _uow.ScheduleRepository.UpdateAsync(schedule, schedule.Id);

                await _uow.CommitAsync();
            }
        }

        public async Task SeedSubscriptionAsync()
        {
            var subscriptionReques1 = new CreateSubscriptionRequest
            {
                Name = "Plani",
                Product = (int)SubscriptionEnum.VIRTUAL_ASESSOR,
                ValueCoins = 1000,
                IsActive = true
            };

            var subscription1 = await _uow.SubscriptionRepository
                .FindAsync(c => c.Product == SubscriptionEnum.VIRTUAL_ASESSOR);
            if (subscription1 == null)
                await CreateSubscriptionAsync(subscriptionReques1);
        }

        private UserSubscription InitOrIncreaseUserSubscriptionObject(int userId, int subscriptionId, UserSubscription userSubscription = null)
        {
            var validFor = TimeSpan.FromDays(30);
            var now = DateTime.UtcNow;

            if (userSubscription == null)
            {
                userSubscription = new UserSubscription
                {
                    UserId = userId,
                    IsActive = true,
                    SubscriptionId = subscriptionId,
                    ValidDays = validFor,
                    ValidAt = now.AddDays(validFor.TotalDays),
                    CreatedAt = now,
                    ModifiedAt = now
                };
            }
            else
            {
                userSubscription.IsActive = true;
                userSubscription.ValidAt = userSubscription.ValidAt.AddDays(validFor.TotalDays);
                userSubscription.ValidDays = validFor;
                userSubscription.ModifiedAt = now;
            }

            return userSubscription;
        }

        private async Task<Schedule> ScheduleJobAsync(UserSubscription userSubscription)
        {
            var usJob = await _uow.UserSubscriptionScheduleRepository.GetAll()
                .Include(usr => usr.Schedule)
                .Where(usr => usr.UserSubscriptionId == userSubscription.Id)
                .FirstOrDefaultAsync();

            if (usJob != null)
            {
                //create
                await _scheduleService.RemoveJobIfExistIfExistAsync(usJob.Schedule.JobId, false);
            }

            var schedule = await _scheduleService.ScheduleDisableSubscriptionAsync(userSubscription, false);
            return schedule;
        }
    }
}
