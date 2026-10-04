namespace Relay.Domain.Entities;

public sealed class UserProfile : AuditableEntity
{
    private UserProfile() { }

    public UserProfile(Guid userId) => UserId = userId;

    public Guid UserId { get; private set; }
    public string? BusinessName { get; private set; }
    public string? Industry { get; private set; }
    public string? Website { get; private set; }
    public string? Location { get; private set; }
    public string? PrimaryGoal { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? AvatarKey { get; private set; }
    public bool OnboardingCompleted { get; private set; }
    public string? CommunityName { get; private set; }
    public string? CommunityPlatform { get; private set; }
    public int? CommunityMembers { get; private set; }
    public string? CommunityCategory { get; private set; }

    public void Update(string? businessName, string? industry, string? website, string? location, string? primaryGoal,
        string? phoneNumber, string? avatarKey, bool onboardingCompleted, string? communityName, string? communityPlatform,
        int? communityMembers, string? communityCategory, DateTimeOffset now)
    {
        BusinessName = Clean(businessName); Industry = Clean(industry); Website = Clean(website); Location = Clean(location);
        PrimaryGoal = Clean(primaryGoal); PhoneNumber = Clean(phoneNumber); AvatarKey = Clean(avatarKey);
        CommunityName = Clean(communityName); CommunityPlatform = Clean(communityPlatform); CommunityMembers = communityMembers; CommunityCategory = Clean(communityCategory);
        OnboardingCompleted = onboardingCompleted; Touch(now);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
