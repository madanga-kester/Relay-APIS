using Relay.Application.Common;

namespace Relay.Application.Contracts
{
    public sealed record NotificationResponse(
        Guid Id,
        string Type,
        string Title,
        string Body,
        string? Href,
        bool Read,
        DateTimeOffset CreatedAt);
}

namespace Relay.Application.Services
{
    using Relay.Application.Contracts;

    public interface INotificationService
    {
        Task<PageResult<NotificationResponse>> MineAsync(PageRequest page, CancellationToken cancellationToken);
        Task<bool> MarkReadAsync(Guid id, CancellationToken cancellationToken);
        Task<int> MarkAllReadAsync(CancellationToken cancellationToken);
    }
}