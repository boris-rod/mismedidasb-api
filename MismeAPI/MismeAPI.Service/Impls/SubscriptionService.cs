﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request.Subscription;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public SubscriptionService(IUnitOfWork uow, IConfiguration config, IUserService userService, INotificationService notificationService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
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

            return userSubscription;
        }

        public async Task DisableUserSubscriptionAsync(int userSubscriptionID)
        {
            var userSubscription = await _uow.UserSubscriptionRepository.GetAll()
                .Include(us => us.User)
                    .ThenInclude(us => us.Devices)
                .Where(us => us.Id == userSubscriptionID)
                .FirstOrDefaultAsync();

            if (userSubscription == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User Subscription");

            var now = DateTime.UtcNow;

            userSubscription.IsActive = false;
            userSubscription.ModifiedAt = now;
            await _uow.UserSubscriptionRepository.UpdateAsync(userSubscription, userSubscriptionID);

            if (userSubscription.User != null && userSubscription.User.Devices != null)
            {
                // TODO: Internationalization on this msgs.
                var title = "Subscription de plani terminada";
                var body = "Su subscription de plani ha terminado, vaya y activela otra vez para disfrutar de su divertida ayuda.";
                await _notificationService.SendFirebaseNotificationAsync(title, body, userSubscription.User.Devices);
            }

            await _uow.CommitAsync();
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

            var admin = await _uow.UserRepository.GetAll()
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.Subscription)
                .Where(u => u.Email == "admin@mismedidas.com")
                .FirstOrDefaultAsync();
            var adminHasPlani = admin.Subscriptions.Any(s => s.Subscription.Product == SubscriptionEnum.VIRTUAL_ASESSOR);

            if (!adminHasPlani)
            {
                // Init subscription to all users in the platform just once
                //TODO: Remove this...
                var users = await _uow.UserRepository.GetAll()
                    .Include(u => u.Subscriptions)
                        .ThenInclude(s => s.Subscription)
                    .ToListAsync();

                foreach (var user in users)
                {
                    await GetOrInitPlaniSubscriptionAsync(user);
                }
            }
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
    }
}
