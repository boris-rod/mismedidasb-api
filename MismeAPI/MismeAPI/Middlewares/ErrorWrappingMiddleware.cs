using Microsoft.AspNetCore.Http;
using MismeAPI.BasicResponses;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace MismeAPI.Middlewares
{
    public class ErrorWrappingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorWrappingMiddleware(RequestDelegate next)
        {
            _next = next;
            Message = "";
        }

        private string Message { get; set; }
        private int CustomStatusCode { get; set; }

        public async Task Invoke(HttpContext context)
        {
            Message = "";
            CustomStatusCode = 0;
            try
            {
                await _next.Invoke(context);
            }

            ///add other exceptions
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                Message = ex.Message;
            }

            if (!context.Response.HasStarted && context.Response.StatusCode != 204)
            {
                context.Response.ContentType = "application/json";

                var response = new ApiResponse(context.Response.StatusCode, Message ?? "");

                var json = JsonConvert.SerializeObject(response, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                await context.Response.WriteAsync(json);
            }
        }
    }
}