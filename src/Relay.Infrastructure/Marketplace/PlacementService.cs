using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Domain.Entities;
using Relay.Domain.Enums;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class PlacementService(RelayDbContext db, ICurrentUser currentUser, IClock clock) : IPlacementService
{
    public async Task<PageResult<PlacementResponse>> MineAsync(PageRequest page, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId) throw new ForbiddenOperationException("Authentication is required.");
        var query = db.Placements.AsNoTracking();
        if (currentUser.Role != UserRole.Admin)
        {
            query = currentUser.Role == UserRole.CommunityOwner
                ? query.Where(x => x.CommunityOwnerId == userId)
                : query.Where(x => db.Campaigns.Any(c => c.Id == x.CampaignId && c.AdvertiserId == userId));
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page.SafePage - 1) * page.SafePageSize).Take(page.SafePageSize).ToListAsync(cancellationToken);
        return new PageResult<PlacementResponse>(items.Select(ToResponse).ToList(), page.SafePage, page.SafePageSize, total);
    }

    public Task<PlacementResponse?> ActivateAsync(Guid id, CancellationToken cancellationToken) => TransitionAsync(id, activate: true, cancellationToken);
    public Task<PlacementResponse?> CompleteAsync(Guid id, CancellationToken cancellationToken) => TransitionAsync(id, activate: false, cancellationToken);




    private async Task<PlacementResponse?> TransitionAsync(Guid id, bool activate, CancellationToken cancellationToken)
    {
        var placement = await db.Placements.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (placement is null) return null;
        if (currentUser.Role != UserRole.Admin && currentUser.UserId != placement.CommunityOwnerId) throw new ForbiddenOperationException("Only the Community Owner or an admin can update this placement.");
        var campaign = await db.Campaigns.SingleAsync(x => x.Id == placement.CampaignId, cancellationToken);
        if (activate)
        {
            placement.Activate(clock.UtcNow);
            campaign.ActivateFromPlacement();
        }
        else placement.Complete(clock.UtcNow);
        db.ActivityEvents.Add(new ActivityEvent(activate ? "Placement activated" : "Placement completed", "Placement", placement.Id, placement.TrackingId, currentUser.UserId, $"Placement status changed to {placement.Status}."));
        if (activate && campaign.Status == CampaignStatus.Active) db.ActivityEvents.Add(new ActivityEvent("Campaign Activated", "Campaign", campaign.Id, campaign.Name, currentUser.UserId, "First active placement confirmed."));
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(placement);
    }

    private static PlacementResponse ToResponse(Placement placement) => new(placement.Id, placement.CampaignId, placement.CommunityId, placement.CommunityOwnerId, placement.TrackingId, placement.Status);
}
