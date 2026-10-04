using Microsoft.EntityFrameworkCore;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Domain.Enums;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class AdminOverviewService(RelayDbContext db) : IAdminOverviewService
{
    public async Task<AdminOverviewResponse> GetAsync(CancellationToken cancellationToken)
    {
        var advertisers = await db.Users.CountAsync(x => x.Role == UserRole.Advertiser, cancellationToken);
        var communityOwners = await db.Users.CountAsync(x => x.Role == UserRole.CommunityOwner, cancellationToken);
        var activeCampaigns = await db.Campaigns.CountAsync(x => x.Status == CampaignStatus.Active, cancellationToken);
        var pendingApplications = await db.Applications.CountAsync(x => x.Status == ApplicationStatus.Pending, cancellationToken);
        var activePlacements = await db.Placements.CountAsync(x => x.Status == PlacementStatus.Active, cancellationToken);
        var activeCommunityIds = await db.Placements.Where(x => x.Status == PlacementStatus.Active).Select(x => x.CommunityId).Distinct().CountAsync(cancellationToken);
        var qualifiedClicks = await db.ClickEvents.CountAsync(x => x.Qualification == ClickQualification.Qualified, cancellationToken);
        var financials = await db.LedgerEntries.Where(x => x.Type == LedgerEntryType.QualifiedClick).GroupBy(_ => 1).Select(group => new { Spend = group.Sum(x => x.AdvertiserCharge), Earnings = group.Sum(x => x.CommunityOwnerEarning), Revenue = group.Sum(x => x.PlatformFee) }).SingleOrDefaultAsync(cancellationToken);
        return new AdminOverviewResponse(
            advertisers + communityOwners,
            advertisers,
            communityOwners,
            await db.Campaigns.CountAsync(cancellationToken),
            activeCampaigns,
            await db.Communities.CountAsync(cancellationToken),
            activeCommunityIds,
            await db.Applications.CountAsync(cancellationToken),
            pendingApplications,
            await db.Placements.CountAsync(cancellationToken),
            activePlacements,
            qualifiedClicks,
            financials?.Spend ?? 0,
            financials?.Earnings ?? 0,
            financials?.Revenue ?? 0);
    }
}
