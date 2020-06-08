using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using MismeAPI.Common.DTO.Request.Subscription;
using MismeAPI.Data.Entities.Enums;

namespace MismeAPI.Service
{
    public interface ISubscriptionService
    {
        Task<PaginatedList<Subscription>> GetSubscriptionsAsync(int pag, int perPag, string sortOrder, bool? isActive, string search);

        Task<Subscription> GetSubscriptionAsync(int id);

        Task<Subscription> GetSubscriptionByNameAsync(SubscriptionEnum subscriptionType);

        Task<Subscription> CreateSubscriptionAsync(CreateSubscriptionRequest request);

        Task<Subscription> UpdateSubscriptionAsync(int id, UpdateSubscriptionRequest request);

        Task DeleteSubscriptionAsync(int id);

        Task<UserSubscription> GetOrInitPlaniSubscriptionAsync(User user);

        Task<UserSubscription> BuySubscriptionAsync(int loggedUser, int subscriptionId);

        Task DisableUserSubscriptionAsync(int userSubscriptionID);

        Task<UserSubscription> AssignSubscriptionAsync(int userId, SubscriptionEnum subscriptionType);

        Task SeedSubscriptionAsync();
    }
}
