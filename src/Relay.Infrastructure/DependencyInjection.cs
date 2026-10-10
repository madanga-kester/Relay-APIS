using Microsoft.Extensions.DependencyInjection;
using Relay.Application.Common;
using Relay.Application.Financial;
using Relay.Application.Services;
using Relay.Infrastructure.Auth;
using Relay.Infrastructure.Email;
using Relay.Infrastructure.Marketplace;
using Relay.Infrastructure.Storage;

namespace Relay.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRelayInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IFinancialCalculator, FinancialCalculator>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ICommunityService, CommunityService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<ITrackingService, TrackingService>();
        services.AddScoped<IPlacementService, PlacementService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPreferencesService, PreferencesService>();
        services.AddScoped<IActivityFeedService, ActivityFeedService>();
        services.AddScoped<IAdminOperationsService, AdminOperationsService>();
        services.AddScoped<IAdminOverviewService, AdminOverviewService>();
        services.AddScoped<IAdminReportingService, AdminReportingService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<IObjectStorage, S3ObjectStorage>();
        return services;
    }
}
