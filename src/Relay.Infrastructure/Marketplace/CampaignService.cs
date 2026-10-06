using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Application.Validation;
using Relay.Domain.Entities;
using Relay.Domain.Enums;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class CampaignService(RelayDbContext db, ICurrentUser currentUser, IClock clock) : ICampaignService
{
    public async Task<PageResult<CampaignResponse>> ListAsync(PageRequest page, CancellationToken cancellationToken)
    {
        var query = db.Campaigns.AsNoTracking().AsQueryable();
        if (currentUser.Role != UserRole.Admin)
        {
            query = currentUser.UserId is Guid advertiserId
                ? query.Where(x => x.AdvertiserId == advertiserId || x.Status == CampaignStatus.Published || x.Status == CampaignStatus.Active)
                : query.Where(x => x.Status == CampaignStatus.Published || x.Status == CampaignStatus.Active);
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page.SafePage - 1) * page.SafePageSize).Take(page.SafePageSize).ToListAsync(cancellationToken);
        return new PageResult<CampaignResponse>(items.Select(ToResponse).ToList(), page.SafePage, page.SafePageSize, total);
    }


    public async Task<PageResult<CampaignResponse>> MineAsync(PageRequest page, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid advertiserId) throw new ForbiddenOperationException("Authentication is required.");
        var query = db.Campaigns.AsNoTracking().Where(x => x.AdvertiserId == advertiserId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page.SafePage - 1) * page.SafePageSize).Take(page.SafePageSize).ToListAsync(cancellationToken);
        return new PageResult<CampaignResponse>(items.Select(ToResponse).ToList(), page.SafePage, page.SafePageSize, total);
    }

    public async Task<IReadOnlyList<CampaignPerformanceResponse>> PerformanceAsync(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid advertiserId) throw new ForbiddenOperationException("Authentication is required.");
        var campaignIds = db.Campaigns.AsNoTracking().Where(x => x.AdvertiserId == advertiserId).Select(x => x.Id);
        var clicks = await db.ClickEvents.AsNoTracking()
            .Where(x => campaignIds.Contains(x.CampaignId))
            .GroupBy(x => x.CampaignId)
            .Select(group => new
            {
                CampaignId = group.Key,
                Qualified = group.Count(x => x.Qualification == ClickQualification.Qualified),
                Rejected = group.Count(x => x.Qualification == ClickQualification.Rejected)
            })
            .ToListAsync(cancellationToken);
        var money = await db.LedgerEntries.AsNoTracking()
            .Where(x => campaignIds.Contains(x.CampaignId) && x.Type == LedgerEntryType.QualifiedClick)
            .GroupBy(x => x.CampaignId)
            .Select(group => new
            {
                CampaignId = group.Key,
                Spend = group.Sum(x => x.AdvertiserCharge),
                Earnings = group.Sum(x => x.CommunityOwnerEarning),
                Fees = group.Sum(x => x.PlatformFee)
            })
            .ToListAsync(cancellationToken);
        var moneyByCampaign = money.ToDictionary(x => x.CampaignId);
        return clicks.Select(item =>
        {
            moneyByCampaign.TryGetValue(item.CampaignId, out var amounts);
            return new CampaignPerformanceResponse(item.CampaignId, item.Qualified, item.Rejected, amounts?.Spend ?? 0m, amounts?.Earnings ?? 0m, amounts?.Fees ?? 0m);
        }).ToList();
    }
    public async Task<CampaignResponse> CreateAsync(CreateCampaignRequest request, CancellationToken cancellationToken)
    {
        EnsureRole(UserRole.Advertiser);
        var errors = RequestValidators.Validate(request);
        if (errors.Count > 0) throw new ValidationFailureException(errors);
        var campaign = new Campaign(currentUser.UserId!.Value, request.Name, request.AdvertiserName, request.Description, request.Advertisement, request.DestinationUrl, request.Platforms, request.MinimumAudience, request.MaximumAudience, request.Category, request.Location, request.DurationDays, request.MaximumCommunities, request.Cpc, request.Budget, request.StartDate, request.EndDate);
        db.Campaigns.Add(campaign);
        db.ActivityEvents.Add(new ActivityEvent("Campaign created", "Campaign", campaign.Id, campaign.Name, currentUser.UserId, "Campaign created as Draft."));
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(campaign);
    }

    public async Task<CampaignResponse?> UpdateAsync(Guid id, UpdateCampaignRequest request, CancellationToken cancellationToken)
    {
        var campaign = await db.Campaigns.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (campaign is null) return null;
        EnsureOwnerOrAdmin(campaign.AdvertiserId);
        var createShape = new CreateCampaignRequest(request.Name, request.AdvertiserName, request.Description, request.Advertisement, request.DestinationUrl, request.Platforms, request.MinimumAudience, request.MaximumAudience, request.Category, request.Location, request.DurationDays, request.MaximumCommunities, request.Cpc, request.Budget, request.StartDate, request.EndDate);
        var errors = RequestValidators.Validate(createShape);
        if (errors.Count > 0) throw new ValidationFailureException(errors);
        campaign.UpdateDraft(request.Name, request.AdvertiserName, request.Description, request.Advertisement, request.DestinationUrl, request.Platforms, request.MinimumAudience, request.MaximumAudience, request.Category, request.Location, request.DurationDays, request.MaximumCommunities, request.Cpc, request.Budget, request.StartDate, request.EndDate);
        db.ActivityEvents.Add(new ActivityEvent("Campaign updated", "Campaign", campaign.Id, campaign.Name, currentUser.UserId, "Draft campaign details updated."));
        campaign.Touch(clock.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(campaign);
    }

    public async Task<CampaignResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var campaign = await db.Campaigns.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (campaign is null) return null;
        EnsureCanView(campaign);
        return ToResponse(campaign);
    }

    public async Task<CampaignResponse?> TransitionAsync(Guid id, string action, CancellationToken cancellationToken)
    {
        var campaign = await db.Campaigns.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (campaign is null) return null;
        EnsureOwnerOrAdmin(campaign.AdvertiserId);
        var normalizedAction = action.Trim().ToLowerInvariant();
        var eventType = normalizedAction switch
        {
            "publish" => "Campaign Published",
            "pause" => "Campaign Paused",
            "resume" => "Campaign Resumed",
            "complete" => "Campaign Completed",
            _ => throw new ValidationFailureException(["Supported actions are publish, pause, resume, and complete."])
        };
        switch (normalizedAction)
        {
            case "publish": campaign.Publish(); break;
            case "pause": campaign.Pause(); break;
            case "resume": campaign.Resume(); break;
            case "complete": campaign.Complete(); break;
        }
        db.ActivityEvents.Add(new ActivityEvent(eventType, "Campaign", campaign.Id, campaign.Name, currentUser.UserId, $"Campaign status changed to {campaign.Status}."));
        campaign.Touch(clock.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(campaign);
    }

    private void EnsureRole(UserRole required) { if (!currentUser.IsInRole(required) || currentUser.UserId is null) throw new ForbiddenOperationException($"{required} access is required."); }
    private void EnsureOwnerOrAdmin(Guid ownerId) { if (currentUser.Role != UserRole.Admin && currentUser.UserId != ownerId) throw new ForbiddenOperationException("You do not have access to this campaign."); }
    private void EnsureCanView(Campaign campaign) { if (currentUser.Role != UserRole.Admin && campaign.Status is not (CampaignStatus.Published or CampaignStatus.Active) && currentUser.UserId != campaign.AdvertiserId) throw new ForbiddenOperationException("You do not have access to this campaign."); }
    private static CampaignResponse ToResponse(Campaign campaign) => new(campaign.Id, campaign.AdvertiserId, campaign.Name, campaign.AdvertiserName, campaign.Description, campaign.Advertisement, campaign.DestinationUrl, campaign.Platforms, campaign.MinimumAudience, campaign.MaximumAudience, campaign.Category, campaign.Location, campaign.DurationDays, campaign.MaximumCommunities, campaign.Cpc, campaign.Budget, campaign.StartDate, campaign.EndDate, campaign.Status);
}
