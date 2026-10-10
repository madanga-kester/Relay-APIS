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
        if (!currentUser.IsInRole(UserRole.CommunityOwner) || currentUser.UserId is null)
        {
            throw new ForbiddenOperationException("Community Owner access is required.");
        }

        var campaign = await db.Campaigns
            .SingleOrDefaultAsync(x => x.Id == request.CampaignId, cancellationToken)
            ?? throw new NotFoundException("Campaign");

        if (campaign.Status is not (CampaignStatus.Published or CampaignStatus.Active))
        {
            throw new ConflictException("This campaign is not accepting applications.");
        }

        var community = await db.Communities
            .SingleOrDefaultAsync(x => x.Id == request.CommunityId && x.OwnerId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("Community");

        if (community.VerificationStatus == VerificationStatus.Suspended)
        {
            throw new ForbiddenOperationException("Suspended communities cannot apply.");
        }

        var alreadyApplied = await db.Applications
            .AnyAsync(x => x.CampaignId == campaign.Id && x.CommunityId == community.Id, cancellationToken);

        if (alreadyApplied)
        {
            throw new ConflictException("This community has already applied to the campaign.");
        }

        var application = new CampaignApplication(campaign.Id, community.Id, currentUser.UserId.Value, campaign.Cpc);
        db.Applications.Add(application);

        db.ActivityEvents.Add(new ActivityEvent(
            "Application submitted",
            "CampaignApplication",
            application.Id,
            campaign.Name,
            currentUser.UserId,
            $"{community.Name} applied to the campaign."));
        db.UserNotifications.Add(new UserNotification(
            campaign.AdvertiserId,
            "application",
            "New application",
                        $"{community.Name} applied to \"{campaign.Name}\".",
                       "/campaign-owner/applications",
            community.Platform.ToString()));

        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(application, community, campaign);
    }

    public async Task<PageResult<ApplicationResponse>> MineAsync(PageRequest page, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new ForbiddenOperationException("Authentication is required.");
        }

        // Admins see everything, community owners see their own applications,
        // advertisers see the applications made to their campaigns.
        var query = currentUser.Role == UserRole.Admin
            ? db.Applications.AsNoTracking()
            : currentUser.Role == UserRole.CommunityOwner
                ? db.Applications.AsNoTracking().Where(x => x.CommunityOwnerId == currentUser.UserId)
                : db.Applications.AsNoTracking().Where(x =>
                    db.Campaigns.Any(c => c.Id == x.CampaignId && c.AdvertiserId == currentUser.UserId));

        query = query.Include(x => x.Placement);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page.SafePage - 1) * page.SafePageSize)
            .Take(page.SafePageSize)
            .ToListAsync(cancellationToken);

        // Attach the applicant community's details so the reviewer can see who is applying,
        // including communities that are still pending verification.
        var communityIds = items.Select(x => x.CommunityId).Distinct().ToList();
        var communityLookup = await db.Communities
            .AsNoTracking()
            .Where(x => communityIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        // Attach the campaign summary too, so a community owner can still see the details of a
        // campaign they were accepted into after it is paused or completed.
        var campaignIds = items.Select(x => x.CampaignId).Distinct().ToList();
        var campaignLookup = await db.Campaigns
            .AsNoTracking()
            .Where(x => campaignIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var responses = items.Select(application =>
        {
            communityLookup.TryGetValue(application.CommunityId, out var community);
            campaignLookup.TryGetValue(application.CampaignId, out var campaign);
            return ToResponse(application, community, campaign);
        }).ToList();

        return new PageResult<ApplicationResponse>(responses, page.SafePage, page.SafePageSize, total);
    }

    public async Task<ApplicationResponse?> ReviewAsync(Guid id, ReviewApplicationRequest request, CancellationToken cancellationToken)
    {
        // The database retries transient failures, so a user-started transaction must run inside
        // an execution strategy that can repeat the whole unit of work.
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<ApplicationResponse?>(async () =>
        {
            db.ChangeTracker.Clear();

            var application = await db.Applications
                .Include(x => x.Placement)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (application is null)
            {
                return null;
            }

            var campaign = await db.Campaigns.SingleAsync(x => x.Id == application.CampaignId, cancellationToken);

            if (currentUser.Role != UserRole.Admin && currentUser.UserId != campaign.AdvertiserId)
            {
                throw new ForbiddenOperationException("Only the campaign owner or an admin can review applications.");
            }

            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            if (request.Accept)
            {
                var accepted = await db.Applications.CountAsync(
                    x => x.CampaignId == campaign.Id && x.Status == ApplicationStatus.Accepted,
                    cancellationToken);

                if (accepted >= campaign.MaximumCommunities)
                {
                    throw new ConflictException("Community limit reached.");
                }

                application.Accept(clock.UtcNow);

                // The placement is created with its Id already set, so EF would treat it as an
                // existing row and try to UPDATE it. Adding it explicitly makes EF INSERT it.
                if (application.Placement is not null)
                {
                    db.Placements.Add(application.Placement);
                }
            }
            else
            {
                application.Reject(clock.UtcNow, request.Reason);
            }

            db.ActivityEvents.Add(new ActivityEvent(
                request.Accept ? "Application approved" : "Application rejected",
                "CampaignApplication",
                application.Id,
                campaign.Name,
                currentUser.UserId,
                request.Reason ?? "Application reviewed."));

            var reviewedCommunity = await db.Communities
            .Where(x => x.Id == application.CommunityId)
            .Select(x => new { x.Name, x.Platform })
            .SingleOrDefaultAsync(cancellationToken);
            var reviewedCommunityName = reviewedCommunity?.Name ?? "Your community";
            var reviewedCommunityPlatform = reviewedCommunity?.Platform.ToString();

            db.UserNotifications.Add(request.Accept
                ? new UserNotification(
                    application.CommunityOwnerId,
                    "application",
                    "Application accepted",
                    $"{reviewedCommunityName} was accepted into \"{campaign.Name}\". Post the ad to start earning.",
                                        "/community-owner/accepted-campaigns",
                    reviewedCommunityPlatform)
                : new UserNotification(
                    application.CommunityOwnerId,
                    "application",
                    "Application declined",
                    string.IsNullOrWhiteSpace(request.Reason)
                        ? $"{reviewedCommunityName} was not selected for \"{campaign.Name}\"."
                        : $"{reviewedCommunityName} was not selected for \"{campaign.Name}\": {request.Reason}",
                                       "/community-owner/campaigns",
                    reviewedCommunityPlatform));

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ToResponse(application);
        });
    }

    private static ApplicationResponse ToResponse(CampaignApplication application, Community? community = null, Campaign? campaign = null) => new(
        application.Id,
        application.CampaignId,
        application.CommunityId,
        application.CommunityOwnerId,
        application.Cpc,
        application.Status,
        application.Placement is null
            ? null
            : new PlacementResponse(
                application.Placement.Id,
                application.Placement.CampaignId,
                application.Placement.CommunityId,
                application.Placement.CommunityOwnerId,
                application.Placement.TrackingId,
                application.Placement.Status),
        community is null
            ? null
            : new ApplicationCommunityResponse(
                community.Name,
                community.Platform,
                community.Members,
                community.Category,
                community.Location,
                community.VerificationStatus,
                community.AudienceDescription,
                community.CommunityLink),
        campaign is null
            ? null
            : new ApplicationCampaignResponse(
                campaign.Name,
                campaign.AdvertiserName,
                campaign.Advertisement,
                campaign.DestinationUrl,
                campaign.DurationDays,
                campaign.StartDate,
                campaign.EndDate,
                campaign.Cpc,
                campaign.Budget,
                campaign.Status));
}