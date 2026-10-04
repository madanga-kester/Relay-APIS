using Relay.Domain.Enums;

namespace Relay.Domain.Entities;

public sealed class Placement : AuditableEntity
{
    private Placement() { }

    public Placement(Guid applicationId, Guid campaignId, Guid communityId, Guid communityOwnerId)
    {
        ApplicationId = applicationId;
        CampaignId = campaignId;
        CommunityId = communityId;
        CommunityOwnerId = communityOwnerId;
        TrackingId = Convert.ToHexString(Guid.NewGuid().ToByteArray()).ToLowerInvariant();
    }

    public Guid ApplicationId { get; private set; }
    public Guid CampaignId { get; private set; }
    public Guid CommunityId { get; private set; }
    public Guid CommunityOwnerId { get; private set; }
    public string TrackingId { get; private set; } = string.Empty;
    public PlacementStatus Status { get; private set; } = PlacementStatus.ReadyToPost;
    public DateTimeOffset? ActivatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public void Activate(DateTimeOffset now)
    {
        if (Status != PlacementStatus.ReadyToPost) throw new InvalidOperationException("Only Ready to Post placements can be activated.");
        Status = PlacementStatus.Active;
        ActivatedAt = now;
    }

    public void Complete(DateTimeOffset now)
    {
        if (Status != PlacementStatus.Active) throw new InvalidOperationException("Only Active placements can be completed.");
        Status = PlacementStatus.Completed;
        CompletedAt = now;
    }
}
