using Relay.Domain.Enums;

namespace Relay.Domain.Entities;

public sealed class UserAccount : AuditableEntity
{
    private UserAccount() { }

    public UserAccount(string email, string displayName, UserRole role)
    {
        Email = email;
        NormalizedEmail = NormalizeEmail(email);
        DisplayName = displayName;
        Role = role;
    }

    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public AccountStatus Status { get; private set; } = AccountStatus.Active;
    public bool EmailConfirmed { get; private set; }
    public DateTimeOffset? LastSignedInAt { get; private set; }

    public void SetPasswordHash(string hash) => PasswordHash = hash;
    public void ConfirmEmail() => EmailConfirmed = true;
    public void RecordSignIn(DateTimeOffset now) => LastSignedInAt = now;
    public void Suspend() => Status = AccountStatus.Suspended;
    public void Activate() => Status = AccountStatus.Active;
    public void ChangeProfile(string email, string displayName)
    {
        Email = email;
        NormalizedEmail = NormalizeEmail(email);
        DisplayName = displayName;
        Touch(DateTimeOffset.UtcNow);
    }

    private static string NormalizeEmail(string value) => value.Trim().ToUpperInvariant();
}
