using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Application.Validation;
using Relay.Domain.Entities;
using Relay.Domain.Enums;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class CommunityService(RelayDbContext db, ICurrentUser currentUser) : ICommunityService
{
    public async Task<PageResult<CommunityResponse>> ListAsync(PageRequest page, CancellationToken cancellationToken)
    {
        var query = db.Communities.AsNoTracking().Where(x => x.VerificationStatus != VerificationStatus.Suspended);
        if (currentUser.Role != UserRole.Admin && currentUser.UserId is Guid ownerId) query = query.Where(x => x.OwnerId == ownerId || x.VerificationStatus == VerificationStatus.Verified);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page.SafePage - 1) * page.SafePageSize).Take(page.SafePageSize).ToListAsync(cancellationToken);
        return new PageResult<CommunityResponse>(items.Select(ToResponse).ToList(), page.SafePage, page.SafePageSize, total);
    }


    public async Task<PageResult<CommunityResponse>> MineAsync(PageRequest page, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid ownerId) throw new ForbiddenOperationException("Authentication is required.");
        var query = db.Communities.AsNoTracking().Where(x => x.OwnerId == ownerId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page.SafePage - 1) * page.SafePageSize).Take(page.SafePageSize).ToListAsync(cancellationToken);
        return new PageResult<CommunityResponse>(items.Select(ToResponse).ToList(), page.SafePage, page.SafePageSize, total);
    }

   
    public async Task<CommunityResponse> CreateAsync(CreateCommunityRequest request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(UserRole.CommunityOwner) || currentUser.UserId is null) throw new ForbiddenOperationException("Community Owner access is required.");
        var errors = RequestValidators.Validate(request);
        if (errors.Count > 0) throw new ValidationFailureException(errors);
        var community = new Community(currentUser.UserId.Value, request.Name, request.Platform, request.Members, request.Category, request.Location, request.CommunityLink, request.AudienceDescription, request.VerificationEvidenceKey);
        db.Communities.Add(community);
        db.ActivityEvents.Add(new ActivityEvent("Community created", "Community", community.Id, community.Name, currentUser.UserId, "Community submitted for verification."));
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(community);
    }

    private static CommunityResponse ToResponse(Community community) => new(community.Id, community.OwnerId, community.Name, community.Platform, community.Members, community.Category, community.Location, community.CommunityLink, community.AudienceDescription, community.VerificationEvidenceKey, community.VerificationStatus);
}
