using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Services;

namespace Relay.Api.Controllers;

[ApiController]
[Route("api/v1/tracking")]
public sealed class TrackingController(ITrackingService tracking) : ControllerBase
{
    [HttpPost("{trackingId}/click")]
    [AllowAnonymous]
    public async Task<ActionResult> Click(string trackingId, CancellationToken cancellationToken)
    {
        var result = await tracking.ProcessClickAsync(trackingId, Request.Headers["Idempotency-Key"].FirstOrDefault(), Request.Headers["X-Visitor-Key"].FirstOrDefault(), cancellationToken);
        return result is null ? NotFound(new { title = "Tracking link unavailable", status = 404 }) : Ok(result);
    }
}
