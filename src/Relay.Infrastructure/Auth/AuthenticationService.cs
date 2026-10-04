#pragma warning disable CA1862
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Application.Validation;
using Relay.Domain.Entities;
using Relay.Domain.Enums;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Auth;

public sealed class AuthenticationService(RelayDbContext db, IEmailSender emailSender, IClock clock) : IAuthenticationService
{
    private readonly PasswordHasher<UserAccount> hasher = new();

    public async Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var errors = RequestValidators.Validate(request);
        if (errors.Count > 0) throw new ValidationFailureException(errors);
        var email = request.Email.Trim();
        var normalized = Normalize(email);
        if (await db.Users.AnyAsync(x => x.NormalizedEmail == normalized, cancellationToken)) throw new ConflictException("An account with this email already exists.");
        var user = new UserAccount(email, request.DisplayName.Trim(), request.Role);
        user.SetPasswordHash(hasher.HashPassword(user, request.Password));
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        await IssueVerificationCodeAsync(user, cancellationToken);
        return ToResponse(user);
    }

    public async Task<UserResponse?> ValidateCredentialsAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var errors = RequestValidators.Validate(request);
        if (errors.Count > 0) throw new ValidationFailureException(errors);
        var user = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == Normalize(request.Email), cancellationToken);
        if (user is null || user.Status != AccountStatus.Active) return null;
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed) return null;
        if (!user.EmailConfirmed) throw new ForbiddenOperationException("Please verify your email before signing in.");
        user.RecordSignIn(clock.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == Normalize(request.Email), cancellationToken);
        if (user is null) return;
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken)));
        db.PasswordResetTokens.Add(new PasswordResetToken(user.Id, tokenHash, clock.UtcNow.AddMinutes(30)));
        await db.SaveChangesAsync(cancellationToken);
        await emailSender.SendPasswordResetAsync(user.Email, user.DisplayName, $"/reset-password?token={Uri.EscapeDataString(rawToken)}", cancellationToken);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || request.NewPassword.Length < 12) return false;
        var hash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.Token)));
        var token = await db.PasswordResetTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (token is null || !token.IsUsable(clock.UtcNow)) return false;
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == token.UserId, cancellationToken);
        if (user is null) return false;
        user.SetPasswordHash(hasher.HashPassword(user, request.NewPassword));
        token.MarkUsed(clock.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code)) return false;
        var user = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == Normalize(request.Email), cancellationToken);
        if (user is null || user.EmailConfirmed) return false;
        var latest = await db.EmailVerificationCodes.Where(x => x.UserId == user.Id && x.UsedAt == null).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        if (latest is null || !latest.IsUsable(clock.UtcNow)) return false;
        var submitted = System.Text.Encoding.UTF8.GetBytes(HashVerificationCode(user.Id, request.Code.Trim()));
        if (!CryptographicOperations.FixedTimeEquals(submitted, System.Text.Encoding.UTF8.GetBytes(latest.CodeHash)))
        {
            latest.RegisterFailure();
            await db.SaveChangesAsync(cancellationToken);
            return false;
        }
        latest.MarkUsed(clock.UtcNow);
        user.ConfirmEmail();
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email)) return;
        var user = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == Normalize(request.Email), cancellationToken);
        if (user is null || user.EmailConfirmed) return;
        var latest = await db.EmailVerificationCodes.Where(x => x.UserId == user.Id).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        if (latest is not null && latest.CreatedAt > clock.UtcNow.AddSeconds(-30)) return;
        await IssueVerificationCodeAsync(user, cancellationToken);
    }

    private async Task IssueVerificationCodeAsync(UserAccount user, CancellationToken cancellationToken)
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
        var pending = await db.EmailVerificationCodes.Where(x => x.UserId == user.Id && x.UsedAt == null).ToListAsync(cancellationToken);
        foreach (var previous in pending) previous.MarkUsed(clock.UtcNow);
        db.EmailVerificationCodes.Add(new EmailVerificationCode(user.Id, HashVerificationCode(user.Id, code), clock.UtcNow.AddMinutes(15)));
        await db.SaveChangesAsync(cancellationToken);
        await emailSender.SendEmailVerificationAsync(user.Email, user.DisplayName, code, cancellationToken);
    }

    private static string HashVerificationCode(Guid userId, string code) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes($"{userId}:{code}")));

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static UserResponse ToResponse(UserAccount user) => new(user.Id, user.Email, user.DisplayName, user.Role, user.Status);
}
#pragma warning restore CA1862