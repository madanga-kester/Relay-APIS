using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;

namespace Relay.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public sealed class NotificationsController(INotificationService notifications) : ControllerBase
{
    [HttpGet]
    public Task<PageResult<NotificationResponse>> Mine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
        => notifications.MineAsync(new PageRequest(page, pageSize), cancellationToken);

    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
        => await notifications.MarkReadAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpPost("read-all")]
    public async Task<ActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await notifications.MarkAllReadAsync(cancellationToken);
        return NoContent();
    }
}