using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Domain.Enums;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class AdminReportingService(RelayDbContext db, ICurrentUser currentUser, IClock clock) : IAdminReportingService
{
    private void EnsureAdmin() { if (currentUser.Role != UserRole.Admin) throw new ForbiddenOperationException("Admin access is required."); }

    public async Task<AdminReportResponse> GetReportAsync(DateTimeOffset? from, DateTimeOffset? toDate, CancellationToken cancellationToken)
    {
        EnsureAdmin();
        var clicks = db.ClickEvents.AsNoTracking().Where(x => x.Qualification == ClickQualification.Qualified);
        if (from is { } start) clicks = clicks.Where(x => x.CreatedAt >= start);
        if (toDate is { } end) clicks = clicks.Where(x => x.CreatedAt <= end);
        var clickIds = await clicks.Select(x => x.Id).ToListAsync(cancellationToken);
        var ledger = db.LedgerEntries.AsNoTracking().Where(x => clickIds.Contains(x.ClickEventId));
        var spend = await ledger.SumAsync(x => (decimal?)x.AdvertiserCharge, cancellationToken) ?? 0m;
        var earnings = await ledger.SumAsync(x => (decimal?)x.CommunityOwnerEarning, cancellationToken) ?? 0m;
        var revenue = await ledger.SumAsync(x => (decimal?)x.PlatformFee, cancellationToken) ?? 0m;
        var campaigns = db.Campaigns.AsNoTracking();
        var communities = db.Communities.AsNoTracking();
        var applications = db.Applications.AsNoTracking();
        var placements = db.Placements.AsNoTracking();
        if (from is { } campaignStart) campaigns = campaigns.Where(x => x.CreatedAt >= campaignStart);
        if (toDate is { } campaignEnd) campaigns = campaigns.Where(x => x.CreatedAt <= campaignEnd);
        var campaignCount = await campaigns.CountAsync(cancellationToken);
        var activeCampaigns = await campaigns.CountAsync(x => x.Status == CampaignStatus.Active, cancellationToken);
        var communityCount = await communities.CountAsync(cancellationToken);
        var activeCommunities = await placements.Where(x => x.Status == PlacementStatus.Active).Select(x => x.CommunityId).Distinct().CountAsync(cancellationToken);
        var applicationCount = await applications.CountAsync(cancellationToken);
        var placementCount = await placements.CountAsync(cancellationToken);
        var delta = spend - earnings - revenue;
        return new(from, toDate, campaignCount, activeCampaigns, communityCount, activeCommunities, applicationCount, placementCount, clickIds.Count, spend, earnings, revenue, delta);
    }

    public async Task<AdminHealthResponse> GetHealthAsync(CancellationToken cancellationToken)
    {
        EnsureAdmin();
        var since = clock.UtcNow.AddHours(-24);
        var activeCampaigns = await db.Campaigns.CountAsync(x => x.Status == CampaignStatus.Active, cancellationToken);
        var activePlacements = await db.Placements.CountAsync(x => x.Status == PlacementStatus.Active, cancellationToken);
        var qualified = await db.ClickEvents.CountAsync(x => x.CreatedAt >= since && x.Qualification == ClickQualification.Qualified, cancellationToken);
        var failed = await db.ClickEvents.CountAsync(x => x.CreatedAt >= since && x.Qualification == ClickQualification.Rejected, cancellationToken);
        var activities = await db.ActivityEvents.CountAsync(x => x.CreatedAt >= since, cancellationToken);
        var spend = await db.LedgerEntries.SumAsync(x => (decimal?)x.AdvertiserCharge, cancellationToken) ?? 0m;
        var allocated = (await db.LedgerEntries.SumAsync(x => (decimal?)x.CommunityOwnerEarning, cancellationToken) ?? 0m) + (await db.LedgerEntries.SumAsync(x => (decimal?)x.PlatformFee, cancellationToken) ?? 0m);
        return new(activeCampaigns, activePlacements, qualified, failed, activities, Math.Abs(spend - allocated) < 0.01m);
    }
}
