using System;
using System.Security.Claims;

namespace survey_pro.Middlewares;

public class RoleAuthorizationLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RoleAuthorizationLoggingMiddleware> _logger;

    public RoleAuthorizationLoggingMiddleware(RequestDelegate next, ILogger<RoleAuthorizationLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity!.IsAuthenticated)
        {
            var userId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var roles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);

            _logger.LogInformation(
                "User {UserId} with roles {Roles} accessing {Path}",
                userId,
                string.Join(", ", roles),
                context.Request.Path);
        }

        await _next(context);
    }
}

public static class RoleAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseRoleAuthorizationLoggingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RoleAuthorizationLoggingMiddleware>();
    }
}
