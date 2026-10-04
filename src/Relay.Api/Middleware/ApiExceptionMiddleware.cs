using System.Net;
using System.Text.Json;
using Relay.Application.Common;

namespace Relay.Api.Middleware;

public sealed partial class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception exception)
        {
            var (status, title, errors) = exception switch
            {
                ValidationFailureException validation => ((int)HttpStatusCode.BadRequest, "Validation failed", validation.Errors),
                NotFoundException => ((int)HttpStatusCode.NotFound, "Resource not found", Array.Empty<string>()),
                ForbiddenOperationException forbidden => ((int)HttpStatusCode.Forbidden, forbidden.Message, Array.Empty<string>()),
                ConflictException conflict => ((int)HttpStatusCode.Conflict, conflict.Message, Array.Empty<string>()),
                _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.", Array.Empty<string>())
            };
            if (status >= 500) LogUnhandled(logger, exception, context.Request.Path);
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { type = "https://relay.example/problems/api-error", title, status, traceId = context.TraceIdentifier, errors }));
        }
    }

    [LoggerMessage(EventId = 1000, Level = LogLevel.Error, Message = "Unhandled API exception for {Path}")]
    private static partial void LogUnhandled(ILogger logger, Exception exception, string path);
}
