using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class NotificationService(RelayDbContext db, ICurrentUser currentUser, IClock clock) : INotificationService
{
    public async Task<PageResult<NotificationResponse>> MineAsync(PageRequest page, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new ForbiddenOperationException("Authentication is required.");
        }

        var query = db.UserNotifications.AsNoTracking().Where(x => x.UserId == userId);
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page.SafePage - 1) * page.SafePageSize)
            .Take(page.SafePageSize)
            .ToListAsync(cancellationToken);

        var responses = items
            .Select(x => new NotificationResponse(x.Id, x.Type, x.Title, x.Body, x.Href, x.ReadAt is not null, x.CreatedAt))
            .ToList();

        return new PageResult<NotificationResponse>(responses, page.SafePage, page.SafePageSize, total);
    }

    public async Task<bool> MarkReadAsync(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new ForbiddenOperationException("Authentication is required.");
        }

        var notification = await db.UserNotifications
            .SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (notification is null)
        {
            return false;
        }

        notification.MarkRead(clock.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> MarkAllReadAsync(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new ForbiddenOperationException("Authentication is required.");
        }

        var unread = await db.UserNotifications
            .Where(x => x.UserId == userId && x.ReadAt == null)
            .ToListAsync(cancellationToken);

        var now = clock.UtcNow;
        foreach (var notification in unread)
        {
            notification.MarkRead(now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return unread.Count;
    }
}