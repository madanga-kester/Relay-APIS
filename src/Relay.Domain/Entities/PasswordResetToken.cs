namespace Relay.Domain.Entities;

public sealed class PasswordResetToken : AuditableEntity
{
    private PasswordResetToken() { }

    public PasswordResetToken(Guid userId, string tokenHash, DateTimeOffset expiresAt)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }

    public bool IsUsable(DateTimeOffset now) => UsedAt is null && ExpiresAt > now;
    public void MarkUsed(DateTimeOffset now) => UsedAt = now;
}
