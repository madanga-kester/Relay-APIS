using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Financial;
using Relay.Application.Services;
using Relay.Domain.Entities;
using Relay.Domain.Enums;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class TrackingService(RelayDbContext db, IFinancialCalculator finances, IClock clock) : ITrackingService
{
    public async Task<TrackingClickResponse?> ProcessClickAsync(string trackingId, string? idempotencyKey, string? visitorKey, CancellationToken cancellationToken)
    {
        var placement = await db.Placements.SingleOrDefaultAsync(x => x.TrackingId == trackingId, cancellationToken);
        if (placement is null) return null;
        var key = string.IsNullOrWhiteSpace(idempotencyKey) ? $"{trackingId}:{visitorKey}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}" : idempotencyKey.Trim();
        var existing = await db.ClickEvents.SingleOrDefaultAsync(x => x.IdempotencyKey == key, cancellationToken);
        if (existing is not null) return new TrackingClickResponse(existing.ClickId, existing.Qualification, existing.RejectionReason, string.Empty);
        var campaign = await db.Campaigns.SingleAsync(x => x.Id == placement.CampaignId, cancellationToken);
        var click = new ClickEvent(Guid.NewGuid().ToString("N"), trackingId, placement.CampaignId, placement.Id, placement.CommunityId, placement.CommunityOwnerId, key);
        var rejection = await GetRejectionReasonAsync(placement, campaign, visitorKey, cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var destination = campaign.DestinationUrl;
        if (rejection is not null) click.Reject(rejection);
        else
        {
            var qualified = await db.ClickEvents.CountAsync(x => x.CampaignId == campaign.Id && x.Qualification == ClickQualification.Qualified, cancellationToken);
            var breakdown = finances.Calculate(campaign.Cpc, campaign.Budget, qualified);
            if (breakdown.QualifiedClicks >= breakdown.MaximumQualifiedClicks) { click.Reject("Campaign budget exhausted"); campaign.MarkBudgetExhausted(); }
            else
            {
                click.Qualify(HashVisitor(visitorKey));
                db.LedgerEntries.Add(new LedgerEntry(click.Id, campaign.Id, placement.Id, breakdown.AdvertiserCpc, breakdown.CommunityOwnerCpc, breakdown.PlatformFeePerClick));
                var payout = await db.PayoutRecords.SingleOrDefaultAsync(x => x.PlacementId == placement.Id, cancellationToken);
                if (payout is null) db.PayoutRecords.Add(new PayoutRecord(placement.CommunityOwnerId, campaign.Id, placement.Id, breakdown.CommunityOwnerCpc));
                else payout.AddAmount(breakdown.CommunityOwnerCpc);
                if (breakdown.QualifiedClicks + 1 >= breakdown.MaximumQualifiedClicks) campaign.MarkBudgetExhausted();
            }
        }
        db.ClickEvents.Add(click);
        db.ActivityEvents.Add(new ActivityEvent("Qualified click recorded", "Placement", placement.Id, placement.TrackingId, null, click.Qualification.ToString()));
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new TrackingClickResponse(click.ClickId, click.Qualification, click.RejectionReason, destination);
    }

    private async Task<string?> GetRejectionReasonAsync(Placement placement, Campaign campaign, string? visitorKey, CancellationToken cancellationToken)
    {
        if (placement.Status != PlacementStatus.Active) return "Placement is not active.";
        if (campaign.Status != CampaignStatus.Active) return "Campaign is not active.";
        if (!Uri.TryCreate(campaign.DestinationUrl, UriKind.Absolute, out var destination) || destination.Scheme is not ("http" or "https")) return "Destination URL is invalid.";
        if (!string.IsNullOrWhiteSpace(visitorKey) && await db.ClickEvents.AnyAsync(x => x.TrackingId == placement.TrackingId && x.VisitorKeyHash == HashVisitor(visitorKey) && x.Qualification == ClickQualification.Qualified && x.CreatedAt > clock.UtcNow.AddMinutes(-1), cancellationToken)) return "Repeated click from the same browser session.";
        return null;
    }

    private static string? HashVisitor(string? visitorKey) => string.IsNullOrWhiteSpace(visitorKey) ? null : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(visitorKey)));
}
