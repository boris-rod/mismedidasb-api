using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Response.Payment;
using MismeAPI.Services;
using MismeAPI.Utils;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/payment")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public PaymentController(IPaymentService paymentService, IMapper mapper, IConfiguration config)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Init a stripe payment intent
        /// </summary>
        /// <param name="productId">Product that the current user will buy</param>
        /// <returns>Client secret for payment intent</returns>
        [Authorize]
        [HttpPost("create-stripe-payment-intent")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreatePaymentIntentToken([FromQuery] int productId)
        {
            var loggedUser = User.GetUserIdFromToken();

            var clientSecret = await _paymentService.PaymentIntentAsync(loggedUser, productId);

            return Created("Payment Intent", new ApiOkResponse(new { clientSecret }));
        }

        /// <summary>
        /// Handle Stripe webhooks
        /// </summary>
        /// <returns></returns>
        [HttpPost("stripe-payment-intent-webhook")]
        public async Task<IActionResult> StripePaymentIntentWebHook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var endpointSecret = _config.GetSection("Stripe")["EndpointSecret"];

            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);

                var signatureHeader = Request.Headers["Stripe-Signature"];
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, endpointSecret);

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    // Then define and call a method to handle the successful payment intent. handlePaymentIntentSucceeded(paymentIntent);
                    await _paymentService.HandlePaymentIntentSucceeded(paymentIntent);
                }
                else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await _paymentService.HandlePaymentIntentFailed(paymentIntent);
                }
                else if (stripeEvent.Type == Events.PaymentIntentCanceled)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await _paymentService.HandlePaymentIntentCanceled(paymentIntent);
                }
                else
                {
                    // Unexpected event type
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }
                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine(e.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// Get current user payment methods used in stripe
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("stripe-payment-methods")]
        [ProducesResponseType(typeof(IEnumerable<StripePaymentMethodResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetStripePaymentMethods()
        {
            var loggedUser = User.GetUserIdFromToken();

            var paymentMethods = await _paymentService.GetStripeCustomerPaymentMethods(loggedUser);

            return Ok(new ApiOkResponse(paymentMethods));
        }
    }
}
