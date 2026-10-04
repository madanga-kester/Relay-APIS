using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Domain.Entities;
using Relay.Domain.Enums;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class AdminOperationsService(RelayDbContext db, ICurrentUser currentUser, IClock clock) : IAdminOperationsService
{
    private void EnsureAdmin() { if (!currentUser.IsInRole(UserRole.Admin) || currentUser.UserId is not Guid) throw new ForbiddenOperationException("Admin access is required."); }
    private Guid AdminId => currentUser.UserId!.Value;

    public async Task<AdminNoteResponse> AddNoteAsync(CreateAdminNoteRequest request, CancellationToken cancellationToken)
    {
        EnsureAdmin();
        if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length > 4000) throw new ValidationFailureException(["Internal note must be between 1 and 4000 characters."]);
        var note = new AdminNote(request.EntityType, request.EntityId, request.EntityName, request.Text, AdminId);
        db.AdminNotes.Add(note); await db.SaveChangesAsync(cancellationToken); return ToResponse(note);
    }
    public async Task<bool> DeleteNoteAsync(Guid id, CancellationToken cancellationToken)
    { EnsureAdmin(); var note = await db.AdminNotes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (note is null) return false; db.AdminNotes.Remove(note); db.ActivityEvents.Add(new ActivityEvent("Admin note deleted", "AdminNote", id, note.EntityName, AdminId, "Internal Admin note deleted.")); await db.SaveChangesAsync(cancellationToken); return true; }
    public async Task<PageResult<AdminNoteResponse>> ListNotesAsync(string? entityType, Guid? entityId, PageRequest page, CancellationToken cancellationToken)
    {
        EnsureAdmin(); var query = db.AdminNotes.AsNoTracking().AsQueryable(); if (!string.IsNullOrWhiteSpace(entityType)) query = query.Where(x => x.EntityType == entityType); if (entityId is Guid id) query = query.Where(x => x.EntityId == id); return await Page(query, page, ToResponse, cancellationToken);
    }
    public async Task<ReviewCaseResponse> CreateReviewCaseAsync(CreateReviewCaseRequest request, CancellationToken cancellationToken)
    {
        EnsureAdmin(); var item = new ReviewCase(request.EntityType, request.EntityId, request.EntityName, request.Reason, AdminId); db.ReviewCases.Add(item); await db.SaveChangesAsync(cancellationToken); return ToResponse(item);
    }
    public async Task<ReviewCaseResponse?> ResolveReviewCaseAsync(Guid id, ResolveReviewCaseRequest request, CancellationToken cancellationToken)
    {
        EnsureAdmin(); var item = await db.ReviewCases.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (item is null) return null; item.Resolve(request.Status, request.Action, AdminId, clock.UtcNow); await db.SaveChangesAsync(cancellationToken); return ToResponse(item);
    }
    public async Task<PageResult<ReviewCaseResponse>> ListReviewCasesAsync(PageRequest page, CancellationToken cancellationToken)
    { EnsureAdmin(); return await Page(db.ReviewCases.AsNoTracking(), page, ToResponse, cancellationToken); }
    public async Task<PageResult<PayoutResponse>> ListPayoutsAsync(PageRequest page, CancellationToken cancellationToken)
    { EnsureAdmin(); return await Page(db.PayoutRecords.AsNoTracking(), page, ToResponse, cancellationToken); }
    public async Task<PayoutResponse?> ChangePayoutStatusAsync(Guid id, ChangePayoutStatusRequest request, CancellationToken cancellationToken)
    { EnsureAdmin(); var payout = await db.PayoutRecords.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (payout is null) return null; payout.ChangeStatus(request.Status, request.FailureReason); await db.SaveChangesAsync(cancellationToken); return ToResponse(payout); }
    public async Task<PageResult<AdminNotificationResponse>> ListNotificationsAsync(PageRequest page, CancellationToken cancellationToken)
    { EnsureAdmin(); return await Page(db.AdminNotifications.AsNoTracking(), page, ToResponse, cancellationToken); }
    public async Task<AdminNotificationResponse?> MarkNotificationReadAsync(Guid id, CancellationToken cancellationToken)
    { EnsureAdmin(); var item = await db.AdminNotifications.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (item is null) return null; item.MarkRead(clock.UtcNow); await db.SaveChangesAsync(cancellationToken); return ToResponse(item); }
    public async Task<AdminSettingResponse?> GetSettingAsync(string key, CancellationToken cancellationToken)
    { EnsureAdmin(); var item = await db.AdminSettings.AsNoTracking().SingleOrDefaultAsync(x => x.Key == key, cancellationToken); return item is null ? null : new(item.Key, item.Value); }
    public async Task<AdminSettingResponse> SaveSettingAsync(string key, SaveAdminSettingRequest request, CancellationToken cancellationToken)
    { EnsureAdmin(); if (string.IsNullOrWhiteSpace(key) || key.Length > 120 || request.Value.Length > 10000) throw new ValidationFailureException(["Setting key or value is invalid."]); var item = await db.AdminSettings.SingleOrDefaultAsync(x => x.Key == key, cancellationToken); if (item is null) db.AdminSettings.Add(item = new AdminSetting(key, request.Value, AdminId)); else item.Update(request.Value, AdminId, clock.UtcNow); await db.SaveChangesAsync(cancellationToken); return new(item.Key, item.Value); }
    public async Task<bool> ChangeUserStatusAsync(Guid id, ChangeUserStatusRequest request, CancellationToken cancellationToken)
    { EnsureAdmin(); var user = await db.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (user is null) return false; if (request.Status == AccountStatus.Suspended) user.Suspend(); else if (request.Status == AccountStatus.Active) user.Activate(); else throw new ValidationFailureException(["Unsupported user status action."]); db.ActivityEvents.Add(new ActivityEvent("User status changed", "User", id, user.DisplayName, AdminId, $"User status changed to {request.Status}.")); await db.SaveChangesAsync(cancellationToken); return true; }
    public async Task<bool> ChangeCommunityStatusAsync(Guid id, ChangeCommunityStatusRequest request, CancellationToken cancellationToken)
    { EnsureAdmin(); var community = await db.Communities.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (community is null) return false; if (request.Status == VerificationStatus.Verified) community.Verify(); else if (request.Status == VerificationStatus.Suspended) community.Suspend(); else throw new ValidationFailureException(["Unsupported community status action."]); db.ActivityEvents.Add(new ActivityEvent("Community moderated", "Community", id, community.Name, AdminId, $"Community status changed to {request.Status}.")); await db.SaveChangesAsync(cancellationToken); return true; }
    public async Task<bool> ChangePlacementStatusAsync(Guid id, ChangePlacementStatusRequest request, CancellationToken cancellationToken)
    { EnsureAdmin(); var placement = await db.Placements.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (placement is null) return false; if (request.Status == PlacementStatus.Active) placement.Activate(clock.UtcNow); else if (request.Status == PlacementStatus.Completed) placement.Complete(clock.UtcNow); else throw new ValidationFailureException(["Unsupported placement status action."]); db.ActivityEvents.Add(new ActivityEvent("Placement status changed", "Placement", id, placement.TrackingId, AdminId, $"Placement status changed to {request.Status}.")); await db.SaveChangesAsync(cancellationToken); return true; }

    private static async Task<PageResult<TResponse>> Page<TEntity, TResponse>(IQueryable<TEntity> query, PageRequest page, Func<TEntity, TResponse> map, CancellationToken cancellationToken) where TEntity : AuditableEntity
    { var total = await query.CountAsync(cancellationToken); var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page.SafePage - 1) * page.SafePageSize).Take(page.SafePageSize).ToListAsync(cancellationToken); return new(items.Select(map).ToList(), page.SafePage, page.SafePageSize, total); }
    private static AdminNoteResponse ToResponse(AdminNote x) => new(x.Id, x.EntityType, x.EntityId, x.EntityName, x.Text, x.AuthorId, x.CreatedAt);
    private static ReviewCaseResponse ToResponse(ReviewCase x) => new(x.Id, x.EntityType, x.EntityId, x.EntityName, x.Reason, x.Status, x.ResolutionAction, x.CreatedAt, x.ResolvedAt);
    private static PayoutResponse ToResponse(PayoutRecord x) => new(x.Id, x.CommunityOwnerId, x.CampaignId, x.PlacementId, x.Amount, x.Status, x.FailureReason, x.CreatedAt);
    private static AdminNotificationResponse ToResponse(AdminNotification x) => new(x.Id, x.Type, x.Title, x.Description, x.Href, x.EntityId, x.ReadAt is not null, x.CreatedAt);
}
