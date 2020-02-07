using Microsoft.AspNetCore.Http;
using MismeAPI.BasicResponses;
using MismeAPI.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
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

        public async Task Invoke(HttpContext context)
        {
            Message = "";
            try
            {
                await _next.Invoke(context);
            }
            catch (NotAllowedException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                Message = ex.Message;
            }
            catch (NotFoundException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                Message = String.IsNullOrWhiteSpace(ex.Entity) ? ex.Message : ex.Entity + ex.Message;
            }
            catch (InvalidDataException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                Message = String.IsNullOrWhiteSpace(ex.Field) ? ex.Message : ex.Field + ex.Message;
            }
            catch (AlreadyExistsException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                Message = ex.Message;
            }
            catch (UnauthorizedException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                Message = ex.Message;
            }
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