using Relay.Domain.Enums;

namespace Relay.Domain.Entities;

public sealed class Community : AuditableEntity
{
    private Community() { }

    public Community(Guid ownerId, string name, CommunityPlatform platform, int members, string category, string location,
        string? communityLink, string audienceDescription, string? verificationEvidenceKey)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(members);
        OwnerId = ownerId;
        Name = name.Trim();
        Platform = platform;
        Members = members;
        Category = category.Trim();
        Location = location.Trim();
        CommunityLink = string.IsNullOrWhiteSpace(communityLink) ? null : communityLink.Trim();
        AudienceDescription = audienceDescription.Trim();
        VerificationEvidenceKey = string.IsNullOrWhiteSpace(verificationEvidenceKey) ? null : verificationEvidenceKey.Trim();
    }

    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CommunityPlatform Platform { get; private set; }
    public int Members { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string? CommunityLink { get; private set; }
    public string AudienceDescription { get; private set; } = string.Empty;
    public string? VerificationEvidenceKey { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; } = VerificationStatus.Pending;

    public void Verify() => VerificationStatus = VerificationStatus.Verified;
    public void Suspend() => VerificationStatus = VerificationStatus.Suspended;
    public void Restore() => VerificationStatus = VerificationStatus.Verified;
}
