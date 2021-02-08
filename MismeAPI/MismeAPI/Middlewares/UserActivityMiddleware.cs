using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MismeAPI.BasicResponses;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data;
using MismeAPI.Service;
using MismeAPI.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Middlewares
{
    public class UserActivityMiddleware
    {
        private readonly RequestDelegate _next;

        public UserActivityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            await _next.Invoke(context);
            //handle response
            //you may also need to check the request path to check whether it requests image
            if (context.User.Identity.IsAuthenticated)
            {
                var id = context.User?.GetUserIdFromToken();
                if (id.HasValue)
                {
                    await userService.SetUserLatestAccessAsync(id.Value);
                }
            }
        }
    }
}
