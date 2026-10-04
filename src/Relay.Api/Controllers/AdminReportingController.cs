using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Contracts;
using Relay.Application.Services;

namespace Relay.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin")]
public sealed class AdminReportingController(IAdminReportingService reporting) : ControllerBase
{
    [HttpGet("reports")]
    public Task<AdminReportResponse> Report([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken cancellationToken) => reporting.GetReportAsync(from, to, cancellationToken);

    [HttpGet("health")]
    public Task<AdminHealthResponse> Health(CancellationToken cancellationToken) => reporting.GetHealthAsync(cancellationToken);
}
