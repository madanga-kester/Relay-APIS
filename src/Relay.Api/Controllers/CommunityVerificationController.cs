using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Application.Services;
using Relay.Infrastructure.Persistence;

namespace Relay.Api.Controllers;

[ApiController]
[Authorize(Roles = "CommunityOwner,Admin")]
[Route("api/v1/communities/verification-evidence")]
public sealed class CommunityVerificationController(IObjectStorage storage, RelayDbContext db) : ControllerBase
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase) { "image/png", "image/jpeg", "application/pdf" };
    private const long MaxBytes = 10 * 1024 * 1024;

    [HttpPost]
    [RequestSizeLimit(MaxBytes)]
    public async Task<ActionResult<object>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest(new { title = "A verification file is required." });
        if (file.Length > MaxBytes) return BadRequest(new { title = "Verification files must be 10 MB or smaller." });
        if (!AllowedTypes.Contains(file.ContentType)) return BadRequest(new { title = "Only PNG, JPEG, and PDF verification files are supported." });
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var ownerId)) return Unauthorized();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var key = $"community-verification/{ownerId:N}/{Guid.NewGuid():N}{extension}";
        await using var stream = file.OpenReadStream();
        var storedKey = await storage.PutAsync(key, stream, file.ContentType, cancellationToken);
        return Ok(new { key = storedKey, contentType = file.ContentType, size = file.Length });
    }

    [HttpGet("{communityId:guid}/url")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> ReadUrl(Guid communityId, CancellationToken cancellationToken)
    {
        var community = await db.Communities.FindAsync([communityId], cancellationToken);
        if (community is null || string.IsNullOrWhiteSpace(community.VerificationEvidenceKey)) return NotFound();
        var url = await storage.CreateReadUrlAsync(community.VerificationEvidenceKey, TimeSpan.FromMinutes(10), cancellationToken);
        return Ok(new { url, expiresInSeconds = 600 });
    }
}
