using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Response.Order;
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
        /// <param name="setupFutureUsage">
        /// Define if the user want to save the card for future on_session payments
        /// </param>
        /// <returns>Client secret for payment intent</returns>
        [Authorize]
        [HttpPost("create-stripe-payment-intent")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreatePaymentIntentToken([FromQuery] int productId, [FromQuery] bool setupFutureUsage)
        {
            var loggedUser = User.GetUserIdFromToken();

            var clientSecret = await _paymentService.PaymentIntentAsync(loggedUser, productId, setupFutureUsage);

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

        /// <summary>
        /// Delete a payment method used in stripe
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("stripe-payment-methods")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteStripePaymentMethod([FromQuery] string paymentMethodId)
        {
            var loggedUser = User.GetUserIdFromToken();

            await _paymentService.DeleteStripeCustomerPaymentMethod(loggedUser, paymentMethodId);

            return NoContent();
        }

        /// <summary>
        /// Verify a success Apple In App Purshase
        /// </summary>
        /// <param name="receipt">Receipt in Base64 string sent by apple in the payment process</param>
        /// <returns>
        /// List of Orders generated in this verification process after processed and give the
        /// points to the user. Use ExternalId as the TransactionId that need to be completed.
        /// </returns>
        [Authorize]
        [HttpPost("verify-apple-in-app-purshase")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> VirifyAppleInAppPurshase([FromBody] string receipt)
        {
            var loggedUser = User.GetUserIdFromToken();

            var orders = await _paymentService.ValidateAppleReceiptAsync(loggedUser, receipt);
            var mapped = _mapper.Map<IEnumerable<OrderResponse>>(orders);

            return Created("Verified", new ApiOkResponse(mapped));
        }
    }
}
