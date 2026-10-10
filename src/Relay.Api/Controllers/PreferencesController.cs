using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Contracts;
using Relay.Application.Services;

namespace Relay.Api.Controllers;

[ApiController]
[Route("api/v1/preferences")]
[Authorize]
public sealed class PreferencesController(IPreferencesService preferences) : ControllerBase
{
    [HttpGet]
    public Task<PreferencesResponse> Get(CancellationToken cancellationToken) => preferences.GetAsync(cancellationToken);

    [HttpPut]
    public async Task<ActionResult<PreferencesResponse>> Save(SavePreferencesRequest request, CancellationToken cancellationToken)
        => Ok(await preferences.SaveAsync(request, cancellationToken));
}