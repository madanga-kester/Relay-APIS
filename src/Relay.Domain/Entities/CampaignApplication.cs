using Relay.Domain.Enums;

namespace Relay.Domain.Entities;

public sealed class CampaignApplication : AuditableEntity
{
    private CampaignApplication() { }

    public CampaignApplication(Guid campaignId, Guid communityId, Guid communityOwnerId, decimal cpc)
    {
        CampaignId = campaignId;
        CommunityId = communityId;
        CommunityOwnerId = communityOwnerId;
        Cpc = cpc;
    }

    public Guid CampaignId { get; private set; }
    public Guid CommunityId { get; private set; }
    public Guid CommunityOwnerId { get; private set; }
    public decimal Cpc { get; private set; }
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Pending;
    public DateTimeOffset? ReviewedAt { get; private set; }
    public string? ReviewReason { get; private set; }
    public Placement? Placement { get; private set; }

    public void Accept(DateTimeOffset now)
    {
        if (Status != ApplicationStatus.Pending) throw new InvalidOperationException("Only pending applications can be accepted.");
        Status = ApplicationStatus.Accepted;
        ReviewedAt = now;
        Placement = new Placement(Id, CampaignId, CommunityId, CommunityOwnerId);
    }

    public void Reject(DateTimeOffset now, string? reason = null)
    {
        if (Status != ApplicationStatus.Pending) throw new InvalidOperationException("Only pending applications can be rejected.");
        Status = ApplicationStatus.Rejected;
        ReviewedAt = now;
        ReviewReason = reason?.Trim();
    }
}
