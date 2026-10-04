using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Contracts;
using Relay.Application.Services;

namespace Relay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/profile")]
public sealed class ProfileController(IProfileService profiles) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> Get(CancellationToken cancellationToken)
        => (await profiles.GetAsync(cancellationToken)) is { } profile ? Ok(profile) : NoContent();

    [HttpPut]
    public async Task<ActionResult<ProfileResponse>> Save(SaveProfileRequest request, CancellationToken cancellationToken)
        => Ok(await profiles.SaveAsync(request, cancellationToken));
}
