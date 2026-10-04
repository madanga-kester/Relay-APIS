using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;

namespace Relay.Api.Controllers;

[ApiController]
[Route("api/v1/communities")]
public sealed class CommunitiesController(ICommunityService communities) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public Task<PageResult<CommunityResponse>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default) => communities.ListAsync(new PageRequest(page, pageSize), cancellationToken);

    [HttpGet("mine")]
    [Authorize(Roles = "CommunityOwner,Admin")]
    public Task<PageResult<CommunityResponse>> Mine([FromQuery] int page = 1, [FromQuery] int pageSize = 100, CancellationToken cancellationToken = default) => communities.MineAsync(new PageRequest(page, pageSize), cancellationToken);

   
    [HttpPost]
    [Authorize(Roles = "CommunityOwner")]
    public async Task<ActionResult<CommunityResponse>> Create(CreateCommunityRequest request, CancellationToken cancellationToken) => Ok(await communities.CreateAsync(request, cancellationToken));
}
