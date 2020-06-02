using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using MismeAPI.Common.DTO.Request.Subscription;

namespace MismeAPI.Service
{
    public interface ISubscriptionService
    {
        Task<PaginatedList<Subscription>> GetSubscriptionsAsync(int pag, int perPag, string sortOrder, bool? isActive, string search);

        Task<Subscription> GetSubscriptionAsync(int id);

        Task<Subscription> CreateSubscriptionAsync(CreateSubscriptionRequest request);

        Task<Subscription> UpdateSubscriptionAsync(int id, UpdateSubscriptionRequest request);

        Task DeleteSubscriptionAsync(int id);

        Task<UserSubscription> GetOrInitPlaniSubscriptionAsync(User user, bool commit);

        Task<UserSubscription> BuySubscriptionAsync(int loggedUser, int subscriptionId);

        Task SeedSubscriptionAsync();
    }
}
