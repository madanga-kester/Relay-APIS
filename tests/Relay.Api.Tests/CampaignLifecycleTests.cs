using Relay.Domain.Entities;
using Relay.Domain.Enums;

namespace Relay.Api.Tests;

public sealed class CampaignLifecycleTests
{
    [Fact]
    public void DraftCanPublishWithoutActivating()
    {
        var campaign = NewCampaign();
        campaign.Publish();
        Assert.Equal(CampaignStatus.Published, campaign.Status);
    }

    [Fact]
    public void ActiveCanPauseResumeAndComplete()
    {
        var campaign = NewCampaign();
        campaign.Publish();
        campaign.ActivateFromPlacement();
        campaign.Pause();
        campaign.Resume();
        campaign.Complete();
        Assert.Equal(CampaignStatus.Completed, campaign.Status);
    }

    [Fact]
    public void CompletedCannotResumeOrPause()
    {
        var campaign = NewCampaign();
        campaign.Publish();
        campaign.ActivateFromPlacement();
        campaign.Complete();
        Assert.Throws<InvalidOperationException>(() => campaign.Pause());
        Assert.Throws<InvalidOperationException>(() => campaign.Resume());
    }

    private static Campaign NewCampaign() => new(Guid.NewGuid(), "Test campaign", "Description", "Advertisement", "https://example.com", 2m, 100m, 2, "Test", "Kenya");
}
