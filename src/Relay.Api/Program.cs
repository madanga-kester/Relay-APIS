
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Relay.Api.Auth;
using Relay.Api.Health;
using Relay.Api.Middleware;
using Relay.Application.Common;
using Relay.Infrastructure;
using Relay.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
if (builder.Environment.IsDevelopment()) builder.Services.AddSwaggerGen();
var dataProtection = builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration["Auth:DataProtectionKeysPath"] ?? Path.Combine(AppContext.BaseDirectory, "data", "keys")));
var certificatePath = builder.Configuration["Auth:DataProtectionCertificatePath"];
if (!builder.Environment.IsDevelopment())
{
    if (string.IsNullOrWhiteSpace(certificatePath) || !File.Exists(certificatePath)) throw new InvalidOperationException("Production requires Auth:DataProtectionCertificatePath for encrypted key storage.");
    dataProtection.ProtectKeysWithCertificate(new X509Certificate2(certificatePath, builder.Configuration["Auth:DataProtectionCertificatePassword"]));
}
builder.Services.AddRelayPersistence(builder.Configuration);
builder.Services.AddRelayInfrastructure();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
    if (allowedOrigins.Length > 0) policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));

var requireHttps = builder.Configuration.GetValue("Auth:RequireHttps", !builder.Environment.IsDevelopment());
var crossSiteCookies = builder.Configuration.GetValue("Auth:CrossSiteCookies", false);
builder.Services.AddAuthentication("RelayCookie").AddCookie("RelayCookie", options =>
{
    options.Cookie.Name = builder.Configuration["Auth:CookieName"] ?? "relay.auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = requireHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = crossSiteCookies ? SameSiteMode.None : SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(builder.Configuration.GetValue("Auth:SessionHours", 12));
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context => RateLimitPartition.GetFixedWindowLimiter("api", _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
});
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("postgres", tags: ["ready"]);

var app = builder.Build();
app.UseMiddleware<ApiExceptionMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<CsrfMiddleware>();
app.UseAuthorization();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.Run();

public partial class Program { }
