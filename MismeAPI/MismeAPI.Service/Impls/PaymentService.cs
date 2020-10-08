﻿using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using DeviceDetectorNET.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Service;
using MismeAPI.Service.Utils;
using rlcx.suid;
using Stripe;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wangkanai.Detection;

namespace MismeAPI.Services.Impls
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _uow;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IUserStatisticsService _userStatisticsService;

        public PaymentService(IConfiguration configuration, IUnitOfWork uow, IProductService productService,
            IUserService userService, IUserStatisticsService userStatisticsService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userStatisticsService = userStatisticsService ?? throw new ArgumentNullException(nameof(userStatisticsService));
        }

        /// <summary>
        /// Create a Stripe Payment Intent Auth
        /// </summary>
        /// <param name="logguedUser"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<string> PaymentIntentAsync(int logguedUser, int productId)
        {
            var user = await _userService.GetUserAsync(logguedUser);
            var product = await _productService.GetProductAsync(productId);

            var amount = GetProductPriceForStripe(product);

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
                    Amount = Convert.ToDecimal(paymentIntent.Amount / 100),
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

                await HandlePointsIncrementActionAsync(order);
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

        public async Task<StripeList<PaymentMethod>> GetStripeCustomerPaymentMethods(int userId)
        {
            var user = await _userService.GetUserAsync(userId);
            var options = new PaymentMethodListOptions
            {
                Customer = user.StripeCustomerId,
                Type = "card",
            };

            var service = new PaymentMethodService();
            var paymentMethods = await service.ListAsync(options);

            return paymentMethods;
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
                    Amount = Convert.ToDecimal(paymentIntent.Amount / 100),
                    Status = orderStatus,
                    StatusInformation = statusInformation,
                    PaymentMethod = PaymentMethodEnum.STRIPE,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await _uow.OrderRepository.AddAsync(order);
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

        private async Task HandlePointsIncrementActionAsync(Data.Entities.Order order)
        {
            var user = order.User;
            var product = order.Product;

            if (user != null && product != null && product.Type == ProductEnum.POINT_OFFER)
            {
                await _userStatisticsService.UpdateTotalPoints(user, product.Value);
            }

            // TODO: Evaluate with Frontend devs y they need a PN with the success payment completed
        }

        private int GetProductPriceForStripe(Data.Entities.Product product)
        {
            switch (product.Type)
            {
                case ProductEnum.POINT_OFFER:
                    return Convert.ToInt32(product.Price * 100);

                default:
                    return 0;
            }
        }

        private async Task<Data.Entities.Order> GetOrderAsync(string externalId)
        {
            var order = await _uow.OrderRepository.GetAll()
                .Include(o => o.User)
                .Where(o => o.ExternalId == externalId)
                .FirstOrDefaultAsync();

            return order;
        }

        private async Task<Data.Entities.Order> CreateOrderAsync(string clientSecret, User user, Data.Entities.Product product, int amount, string StatusInformation = "")
        {
            var order = new Data.Entities.Order
            {
                ExternalId = clientSecret,
                UserId = user.Id,
                UserEmail = user.Email,
                UserFullName = user.FullName,
                ProductId = product.Id,
                ProductName = product.Name,
                ProductDescription = product.Description,
                Amount = Convert.ToDecimal(amount / 100),
                Status = OrderStatusEnum.PROCESING,
                StatusInformation = "Stripe Payment Intent Requested",
                PaymentMethod = PaymentMethodEnum.STRIPE,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            await _uow.OrderRepository.AddAsync(order);
            await _uow.CommitAsync();

            return order;
        }
    }
}
