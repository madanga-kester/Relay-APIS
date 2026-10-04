using Relay.Domain.Enums;

namespace Relay.Application.Contracts;

public sealed record RegisterRequest(string Email, string DisplayName, string Password, UserRole Role);
public sealed record LoginRequest(string Email, string Password);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Token, string NewPassword);
public sealed record VerifyEmailRequest(string Email, string Code);
public sealed record ResendVerificationRequest(string Email);

public sealed record UserResponse(Guid Id, string Email, string DisplayName, UserRole Role, AccountStatus Status);

public sealed record SaveProfileRequest(string? BusinessName, string? Industry, string? Website, string? Location, string? PrimaryGoal,
    string? PhoneNumber, string? AvatarKey, bool OnboardingCompleted, string? CommunityName, string? CommunityPlatform,
    int? CommunityMembers, string? CommunityCategory);
public sealed record ProfileResponse(Guid UserId, string? BusinessName, string? Industry, string? Website, string? Location, string? PrimaryGoal,
    string? PhoneNumber, string? AvatarKey, bool OnboardingCompleted, string? CommunityName, string? CommunityPlatform,
    int? CommunityMembers, string? CommunityCategory);

public sealed record CreateCampaignRequest(string Name, string AdvertiserName, string Description, string Advertisement, string DestinationUrl,
    string[] Platforms, int MinimumAudience, int MaximumAudience, string Category, string Location, int DurationDays,
    int MaximumCommunities, decimal Cpc, decimal Budget, DateOnly StartDate, DateOnly EndDate)
{
    public CreateCampaignRequest(string name, string description, string advertisement, string destinationUrl, decimal cpc, decimal budget, int maximumCommunities, string category, string location)
        : this(name, name, description, advertisement, destinationUrl, ["WhatsApp"], 1, int.MaxValue, category, location, 7, maximumCommunities, cpc, budget, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))) { }
}
public sealed record UpdateCampaignRequest(string Name, string AdvertiserName, string Description, string Advertisement, string DestinationUrl,
    string[] Platforms, int MinimumAudience, int MaximumAudience, string Category, string Location, int DurationDays,
    int MaximumCommunities, decimal Cpc, decimal Budget, DateOnly StartDate, DateOnly EndDate);
public sealed record CampaignResponse(Guid Id, Guid AdvertiserId, string Name, string AdvertiserName, string Description, string Advertisement,
    string DestinationUrl, IReadOnlyCollection<string> Platforms, int MinimumAudience, int MaximumAudience, string Category, string Location,
    int DurationDays, int MaximumCommunities, decimal Cpc, decimal Budget, DateOnly StartDate, DateOnly EndDate, CampaignStatus Status);

public sealed record CreateCommunityRequest(string Name, CommunityPlatform Platform, int Members, string Category, string Location,
    string? CommunityLink, string AudienceDescription, string? VerificationEvidenceKey);
public sealed record CommunityResponse(Guid Id, Guid OwnerId, string Name, CommunityPlatform Platform, int Members, string Category,
    string Location, string? CommunityLink, string AudienceDescription, string? VerificationEvidenceKey, VerificationStatus VerificationStatus);

public sealed record ApplyToCampaignRequest(Guid CampaignId, Guid CommunityId);
public sealed record ReviewApplicationRequest(bool Accept, string? Reason);
public sealed record ApplicationResponse(Guid Id, Guid CampaignId, Guid CommunityId, Guid CommunityOwnerId, decimal Cpc, ApplicationStatus Status, PlacementResponse? Placement);
public sealed record PlacementResponse(Guid Id, Guid CampaignId, Guid CommunityId, Guid CommunityOwnerId, string TrackingId, PlacementStatus Status);
public sealed record TrackingClickResponse(string ClickId, ClickQualification Qualification, string? RejectionReason, string DestinationUrl);

public sealed record CreateAdminNoteRequest(string EntityType, Guid EntityId, string EntityName, string Text);
public sealed record AdminNoteResponse(Guid Id, string EntityType, Guid EntityId, string EntityName, string Text, Guid AuthorId, DateTimeOffset CreatedAt);
public sealed record CreateReviewCaseRequest(string EntityType, Guid EntityId, string EntityName, string Reason);
public sealed record ResolveReviewCaseRequest(ReviewCaseStatus Status, string Action);
public sealed record ReviewCaseResponse(Guid Id, string EntityType, Guid EntityId, string EntityName, string Reason, ReviewCaseStatus Status, string? ResolutionAction, DateTimeOffset CreatedAt, DateTimeOffset? ResolvedAt);
public sealed record PayoutResponse(Guid Id, Guid CommunityOwnerId, Guid CampaignId, Guid PlacementId, decimal Amount, PayoutStatus Status, string? FailureReason, DateTimeOffset CreatedAt);
public sealed record ChangePayoutStatusRequest(PayoutStatus Status, string? FailureReason);
public sealed record AdminNotificationResponse(Guid Id, string Type, string Title, string Description, string Href, Guid? EntityId, bool Read, DateTimeOffset CreatedAt);
public sealed record AdminSettingResponse(string Key, string Value);
public sealed record SaveAdminSettingRequest(string Value);
public sealed record ChangeUserStatusRequest(AccountStatus Status);
public sealed record ChangeCommunityStatusRequest(VerificationStatus Status);
public sealed record ChangePlacementStatusRequest(PlacementStatus Status);
public sealed record AdminOverviewResponse(int TotalUsers, int Advertisers, int CommunityOwners, int Campaigns, int ActiveCampaigns, int Communities, int ActiveCommunities, int Applications, int PendingApplications, int Placements, int ActivePlacements, int QualifiedClicks, decimal AdvertiserSpend, decimal CommunityOwnerEarnings, decimal PlatformRevenue);
public sealed record AdminReportResponse(DateTimeOffset? From, DateTimeOffset? To, int Campaigns, int ActiveCampaigns, int Communities, int ActiveCommunities, int Applications, int Placements, int QualifiedClicks, decimal AdvertiserSpend, decimal CommunityOwnerEarnings, decimal PlatformRevenue, decimal ReconciliationDelta);
public sealed record AdminHealthResponse(int ActiveCampaigns, int ActivePlacements, int QualifiedClicksLast24Hours, int FailedEventsLast24Hours, int ActivityEventsLast24Hours, bool FinancialsReconciled);
