namespace Relay.Application.Services;

public interface IEmailSender
{
    Task SendPasswordResetAsync(string recipient, string displayName, string resetLink, CancellationToken cancellationToken);
    Task SendEmailVerificationAsync(string recipient, string displayName, string code, CancellationToken cancellationToken);
}

public interface IObjectStorage
{
    Task<string> PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken);
    Task<string> CreateReadUrlAsync(string key, TimeSpan lifetime, CancellationToken cancellationToken);
}
