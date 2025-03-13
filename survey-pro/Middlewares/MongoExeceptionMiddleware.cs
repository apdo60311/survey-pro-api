using MongoDB.Driver;

public class MongoDBExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MongoDBExceptionMiddleware> _logger;

    public MongoDBExceptionMiddleware(RequestDelegate next, ILogger<MongoDBExceptionMiddleware> logger)
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
        catch (MongoConnectionException ex)
        {
            _logger.LogError(ex, "MongoDB Connection Error");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Database connection error",
                message = "Unable to connect to the database"
            });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "MongoDB Timeout Error");
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Database timeout",
                message = "The database operation timed out"
            });
        }
    }
}
