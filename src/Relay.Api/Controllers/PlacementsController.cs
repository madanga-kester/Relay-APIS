using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;


namespace Relay.Api.Controllers;

[ApiController]
[Route("api/v1/placements")]
[Authorize]
public sealed class PlacementsController(IPlacementService placements) : ControllerBase
{
    [HttpGet("mine")]
    [Authorize(Roles = "Advertiser,CommunityOwner,Admin")]
    public Task<PageResult<PlacementResponse>> Mine([FromQuery] int page = 1, [FromQuery] int pageSize = 100, CancellationToken cancellationToken = default) => placements.MineAsync(new PageRequest(page, pageSize), cancellationToken);



    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "CommunityOwner,Admin")]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken) => await placements.ActivateAsync(id, cancellationToken) is { } result ? Ok(result) : NotFound();

    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "CommunityOwner,Admin")]
    public async Task<ActionResult> Complete(Guid id, CancellationToken cancellationToken) => await placements.CompleteAsync(id, cancellationToken) is { } result ? Ok(result) : NotFound();
}
