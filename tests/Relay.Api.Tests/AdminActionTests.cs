using Relay.Domain.Entities;
using Relay.Domain.Enums;

namespace Relay.Api.Tests;

public sealed class AdminActionTests
{
    [Fact]
    public void UserStatusCanSuspendAndReactivate()
    {
        var user = new UserAccount("admin-test@example.com", "Admin Test", UserRole.CommunityOwner);
        user.Suspend();
        Assert.Equal(AccountStatus.Suspended, user.Status);
        user.Activate();
        Assert.Equal(AccountStatus.Active, user.Status);
    }

    [Fact]
    public void CommunityModerationCanVerifySuspendAndRestore()
    {
        var community = new Community(Guid.NewGuid(), "Test Community", CommunityPlatform.WhatsApp, 100, "Fashion", "Nairobi", null, "Audience", null);
        community.Verify();
        Assert.Equal(VerificationStatus.Verified, community.VerificationStatus);
        community.Suspend();
        Assert.Equal(VerificationStatus.Suspended, community.VerificationStatus);
        community.Restore();
        Assert.Equal(VerificationStatus.Verified, community.VerificationStatus);
    }

    [Fact]
    public void PlacementCanActivateAndComplete()
    {
        var placement = new Placement(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        placement.Activate(DateTimeOffset.UtcNow);
        Assert.Equal(PlacementStatus.Active, placement.Status);
        placement.Complete(DateTimeOffset.UtcNow);
        Assert.Equal(PlacementStatus.Completed, placement.Status);
    }

    [Fact]
    public void CampaignCanBePausedAndCompleted()
    {
        var campaign = new Campaign(Guid.NewGuid(), "Admin campaign", "Description", "Ad", "https://example.com", 2m, 100m, 2, "Fashion", "Nairobi");
        campaign.Publish();
        campaign.ActivateFromPlacement();
        campaign.Pause();
        Assert.Equal(CampaignStatus.Paused, campaign.Status);
        campaign.Complete();
        Assert.Equal(CampaignStatus.Completed, campaign.Status);
    }

    [Fact]
    public void AdminNoteAndPayoutRecordsPreserveTheirIdentifiers()
    {
        var adminId = Guid.NewGuid();
        var note = new AdminNote("Campaign", Guid.NewGuid(), "Campaign", "Internal review", adminId);
        var payout = new PayoutRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m);
        var noteId = note.Id;
        var payoutId = payout.Id;
        payout.ChangeStatus(PayoutStatus.Completed, null);
        Assert.Equal(noteId, note.Id);
        Assert.Equal(payoutId, payout.Id);
        Assert.Equal(PayoutStatus.Completed, payout.Status);
    }
}
