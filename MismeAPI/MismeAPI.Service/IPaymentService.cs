using MismeAPI.Common.DTO.Response.Payment;
using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Services
{
    public interface IPaymentService
    {
        Task<string> PaymentIntentAsync(int logguedUser, int productId, bool setupFutureUsage);

        Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent);

        Task HandlePaymentIntentFailed(PaymentIntent paymentIntent);

        Task HandlePaymentIntentCanceled(PaymentIntent paymentIntent);

        Task<IEnumerable<StripePaymentMethodResponse>> GetStripeCustomerPaymentMethods(int userId);

        Task DeleteStripeCustomerPaymentMethod(int userId, string paymentMethodId);

        Task<IEnumerable<Data.Entities.Order>> ValidateAppleReceiptAsync(int userId, string receiptData);
    }
}
