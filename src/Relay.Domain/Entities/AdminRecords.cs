using Relay.Domain.Enums;

namespace Relay.Domain.Entities;

public sealed class AdminNote : AuditableEntity
{
    private AdminNote() { }
    public AdminNote(string entityType, Guid entityId, string entityName, string text, Guid authorId) { EntityType = entityType.Trim(); EntityId = entityId; EntityName = entityName.Trim(); Text = text.Trim(); AuthorId = authorId; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string EntityName { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public Guid AuthorId { get; private set; }
}

public sealed class ReviewCase : AuditableEntity
{
    private ReviewCase() { }
    public ReviewCase(string entityType, Guid entityId, string entityName, string reason, Guid? reportedBy) { EntityType = entityType.Trim(); EntityId = entityId; EntityName = entityName.Trim(); Reason = reason.Trim(); ReportedBy = reportedBy; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string EntityName { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public ReviewCaseStatus Status { get; private set; } = ReviewCaseStatus.Open;
    public string? ResolutionAction { get; private set; }
    public Guid? ReportedBy { get; private set; }
    public Guid? ResolvedBy { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public void Resolve(ReviewCaseStatus status, string action, Guid adminId, DateTimeOffset now) { if (status is ReviewCaseStatus.Open) throw new InvalidOperationException("A review case must resolve to a closed status."); Status = status; ResolutionAction = action.Trim(); ResolvedBy = adminId; ResolvedAt = now; }
}

public sealed class PayoutRecord : AuditableEntity
{
    private PayoutRecord() { }
    public PayoutRecord(Guid ownerId, Guid campaignId, Guid placementId, decimal amount) { CommunityOwnerId = ownerId; CampaignId = campaignId; PlacementId = placementId; Amount = amount; }
    public Guid CommunityOwnerId { get; private set; }
    public Guid CampaignId { get; private set; }
    public Guid PlacementId { get; private set; }
    public decimal Amount { get; private set; }
    public PayoutStatus Status { get; private set; } = PayoutStatus.Pending;
    public string? FailureReason { get; private set; }
    public void AddAmount(decimal amount) { ArgumentOutOfRangeException.ThrowIfNegative(amount); Amount += amount; }
    public void ChangeStatus(PayoutStatus status, string? failureReason = null) { Status = status; FailureReason = string.IsNullOrWhiteSpace(failureReason) ? null : failureReason.Trim(); }
}

public sealed class AdminNotification : AuditableEntity
{
    private AdminNotification() { }
    public AdminNotification(string type, string title, string description, string href, Guid? entityId) { Type = type.Trim(); Title = title.Trim(); Description = description.Trim(); Href = href.Trim(); EntityId = entityId; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Href { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public void MarkRead(DateTimeOffset now) => ReadAt = now;
}

public sealed class AdminSetting : AuditableEntity
{
    private AdminSetting() { }
    public AdminSetting(string key, string value, Guid updatedBy) { Key = key.Trim(); Value = value; UpdatedBy = updatedBy; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public Guid UpdatedBy { get; private set; }
    public void Update(string value, Guid updatedBy, DateTimeOffset now) { Value = value; UpdatedBy = updatedBy; Touch(now); }
}
