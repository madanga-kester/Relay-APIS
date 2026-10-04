namespace Relay.Api.Middleware;

public sealed class CsrfMiddleware(RequestDelegate next)
{
    private const string CookieName = "relay.csrf";
    private const string HeaderName = "X-CSRF-TOKEN";

    public async Task InvokeAsync(HttpContext context)
    {
        var unsafeMethod = HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) || HttpMethods.IsPatch(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method);
        if (unsafeMethod && context.User.Identity?.IsAuthenticated == true && !context.Request.Path.StartsWithSegments("/health"))
        {
            var cookie = context.Request.Cookies[CookieName];
            var header = context.Request.Headers[HeaderName].ToString();
            if (string.IsNullOrWhiteSpace(cookie) || !string.Equals(cookie, header, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { title = "CSRF validation failed", status = 400, traceId = context.TraceIdentifier });
                return;
            }
        }
        await next(context);
    }
}
