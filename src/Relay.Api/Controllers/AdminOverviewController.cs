using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Contracts;
using Relay.Application.Services;

namespace Relay.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/overview")]
public sealed class AdminOverviewController(IAdminOverviewService overview) : ControllerBase
{
    [HttpGet]
    public Task<AdminOverviewResponse> Get(CancellationToken cancellationToken) => overview.GetAsync(cancellationToken);
}
