using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request.Subscription;
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

            if (!subscription.IsActive)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "subscription.");
            }

            if (userSubscription == null)
            {
                userSubscription = await GetOrInitSubscriptionAsync(user, subscriptionType);
            }
            else
            {
                userSubscription = InitOrIncreaseUserSubscriptionObject(user.Id, subscription.Id, userSubscription);
                await _uow.UserSubscriptionRepository.UpdateAsync(userSubscription, userSubscription.Id);
            }

            await _uow.CommitAsync();

            return userSubscription;
        }

        public async Task<UserSubscription> GetOrInitSubscriptionAsync(User user, SubscriptionEnum subscriptionType)
        {
            if (user == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");

            var existSubscription = await _uow.UserSubscriptionRepository.GetAll()
                .Include(us => us.Subscription)
                .Where(us => us.UserId == user.Id && us.Subscription.Product == subscriptionType)
                .FirstOrDefaultAsync();

            if (existSubscription != null)
                return existSubscription;

            var subscription = await GetSubscriptionByProductAsync(subscriptionType);

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
                throw new UnprocessableEntityException("No tiene suficientes monedas para comprar la subscripción.");
            }

            if (!subscription.IsActive)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "subscription.");
            }

            if (userSubscription == null)
            {
                userSubscription = await GetOrInitSubscriptionAsync(user, subscription.Product);
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

        public async Task<IEnumerable<UserSubscription>> BuySubscriptionPackageAsync(int loggedUser)
        {
            var bulkSubscriptions = new List<SubscriptionEnum> { SubscriptionEnum.VIRTUAL_ASESSOR, SubscriptionEnum.FOOD_REPORT, SubscriptionEnum.NUTRITION_REPORT };
            var unitPrice = 3000;

            var user = await _userService.GetUserAsync(loggedUser);
            var subscriptions = await _uow.SubscriptionRepository.GetAll()
                .Where(s => bulkSubscriptions.Contains(s.Product))
                .ToListAsync();

            if (subscriptions.Where(s => s.IsActive).Count() != 4)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "subscriptions.");
            }

            var statistics = user.UserStatistics;

            if (statistics == null || statistics.Coins < unitPrice)
            {
                throw new UnprocessableEntityException("No tiene suficientes monedas para comprar la subscripción.");
            }

            foreach (var subscription in subscriptions)
            {
                var userSubscription = user.Subscriptions.FirstOrDefault(us => us.Subscription.Product == subscription.Product);
                if (userSubscription == null)
                {
                    userSubscription = await GetOrInitSubscriptionAsync(user, subscription.Product);
                }
                else
                {
                    userSubscription = InitOrIncreaseUserSubscriptionObject(user.Id, subscription.Id, userSubscription);
                    await _uow.UserSubscriptionRepository.UpdateAsync(userSubscription, userSubscription.Id);
                }
            }

            statistics.Coins -= unitPrice;
            await _uow.UserStatisticsRepository.UpdateAsync(statistics, statistics.Id);

            await _uow.CommitAsync();

            return user.Subscriptions;
        }

        public async Task DisableUserSubscriptionAsync(int userSubscriptionID)
        {
            var userSubscription = await _uow.UserSubscriptionRepository.GetAll()
                .Include(us => us.Subscription)
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

            var subscriptionName = userSubscription.Subscription != null ? userSubscription.Subscription.Name : "-";

            if (userSubscription.User != null && userSubscription.User.Devices != null)
            {
                // TODO: Internationalization on this msgs.
                var title = "Subscripción terminada";
                var body = "Su período de suscripción a " + subscriptionName + " ha expirado. Renovar";
                await _notificationService.SendFirebaseNotificationAsync(title, body, userSubscription.User.Devices);
            }

            await _uow.CommitAsync();
        }

        public async Task NotifySubscriptionAboutToExpireAsync(int userSubscriptionID)
        {
            var userSubscription = await _uow.UserSubscriptionRepository.GetAll()
                .Include(us => us.Subscription)
                .Include(us => us.User)
                    .ThenInclude(us => us.Devices)
                .Where(us => us.Id == userSubscriptionID)
                .FirstOrDefaultAsync();

            if (userSubscription == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User Subscription");

            var subscriptionName = userSubscription.Subscription != null ? userSubscription.Subscription.Name : "-";

            if (userSubscription.User != null && userSubscription.User.Devices != null)
            {
                // TODO: Internationalization on this msgs.
                var title = "Subscripcion cerca de expirar";
                var body = "Subscripción " + subscriptionName + " cerca de expirar (24h). Renovar";
                await _notificationService.SendFirebaseNotificationAsync(title, body, userSubscription.User.Devices);
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

            var subscriptionReques2 = new CreateSubscriptionRequest
            {
                Name = "Reporte de Alimentacion",
                Product = (int)SubscriptionEnum.FOOD_REPORT,
                ValueCoins = 1000,
                IsActive = true
            };

            var subscriptionReques3 = new CreateSubscriptionRequest
            {
                Name = "Reporte de Nutricion",
                Product = (int)SubscriptionEnum.NUTRITION_REPORT,
                ValueCoins = 1000,
                IsActive = true
            };

            var subscriptionReques4 = new CreateSubscriptionRequest
            {
                Name = "Planes pre-elaborados (Menues)",
                Product = (int)SubscriptionEnum.MENUES,
                ValueCoins = 1000,
                IsActive = true
            };

            var subscriptions = await _uow.SubscriptionRepository.GetAll().ToListAsync();

            var subscription1 = subscriptions.FirstOrDefault(c => c.Product == SubscriptionEnum.VIRTUAL_ASESSOR);
            if (subscription1 == null)
                await CreateSubscriptionAsync(subscriptionReques1);

            var subscription2 = subscriptions.FirstOrDefault(c => c.Product == SubscriptionEnum.FOOD_REPORT);
            if (subscription2 == null)
                await CreateSubscriptionAsync(subscriptionReques2);

            var subscription3 = subscriptions.FirstOrDefault(c => c.Product == SubscriptionEnum.NUTRITION_REPORT);
            if (subscription3 == null)
                await CreateSubscriptionAsync(subscriptionReques3);

            var subscription4 = subscriptions.FirstOrDefault(c => c.Product == SubscriptionEnum.MENUES);
            if (subscription4 == null)
                await CreateSubscriptionAsync(subscriptionReques4);

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
                    await GetOrInitSubscriptionAsync(user, SubscriptionEnum.VIRTUAL_ASESSOR);
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
