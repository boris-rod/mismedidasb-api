using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Request.Subscription;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.Subscription;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Service;
using MismeAPI.Service.Hubs;
using MismeAPI.Service.Utils;
using MismeAPI.Services;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace APITaxi.API.Controllers
{
    [Route("api/subscriptions")]
    public class SubscriptionController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private IHubContext<UserHub> _hub;
        private IUserReferralService _userReferralService;
        private IRewardHelper _rewardHelper;
        private IUserService _userService;
        private ISubscriptionService _subscriptionService;

        public SubscriptionController(IAccountService accountService, IMapper mapper, IEmailService emailService, IWebHostEnvironment env,
            IHubContext<UserHub> hub, IUserReferralService userReferralService, IRewardHelper rewardHelper, IUserService userService, ISubscriptionService subscriptionService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
            _userReferralService = userReferralService ?? throw new ArgumentNullException(nameof(userReferralService));
            _rewardHelper = rewardHelper ?? throw new ArgumentNullException(nameof(rewardHelper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        }

        /// <summary>
        /// Get active subscriptions. Require admin authentication
        /// </summary>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of eats to be displayed per page. 10 by default.</param>
        /// <param name="sortOrder">field_asc or field_desc for ordering</param>
        /// <param name="search">search a text/numer</param>
        /// <returns>subscription</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<SubscriptionResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index(int? page, int? perPage, string sortOrder, string search)
        {
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _subscriptionService.GetSubscriptionsAsync(pag, perPag, sortOrder, true, search);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";
            var mapped = _mapper.Map<IEnumerable<SubscriptionResponse>>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get subscriptions. Require admin authentication
        /// </summary>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of eats to be displayed per page. 10 by default.</param>
        /// <param name="sortOrder">field_asc or field_desc for ordering</param>
        /// <param name="isActive">Filter subscription by status active or not</param>
        /// <param name="search">search a text/numer</param>
        /// <returns>subscription</returns>
        [HttpGet("admin")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(IEnumerable<SubscriptionResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index(int? page, int? perPage, string sortOrder, bool? isActive, string search)
        {
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _subscriptionService.GetSubscriptionsAsync(pag, perPag, sortOrder, isActive, search);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";
            var mapped = _mapper.Map<IEnumerable<SubscriptionResponse>>(result);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Create a subscription
        /// </summary>
        /// <param name="request">Subscription object</param>
        /// <returns>User with subscription object</returns>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(SubscriptionResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AssignSubscription(CreateSubscriptionRequest request)
        {
            var subscription = await _subscriptionService.CreateSubscriptionAsync(request);

            var mapped = _mapper.Map<SubscriptionResponse>(subscription);

            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Update a subscription
        /// </summary>
        /// <param name="id">Subscription id</param>
        /// <param name="request">Subscription object</param>
        /// <returns>User with subscription object</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(SubscriptionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update([FromRoute]int id, UpdateSubscriptionRequest request)
        {
            var subscription = await _subscriptionService.UpdateSubscriptionAsync(id, request);

            var mapped = _mapper.Map<SubscriptionResponse>(subscription);

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a subscripton. Only an admin can do this operation. Requires authentication.
        /// </summary>
        /// <param name="id">subscription id to delete.</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteCutPoint([FromRoute]int id)
        {
            await _subscriptionService.DeleteSubscriptionAsync(id);
            return Ok();
        }

        /// <summary>
        /// Assign subscription to an user
        /// </summary>
        /// <param name="userId">User who wich the admin will add a subscription</param>
        /// <param name="subscription">Subscription to assign to this user</param>
        /// <returns>User with subscription object</returns>
        [HttpPost]
        [Route("/assign-subscription")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(UserWithSubscriptionResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AssignSubscription(int userId, SubscriptionEnum subscription)
        {
            var user = await _userService.GetUserAsync(userId);

            await _subscriptionService.AssignSubscriptionAsync(userId, subscription);

            var mapped = _mapper.Map<UserWithSubscriptionResponse>(user);

            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Buy a subscription
        /// </summary>
        /// <param name="id">Id of the subscription to buy</param>
        /// <returns>User with subscription object</returns>
        [HttpPost]
        [Route("{id}/buy")]
        [Authorize]
        [ProducesResponseType(typeof(UserSubscriptionResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> BuySubscription(int id)
        {
            var loggedUser = User.GetUserIdFromToken();

            var userSubscription = await _subscriptionService.BuySubscriptionAsync(loggedUser, id);
            var mapped = _mapper.Map<UserSubscriptionResponse>(userSubscription);

            return Created("", new ApiOkResponse(mapped));
        }
    }
}
