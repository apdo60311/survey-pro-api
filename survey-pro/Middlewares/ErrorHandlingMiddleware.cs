using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using survey_pro.Models;

namespace survey_pro.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred";
            var errorCode = "INTERNAL_SERVER_ERROR";

            // Customize response based on exception type
            switch (exception)
            {
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = "The requested resource was not found";
                    errorCode = "RESOURCE_NOT_FOUND";
                    break;
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Unauthorized access";
                    errorCode = "UNAUTHORIZED";
                    break;
                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    errorCode = "BAD_REQUEST";
                    break;
                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    errorCode = "INVALID_OPERATION";
                    break;
                case CryptographicException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Invalid cryptographic operation";
                    errorCode = "CRYPTOGRAPHIC_ERROR";
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "An unexpected error occurred";
                    errorCode = "INTERNAL_SERVER_ERROR";
                    break;
            }

            var response = new ErrorResponse
            {
                StatusCode = (int)statusCode,
                ErrorCode = errorCode,
                Message = message,
                Detail = exception.Message,
                TraceId = context.TraceIdentifier
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }

    public static class ExeceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}