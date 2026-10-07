using Relay.Application.Contracts;
using Relay.Application.Common;

namespace Relay.Application.Services;

public interface IAuthenticationService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<UserResponse?> ValidateCredentialsAsync(LoginRequest request, CancellationToken cancellationToken);
    Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);


    Task<bool> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken);
    Task ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken);
}

public interface ICampaignService
{
    Task<PageResult<CampaignResponse>> ListAsync(PageRequest page, CancellationToken cancellationToken);

    Task<IReadOnlyList<BillingActivityResponse>> BillingActivityAsync(CancellationToken cancellationToken);
    Task<PageResult<CampaignResponse>> MineAsync(PageRequest page, CancellationToken cancellationToken);

    
    Task<IReadOnlyList<CampaignPerformanceResponse>> PerformanceAsync(CancellationToken cancellationToken);
    Task<CampaignResponse> CreateAsync(CreateCampaignRequest request, CancellationToken cancellationToken);
    Task<CampaignResponse?> UpdateAsync(Guid id, UpdateCampaignRequest request, CancellationToken cancellationToken);
    Task<CampaignResponse?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<CampaignResponse?> TransitionAsync(Guid id, string action, CancellationToken cancellationToken);
}

public interface ICommunityService
{
    Task<PageResult<CommunityResponse>> ListAsync(PageRequest page, CancellationToken cancellationToken);
    Task<PageResult<CommunityResponse>> MineAsync(PageRequest page, CancellationToken cancellationToken);
    Task<CommunityResponse> CreateAsync(CreateCommunityRequest request, CancellationToken cancellationToken);
}

public interface IApplicationService
{
    Task<ApplicationResponse> ApplyAsync(ApplyToCampaignRequest request, CancellationToken cancellationToken);
    Task<PageResult<ApplicationResponse>> MineAsync(PageRequest page, CancellationToken cancellationToken);
    Task<ApplicationResponse?> ReviewAsync(Guid id, ReviewApplicationRequest request, CancellationToken cancellationToken);
}

public interface ITrackingService
{
    Task<TrackingClickResponse?> ProcessClickAsync(string trackingId, string? idempotencyKey, string? visitorKey, CancellationToken cancellationToken);
}

public interface IProfileService
{
    Task<ProfileResponse?> GetAsync(CancellationToken cancellationToken);
    Task<ProfileResponse> SaveAsync(SaveProfileRequest request, CancellationToken cancellationToken);
}

public interface IAdminOperationsService
{
    Task<AdminNoteResponse> AddNoteAsync(CreateAdminNoteRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteNoteAsync(Guid id, CancellationToken cancellationToken);
    Task<PageResult<AdminNoteResponse>> ListNotesAsync(string? entityType, Guid? entityId, PageRequest page, CancellationToken cancellationToken);
    Task<ReviewCaseResponse> CreateReviewCaseAsync(CreateReviewCaseRequest request, CancellationToken cancellationToken);
    Task<ReviewCaseResponse?> ResolveReviewCaseAsync(Guid id, ResolveReviewCaseRequest request, CancellationToken cancellationToken);
    Task<PageResult<ReviewCaseResponse>> ListReviewCasesAsync(PageRequest page, CancellationToken cancellationToken);
    Task<PageResult<PayoutResponse>> ListPayoutsAsync(PageRequest page, CancellationToken cancellationToken);
    Task<PayoutResponse?> ChangePayoutStatusAsync(Guid id, ChangePayoutStatusRequest request, CancellationToken cancellationToken);
    Task<PageResult<AdminNotificationResponse>> ListNotificationsAsync(PageRequest page, CancellationToken cancellationToken);
    Task<AdminNotificationResponse?> MarkNotificationReadAsync(Guid id, CancellationToken cancellationToken);
    Task<AdminSettingResponse?> GetSettingAsync(string key, CancellationToken cancellationToken);
    Task<AdminSettingResponse> SaveSettingAsync(string key, SaveAdminSettingRequest request, CancellationToken cancellationToken);
    Task<bool> ChangeUserStatusAsync(Guid id, ChangeUserStatusRequest request, CancellationToken cancellationToken);
    Task<bool> ChangeCommunityStatusAsync(Guid id, ChangeCommunityStatusRequest request, CancellationToken cancellationToken);
    Task<bool> ChangePlacementStatusAsync(Guid id, ChangePlacementStatusRequest request, CancellationToken cancellationToken);
}

public interface IAdminOverviewService
{
    Task<AdminOverviewResponse> GetAsync(CancellationToken cancellationToken);
}
public interface IAdminReportingService
{
    Task<AdminReportResponse> GetReportAsync(DateTimeOffset? from, DateTimeOffset? toDate, CancellationToken cancellationToken);
    Task<AdminHealthResponse> GetHealthAsync(CancellationToken cancellationToken);
}
