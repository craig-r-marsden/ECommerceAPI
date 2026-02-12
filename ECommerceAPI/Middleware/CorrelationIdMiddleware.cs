namespace ECommerceAPI.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items["CorrelationId"] = correlationId;

        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

        _logger.LogInformation(
            "Incoming request: {Method} {Path} with Correlation-ID: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId
        );

        await _next(context);

        _logger.LogInformation(
            "Outgoing response: {StatusCode} for {Method} {Path} with Correlation-ID: {CorrelationId}",
            context.Response.StatusCode,
            context.Request.Method,
            context.Request.Path,
            correlationId
        );
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
