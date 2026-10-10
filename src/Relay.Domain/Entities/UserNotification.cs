using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Relay.Domain.Entities;

public sealed class UserNotification : AuditableEntity
{
    private UserNotification() { }

    public UserNotification(Guid userId, string type, string title, string body, string? href, string? platform = null)
    {
        UserId = userId;
        Type = type.Trim();
        Title = title.Trim();
        Body = body.Trim();
        Href = string.IsNullOrWhiteSpace(href) ? null : href.Trim();
        Platform = string.IsNullOrWhiteSpace(platform) ? null : platform.Trim();
    }

    public Guid UserId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? Href { get; private set; }
    public string? Platform { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    public void MarkRead(DateTimeOffset now)
    {
        ReadAt ??= now;
    }
}