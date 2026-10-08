using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Domain.Entities;
using Relay.Domain.Enums;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class ActivityFeedService(RelayDbContext db, ICurrentUser currentUser) : IActivityFeedService
{
    private const string ClickEventType = "Qualified click recorded";

    public async Task<IReadOnlyList<ActivityFeedItemResponse>> GetAsync(int limit, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId) throw new ForbiddenOperationException("Authentication is required.");
        var take = Math.Clamp(limit, 1, 100);
        List<ActivityEvent> events;
        if (currentUser.Role == UserRole.Advertiser)
        {
            var campaignIds = db.Campaigns.AsNoTracking().Where(x => x.AdvertiserId == userId).Select(x => x.Id);
            events = await CollectAsync(
                campaignIds,
                db.Applications.AsNoTracking().Where(x => campaignIds.Contains(x.CampaignId)).Select(x => x.Id),
                db.Placements.AsNoTracking().Where(x => campaignIds.Contains(x.CampaignId)).Select(x => x.Id),
                "Campaign",
                take,
                cancellationToken);
        }
        else if (currentUser.Role == UserRole.CommunityOwner)
        {
            events = await CollectAsync(
                db.Communities.AsNoTracking().Where(x => x.OwnerId == userId).Select(x => x.Id),
                db.Applications.AsNoTracking().Where(x => x.CommunityOwnerId == userId).Select(x => x.Id),
                db.Placements.AsNoTracking().Where(x => x.CommunityOwnerId == userId).Select(x => x.Id),
                "Community",
                take,
                cancellationToken);
        }
        else throw new ForbiddenOperationException("Activity is available to Campaign Owners and Community Owners.");

        var placementEventIds = events.Where(x => x.EntityType == "Placement").Select(x => x.EntityId).Distinct().ToList();
        var campaignNames = new Dictionary<Guid, string>();
        if (placementEventIds.Count > 0)
        {
            var rows = await (from placement in db.Placements.AsNoTracking()
                              join campaign in db.Campaigns.AsNoTracking() on placement.CampaignId equals campaign.Id
                              where placementEventIds.Contains(placement.Id)
                              select new { placement.Id, campaign.Name }).ToListAsync(cancellationToken);
            campaignNames = rows.ToDictionary(x => x.Id, x => x.Name);
        }

        return events.Select(x => new ActivityFeedItemResponse(
            x.Id,
            x.EventType,
            x.EntityType,
            x.EntityId,
            x.EntityType == "Placement" && campaignNames.TryGetValue(x.EntityId, out var campaignName) ? campaignName : x.EntityName,
            x.Detail,
            x.CreatedAt)).ToList();
    }

    private async Task<List<ActivityEvent>> CollectAsync(IQueryable<Guid> ownEntityIds, IQueryable<Guid> applicationIds, IQueryable<Guid> placementIds, string ownEntityType, int take, CancellationToken cancellationToken)
    {
        var own = await db.ActivityEvents.AsNoTracking()
            .Where(x => x.EntityType == ownEntityType && ownEntityIds.Contains(x.EntityId))
            .OrderByDescending(x => x.CreatedAt).Take(take).ToListAsync(cancellationToken);
        var applications = await db.ActivityEvents.AsNoTracking()
            .Where(x => x.EntityType == "CampaignApplication" && applicationIds.Contains(x.EntityId))
            .OrderByDescending(x => x.CreatedAt).Take(take).ToListAsync(cancellationToken);
        var placements = await db.ActivityEvents.AsNoTracking()
            .Where(x => x.EntityType == "Placement" && x.EventType != ClickEventType && placementIds.Contains(x.EntityId))
            .OrderByDescending(x => x.CreatedAt).Take(take).ToListAsync(cancellationToken);
        return own.Concat(applications).Concat(placements).OrderByDescending(x => x.CreatedAt).Take(take).ToList();
    }
}