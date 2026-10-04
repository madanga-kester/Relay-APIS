using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;

namespace Relay.Api.Controllers;

[ApiController]
[Route("api/v1/applications")]
[Authorize]
public sealed class ApplicationsController(IApplicationService applications) : ControllerBase
{
    [HttpGet("mine")]
    public Task<PageResult<ApplicationResponse>> Mine([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default) => applications.MineAsync(new PageRequest(page, pageSize), cancellationToken);

    [HttpPost]
    [Authorize(Roles = "CommunityOwner")]
    public async Task<ActionResult<ApplicationResponse>> Apply(ApplyToCampaignRequest request, CancellationToken cancellationToken) => Ok(await applications.ApplyAsync(request, cancellationToken));

    [HttpPost("{id:guid}/review")]
    [Authorize(Roles = "Advertiser,Admin")]
    public async Task<ActionResult<ApplicationResponse>> Review(Guid id, ReviewApplicationRequest request, CancellationToken cancellationToken) => await applications.ReviewAsync(id, request, cancellationToken) is { } result ? Ok(result) : NotFound();
}
