using Relay.Domain.Enums;

namespace Relay.Domain.Entities;

public sealed class ClickEvent : AuditableEntity
{
    private ClickEvent() { }

    public ClickEvent(string clickId, string trackingId, Guid campaignId, Guid placementId, Guid communityId, Guid communityOwnerId, string idempotencyKey)
    {
        ClickId = clickId.Trim();
        TrackingId = trackingId.Trim();
        CampaignId = campaignId;
        PlacementId = placementId;
        CommunityId = communityId;
        CommunityOwnerId = communityOwnerId;
        IdempotencyKey = idempotencyKey.Trim();
    }

    public string ClickId { get; private set; } = string.Empty;
    public string TrackingId { get; private set; } = string.Empty;
    public Guid CampaignId { get; private set; }
    public Guid PlacementId { get; private set; }
    public Guid CommunityId { get; private set; }
    public Guid CommunityOwnerId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public ClickQualification Qualification { get; private set; } = ClickQualification.Rejected;
    public string? RejectionReason { get; private set; }
    public string? VisitorKeyHash { get; private set; }

    public void Qualify(string? visitorKeyHash)
    {
        Qualification = ClickQualification.Qualified;
        VisitorKeyHash = visitorKeyHash;
        RejectionReason = null;
    }

    public void Reject(string reason)
    {
        Qualification = ClickQualification.Rejected;
        RejectionReason = reason.Trim();
    }
}

public sealed class LedgerEntry : AuditableEntity
{
    private LedgerEntry() { }

    public LedgerEntry(Guid clickEventId, Guid campaignId, Guid placementId, decimal advertiserCharge, decimal ownerEarning, decimal platformFee)
    {
        ClickEventId = clickEventId;
        CampaignId = campaignId;
        PlacementId = placementId;
        AdvertiserCharge = advertiserCharge;
        CommunityOwnerEarning = ownerEarning;
        PlatformFee = platformFee;
    }

    public Guid ClickEventId { get; private set; }
    public Guid CampaignId { get; private set; }
    public Guid PlacementId { get; private set; }
    public LedgerEntryType Type { get; private set; } = LedgerEntryType.QualifiedClick;
    public decimal AdvertiserCharge { get; private set; }
    public decimal CommunityOwnerEarning { get; private set; }
    public decimal PlatformFee { get; private set; }
}
