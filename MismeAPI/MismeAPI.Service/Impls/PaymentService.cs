using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common.DTO.Response.Payment;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Service;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Services.Impls
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _uow;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly IAppleAppStoreService _appleAppStoreService;

        public PaymentService(IConfiguration configuration, IUnitOfWork uow, IProductService productService,
            IUserService userService, IUserStatisticsService userStatisticsService, IAppleAppStoreService appleAppStoreService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userStatisticsService = userStatisticsService ?? throw new ArgumentNullException(nameof(userStatisticsService));
            _appleAppStoreService = appleAppStoreService ?? throw new ArgumentNullException(nameof(appleAppStoreService));
        }

        /// <summary>
        /// Create a Stripe Payment Intent Auth
        /// </summary>
        /// <param name="logguedUser"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<string> PaymentIntentAsync(int logguedUser, int productId, bool setupFutureUsage)
        {
            var user = await _userService.GetUserAsync(logguedUser);
            var product = await _productService.GetProductAsync(productId);

            var amount = GetProductPriceInteger(product);

            if (amount <= 0)
                throw new NotAllowedException("Price must to be great than 0");

            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                await CreateStripeCustomerAsync(user);
            }

            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "eur",
                Customer = user.StripeCustomerId,
                Description = "Buying product " + product.Name + ". {ID: " + product.Id + "}"
            };

            if (setupFutureUsage)
                options.SetupFutureUsage = "on_session";

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            var statusInfo = "Stripe Payment Intent Requested";
            await CreateOrderAsync(paymentIntent.ClientSecret, user, product, amount, statusInfo);

            return paymentIntent.ClientSecret;
        }

        /// <summary>
        /// Stripe Webhook
        /// </summary>
        /// <param name="paymentIntent"></param>
        /// <returns></returns>
        public async Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent)
        {
            var clientSecret = paymentIntent.ClientSecret;

            var order = await GetOrderAsync(clientSecret);

            if (order == null)
            {
                order = new Data.Entities.Order
                {
                    ExternalId = clientSecret,
                    Amount = paymentIntent.Amount / 100m,
                    Status = OrderStatusEnum.SUCCED,
                    StatusInformation = paymentIntent.ToString(),
                    PaymentMethod = PaymentMethodEnum.STRIPE,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await _uow.OrderRepository.AddAsync(order);
                await _uow.CommitAsync();
            }
            else
            {
                if (order.Status == OrderStatusEnum.SUCCED)
                    return;

                order.Status = OrderStatusEnum.SUCCED;
                order.StatusInformation = paymentIntent.Description;
                order.ModifiedAt = DateTime.UtcNow;
                await _uow.OrderRepository.UpdateAsync(order, order.Id);

                await HandleCoinsIncrementActionAsync(order);
                await _uow.CommitAsync();
            }
        }

        /// <summary>
        /// Stripe Webhook
        /// </summary>
        /// <param name="paymentIntent"></param>
        /// <returns></returns>
        public async Task HandlePaymentIntentFailed(PaymentIntent paymentIntent)
        {
            var clientSecret = paymentIntent.ClientSecret;
            var order = await GetOrderAsync(clientSecret);

            await HandlePaymentIntentNotSuccess(paymentIntent, order, OrderStatusEnum.FAILED);
        }

        /// <summary>
        /// Stripe Webhook
        /// </summary>
        /// <param name="paymentIntent"></param>
        /// <returns></returns>
        public async Task HandlePaymentIntentCanceled(PaymentIntent paymentIntent)
        {
            var clientSecret = paymentIntent.ClientSecret;
            var order = await GetOrderAsync(clientSecret);

            await HandlePaymentIntentNotSuccess(paymentIntent, order, OrderStatusEnum.CANCELED);
        }

        public async Task<IEnumerable<StripePaymentMethodResponse>> GetStripeCustomerPaymentMethods(int userId)
        {
            var user = await _userService.GetUserDevicesAsync(userId);

            var result = new List<StripePaymentMethodResponse>();

            if (!string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var options = new PaymentMethodListOptions
                {
                    Customer = user.StripeCustomerId,
                    Type = "card"
                };

                var service = new PaymentMethodService();
                var paymentMethods = await service.ListAsync(options);

                foreach (var method in paymentMethods)
                {
                    if (method.Type == "card" && method.Card != null)
                    {
                        var card = method.Card;
                        var mapped = new StripePaymentMethodResponse
                        {
                            PaymentMethodId = method.Id,
                            Last4 = card.Last4,
                            Country = card.Country,
                            Description = card.Description,
                            ExpMonth = card.ExpMonth,
                            ExpYear = card.ExpYear,
                            Funding = card.Funding,
                            Issuer = card.Issuer
                        };

                        result.Add(mapped);
                    }
                }
            }

            return result;
        }

        public async Task DeleteStripeCustomerPaymentMethod(int userId, string paymentMethodId)
        {
            var user = await _userService.GetUserDevicesAsync(userId);

            if (!string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var service = new PaymentMethodService();
                var paymentMethod = await service.GetAsync(paymentMethodId);

                if (paymentMethod != null)
                {
                    if (paymentMethod.CustomerId != user.StripeCustomerId)
                    {
                        throw new ForbiddenException("You dont have permission to remove this payment method");
                    }
                    else
                    {
                        await service.DetachAsync(paymentMethodId);
                    }
                }
            }
        }

        /// <summary>
        /// validates the receipt and returns the list of orders generated from that receipt. All
        /// payment not tracked in the api are processed and completed.
        /// </summary>
        /// <param name="userId">user which will receive all the unprocessed payments in the receipt</param>
        /// <param name="receiptData">receipt as a base64 string</param>
        /// <returns>List of orders processed</returns>
        public async Task<IEnumerable<Data.Entities.Order>> ValidateAppleReceiptAsync(int userId, string receiptData)
        {
            var orders = new List<Data.Entities.Order>();
            var receipt = _appleAppStoreService.GetReceipt(receiptData);
            var isReceiptValid = _appleAppStoreService.IsReceiptValid(receipt);

            if (isReceiptValid)
            {
                var user = await _userService.GetUserAsync(userId);
                foreach (var item in receipt.Receipts)
                {
                    var applProductId = item.ProductId;
                    Data.Entities.Product product = null;
                    // check if order does not exist and save it
                    var order = await GetOrderAsync(item.TransactionId);
                    // If order exist then this receipt was already processed
                    if (order == null)
                    {
                        switch (applProductId)
                        {
                            case "p500":
                                product = await _uow.ProductRepository.GetAll().FirstOrDefaultAsync(p => p.Value == 500 && p.Type == ProductEnum.COIN_OFFER);
                                break;

                            case "p2000":
                                product = await _uow.ProductRepository.GetAll().FirstOrDefaultAsync(p => p.Value == 2000 && p.Type == ProductEnum.COIN_OFFER);
                                break;

                            case "p3500":
                                product = await _uow.ProductRepository.GetAll().FirstOrDefaultAsync(p => p.Value == 3500 && p.Type == ProductEnum.COIN_OFFER);
                                break;

                            default:
                                break;
                        }

                        var statusInfo = "In App Purshase Completed";

                        if (product == null)
                        {
                            statusInfo = "Product not found in the server (" + applProductId + ")";
                            await CreateOrderAsync(item.TransactionId, user, product, 0, statusInfo, PaymentMethodEnum.IN_APP_PURSHASE_APPLE, OrderStatusEnum.FAILED);

                            Console.WriteLine(statusInfo);
                        }
                        else
                        {
                            var amount = GetProductPriceInteger(product);
                            order = await CreateOrderAsync(item.TransactionId, user, product, amount, statusInfo, PaymentMethodEnum.IN_APP_PURSHASE_APPLE, OrderStatusEnum.SUCCED);

                            // give coins to the user
                            await HandleCoinsIncrementActionAsync(order);
                            await _uow.CommitAsync();

                            orders.Add(order);
                        }
                    }
                }
            }

            return orders;
        }

        private async Task CreateStripeCustomerAsync(User user)
        {
            var options = new CustomerCreateOptions
            {
                Email = user.Email,
                Name = user.FullName,
                Phone = user.Phone,
                Description = "PlaniFive user"
            };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options);

            if (customer != null)
            {
                user.StripeCustomerId = customer.Id;
                await _uow.UserRepository.UpdateAsync(user, user.Id);
                await _uow.CommitAsync();
            }
        }

        private async Task HandlePaymentIntentNotSuccess(PaymentIntent paymentIntent, Data.Entities.Order order, OrderStatusEnum orderStatus)
        {
            var statusInformation = orderStatus == OrderStatusEnum.CANCELED ? paymentIntent.CancellationReason : paymentIntent.LastPaymentError?.Message;
            if (order == null)
            {
                order = new Data.Entities.Order
                {
                    ExternalId = paymentIntent.ClientSecret,
                    Amount = paymentIntent.Amount / 100m,
                    Status = orderStatus,
                    StatusInformation = statusInformation,
                    PaymentMethod = PaymentMethodEnum.STRIPE,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await _uow.OrderRepository.AddAsync(order);
                await _uow.CommitAsync();
            }
            else
            {
                order.Status = orderStatus;
                order.StatusInformation = statusInformation;
                order.ModifiedAt = DateTime.UtcNow;

                await _uow.OrderRepository.UpdateAsync(order, order.Id);
                await _uow.CommitAsync();
            }
        }

        private async Task HandleCoinsIncrementActionAsync(Data.Entities.Order order)
        {
            var user = order.User;
            var product = order.Product;

            if (user != null && product != null && product.Type == ProductEnum.COIN_OFFER)
            {
                await _userStatisticsService.UpdateTotalCoinsAsync(user, product.Value);
            }

            // TODO: Evaluate with Frontend devs y they need a PN with the success payment completed
        }

        private int GetProductPriceInteger(Data.Entities.Product product)
        {
            switch (product.Type)
            {
                case ProductEnum.COIN_OFFER:
                    return Convert.ToInt32(product.Price * 100);

                default:
                    return 0;
            }
        }

        private async Task<Data.Entities.Order> GetOrderAsync(string externalId)
        {
            var order = await _uow.OrderRepository.GetAll()
                .Include(o => o.User)
                .Include(o => o.Product)
                .Where(o => o.ExternalId == externalId)
                .FirstOrDefaultAsync();

            return order;
        }

        private async Task<Data.Entities.Order> CreateOrderAsync(string externalId, User user, Data.Entities.Product product, int amount,
            string statusInformation = "", PaymentMethodEnum paymentMethod = PaymentMethodEnum.STRIPE, OrderStatusEnum status = OrderStatusEnum.PROCESING)
        {
            var order = new Data.Entities.Order
            {
                ExternalId = externalId,
                UserId = user.Id,
                UserEmail = user.Email,
                UserFullName = user.FullName,
                ProductId = product?.Id,
                ProductName = product?.Name,
                ProductDescription = product?.Description,
                Amount = amount / 100m,
                Status = status,
                StatusInformation = statusInformation,
                PaymentMethod = paymentMethod,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            await _uow.OrderRepository.AddAsync(order);
            await _uow.CommitAsync();

            return order;
        }
    }
}
