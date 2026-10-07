using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;

namespace Relay.Api.Controllers;

[ApiController]
[Route("api/v1/campaigns")]
public sealed class CampaignsController(ICampaignService campaigns) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public Task<PageResult<CampaignResponse>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default) => campaigns.ListAsync(new PageRequest(page, pageSize), cancellationToken);


    [HttpGet("mine")]
    [Authorize(Roles = "Advertiser,Admin")]
    public Task<PageResult<CampaignResponse>> Mine([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default) => campaigns.MineAsync(new PageRequest(page, pageSize), cancellationToken);
    [HttpGet("billing-activity")]
    [Authorize(Roles = "Advertiser")]
    public Task<IReadOnlyList<BillingActivityResponse>> BillingActivity(CancellationToken cancellationToken = default) => campaigns.BillingActivityAsync(cancellationToken);

    [HttpGet("performance")]
    [Authorize(Roles = "Advertiser")]
    public Task<IReadOnlyList<CampaignPerformanceResponse>> Performance(CancellationToken cancellationToken = default) => campaigns.PerformanceAsync(cancellationToken);



    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CampaignResponse>> Get(Guid id, CancellationToken cancellationToken) => await campaigns.GetAsync(id, cancellationToken) is { } campaign ? Ok(campaign) : NotFound();

    [HttpPost]
    [Authorize(Roles = "Advertiser,Admin")]
    public async Task<ActionResult<CampaignResponse>> Create(CreateCampaignRequest request, CancellationToken cancellationToken) => Ok(await campaigns.CreateAsync(request, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Advertiser,Admin")]
    public async Task<ActionResult<CampaignResponse>> Update(Guid id, UpdateCampaignRequest request, CancellationToken cancellationToken) => await campaigns.UpdateAsync(id, request, cancellationToken) is { } campaign ? Ok(campaign) : NotFound();

    [HttpPost("{id:guid}/actions/{operation}")]
    [Authorize(Roles = "Advertiser,Admin")]
    public async Task<ActionResult<CampaignResponse>> Transition(Guid id, string operation, CancellationToken cancellationToken) => await campaigns.TransitionAsync(id, operation, cancellationToken) is { } campaign ? Ok(campaign) : NotFound();

}
