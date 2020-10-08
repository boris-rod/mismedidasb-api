using Amazon.S3;
using Amazon.S3.Model;
using Stripe;
using System.IO;
using System.Threading.Tasks;

namespace MismeAPI.Services
{
    public interface IPaymentService
    {
        Task<string> PaymentIntentAsync(int logguedUser, int productId);

        Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent);

        Task HandlePaymentIntentFailed(PaymentIntent paymentIntent);

        Task HandlePaymentIntentCanceled(PaymentIntent paymentIntent);
    }
}
