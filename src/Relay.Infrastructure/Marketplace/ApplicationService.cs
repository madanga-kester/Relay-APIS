using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Domain.Entities;
using Relay.Domain.Enums;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class ApplicationService(RelayDbContext db, ICurrentUser currentUser, IClock clock) : IApplicationService
{
    public async Task<ApplicationResponse> ApplyAsync(ApplyToCampaignRequest request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(UserRole.CommunityOwner) || currentUser.UserId is null) throw new ForbiddenOperationException("Community Owner access is required.");
        var campaign = await db.Campaigns.SingleOrDefaultAsync(x => x.Id == request.CampaignId, cancellationToken) ?? throw new NotFoundException("Campaign");
        if (campaign.Status is not (CampaignStatus.Published or CampaignStatus.Active)) throw new ConflictException("This campaign is not accepting applications.");
        var community = await db.Communities.SingleOrDefaultAsync(x => x.Id == request.CommunityId && x.OwnerId == currentUser.UserId, cancellationToken) ?? throw new NotFoundException("Community");
        if (community.VerificationStatus == VerificationStatus.Suspended) throw new ForbiddenOperationException("Suspended communities cannot apply.");
        if (await db.Applications.AnyAsync(x => x.CampaignId == campaign.Id && x.CommunityId == community.Id, cancellationToken)) throw new ConflictException("This community has already applied to the campaign.");
        var application = new CampaignApplication(campaign.Id, community.Id, currentUser.UserId.Value, campaign.Cpc);
        db.Applications.Add(application);
        db.ActivityEvents.Add(new ActivityEvent("Application submitted", "CampaignApplication", application.Id, campaign.Name, currentUser.UserId, $"{community.Name} applied to the campaign."));
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(application);
    }

    public async Task<PageResult<ApplicationResponse>> MineAsync(PageRequest page, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) throw new ForbiddenOperationException("Authentication is required.");
        var query = currentUser.Role == UserRole.Admin ? db.Applications.AsNoTracking() : currentUser.Role == UserRole.CommunityOwner ? db.Applications.AsNoTracking().Where(x => x.CommunityOwnerId == currentUser.UserId) : db.Applications.AsNoTracking().Where(x => db.Campaigns.Any(c => c.Id == x.CampaignId && c.AdvertiserId == currentUser.UserId));
        query = query.Include(x => x.Placement);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page.SafePage - 1) * page.SafePageSize).Take(page.SafePageSize).ToListAsync(cancellationToken);
        return new PageResult<ApplicationResponse>(items.Select(ToResponse).ToList(), page.SafePage, page.SafePageSize, total);
    }

    public async Task<ApplicationResponse?> ReviewAsync(Guid id, ReviewApplicationRequest request, CancellationToken cancellationToken)
    {
        var application = await db.Applications.Include(x => x.Placement).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (application is null) return null;
        var campaign = await db.Campaigns.SingleAsync(x => x.Id == application.CampaignId, cancellationToken);
        if (currentUser.Role != UserRole.Admin && currentUser.UserId != campaign.AdvertiserId) throw new ForbiddenOperationException("Only the campaign owner or an admin can review applications.");
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        if (request.Accept)
        {
            var accepted = await db.Applications.CountAsync(x => x.CampaignId == campaign.Id && x.Status == ApplicationStatus.Accepted, cancellationToken);
            if (accepted >= campaign.MaximumCommunities) throw new ConflictException("Community limit reached.");
            application.Accept(clock.UtcNow);
        }
        else application.Reject(clock.UtcNow, request.Reason);
        db.ActivityEvents.Add(new ActivityEvent(request.Accept ? "Application approved" : "Application rejected", "CampaignApplication", application.Id, campaign.Name, currentUser.UserId, request.Reason ?? "Application reviewed."));
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToResponse(application);
    }

    private static ApplicationResponse ToResponse(CampaignApplication application) => new(application.Id, application.CampaignId, application.CommunityId, application.CommunityOwnerId, application.Cpc, application.Status, application.Placement is null ? null : new PlacementResponse(application.Placement.Id, application.Placement.CampaignId, application.Placement.CommunityId, application.Placement.CommunityOwnerId, application.Placement.TrackingId, application.Placement.Status));
}
