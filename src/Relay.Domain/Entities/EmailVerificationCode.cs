namespace Relay.Domain.Entities;

public sealed class EmailVerificationCode : AuditableEntity
{
    public const int MaxFailedAttempts = 5;

    private EmailVerificationCode() { }

    public EmailVerificationCode(Guid userId, string codeHash, DateTimeOffset expiresAt)
    {
        UserId = userId;
        CodeHash = codeHash;
        ExpiresAt = expiresAt;
    }

    public Guid UserId { get; private set; }
    public string CodeHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }
    public int FailedAttempts { get; private set; }

    public bool IsUsable(DateTimeOffset now) => UsedAt is null && ExpiresAt > now && FailedAttempts < MaxFailedAttempts;
    public void RegisterFailure() => FailedAttempts++;
    public void MarkUsed(DateTimeOffset now) => UsedAt = now;
}