namespace Relay.Domain.Entities;

public abstract class AuditableEntity
{
    public Guid Id { get; protected init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; protected set; } = DateTimeOffset.UtcNow;

    public void Touch(DateTimeOffset now) => UpdatedAt = now;
}
