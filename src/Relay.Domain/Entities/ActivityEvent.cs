namespace Relay.Domain.Entities;

public sealed class ActivityEvent : AuditableEntity
{
    private ActivityEvent() { }

    public ActivityEvent(string eventType, string entityType, Guid entityId, string entityName, Guid? actorId, string detail)
    {
        EventType = eventType.Trim();
        EntityType = entityType.Trim();
        EntityId = entityId;
        EntityName = entityName.Trim();
        ActorId = actorId;
        Detail = detail.Trim();
    }

    public string EventType { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string EntityName { get; private set; } = string.Empty;
    public Guid? ActorId { get; private set; }
    public string Detail { get; private set; } = string.Empty;
}
