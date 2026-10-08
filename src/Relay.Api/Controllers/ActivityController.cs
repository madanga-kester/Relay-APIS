using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Contracts;
using Relay.Application.Services;

namespace Relay.Api.Controllers;

[ApiController]
[Authorize(Roles = "Advertiser,CommunityOwner")]
[Route("api/v1/activity")]
public sealed class ActivityController(IActivityFeedService activity) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ActivityFeedItemResponse>> Get([FromQuery] int limit = 30, CancellationToken cancellationToken = default) => activity.GetAsync(limit, cancellationToken);
}