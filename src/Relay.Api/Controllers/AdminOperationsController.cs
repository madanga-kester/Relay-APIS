using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;

namespace Relay.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin")]
public sealed class AdminOperationsController(IAdminOperationsService admin) : ControllerBase
{
    [HttpGet("notes")]
    public Task<PageResult<AdminNoteResponse>> Notes([FromQuery] string? entityType, [FromQuery] Guid? entityId, [FromQuery] PageRequest page, CancellationToken ct) => admin.ListNotesAsync(entityType, entityId, page, ct);
    [HttpPost("notes")]
    public Task<AdminNoteResponse> AddNote(CreateAdminNoteRequest request, CancellationToken ct) => admin.AddNoteAsync(request, ct);
    [HttpDelete("notes/{id:guid}")]
    public async Task<IActionResult> DeleteNote(Guid id, CancellationToken ct) => await admin.DeleteNoteAsync(id, ct) ? NoContent() : NotFound();
    [HttpGet("reviews")]
    public Task<PageResult<ReviewCaseResponse>> Reviews([FromQuery] PageRequest page, CancellationToken ct) => admin.ListReviewCasesAsync(page, ct);
    [HttpPost("reviews")]
    public Task<ReviewCaseResponse> CreateReview(CreateReviewCaseRequest request, CancellationToken ct) => admin.CreateReviewCaseAsync(request, ct);
    [HttpPost("reviews/{id:guid}/resolve")]
    public async Task<ActionResult<ReviewCaseResponse>> ResolveReview(Guid id, ResolveReviewCaseRequest request, CancellationToken ct) => (await admin.ResolveReviewCaseAsync(id, request, ct)) is { } result ? Ok(result) : NotFound();
    [HttpGet("payouts")]
    public Task<PageResult<PayoutResponse>> Payouts([FromQuery] PageRequest page, CancellationToken ct) => admin.ListPayoutsAsync(page, ct);
    [HttpPost("payouts/{id:guid}/status")]
    public async Task<ActionResult<PayoutResponse>> ChangePayout(Guid id, ChangePayoutStatusRequest request, CancellationToken ct) => (await admin.ChangePayoutStatusAsync(id, request, ct)) is { } result ? Ok(result) : NotFound();
    [HttpGet("notifications")]
    public Task<PageResult<AdminNotificationResponse>> Notifications([FromQuery] PageRequest page, CancellationToken ct) => admin.ListNotificationsAsync(page, ct);
    [HttpPost("notifications/{id:guid}/read")]
    public async Task<ActionResult<AdminNotificationResponse>> ReadNotification(Guid id, CancellationToken ct) => (await admin.MarkNotificationReadAsync(id, ct)) is { } result ? Ok(result) : NotFound();
    [HttpGet("settings/{key}")]
    public async Task<ActionResult<AdminSettingResponse>> Setting(string key, CancellationToken ct) => (await admin.GetSettingAsync(key, ct)) is { } result ? Ok(result) : NotFound();
    [HttpPut("settings/{key}")]
    public Task<AdminSettingResponse> SaveSetting(string key, SaveAdminSettingRequest request, CancellationToken ct) => admin.SaveSettingAsync(key, request, ct);
    [HttpPost("users/{id:guid}/status")]
    public async Task<IActionResult> ChangeUserStatus(Guid id, ChangeUserStatusRequest request, CancellationToken ct) => await admin.ChangeUserStatusAsync(id, request, ct) ? NoContent() : NotFound();
    [HttpPost("communities/{id:guid}/status")]
    public async Task<IActionResult> ChangeCommunityStatus(Guid id, ChangeCommunityStatusRequest request, CancellationToken ct) => await admin.ChangeCommunityStatusAsync(id, request, ct) ? NoContent() : NotFound();
    [HttpPost("placements/{id:guid}/status")]
    public async Task<IActionResult> ChangePlacementStatus(Guid id, ChangePlacementStatusRequest request, CancellationToken ct) => await admin.ChangePlacementStatusAsync(id, request, ct) ? NoContent() : NotFound();
}
