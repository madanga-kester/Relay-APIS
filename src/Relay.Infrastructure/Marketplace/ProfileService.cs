using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Application.Validation;
using Relay.Domain.Entities;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class ProfileService(RelayDbContext db, ICurrentUser currentUser, IClock clock) : IProfileService
{
    public async Task<ProfileResponse?> GetAsync(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId) throw new UnauthorizedAccessException();
        var profile = await db.UserProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        return profile is null ? null : ToResponse(profile);
    }

    public async Task<ProfileResponse> SaveAsync(SaveProfileRequest request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId) throw new UnauthorizedAccessException();
        var errors = RequestValidators.Validate(request);
        if (errors.Count > 0) throw new ValidationFailureException(errors);

        var profile = await db.UserProfiles.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (profile is null) { profile = new UserProfile(userId); db.UserProfiles.Add(profile); }
        profile.Update(request.BusinessName, request.Industry, request.Website, request.Location, request.PrimaryGoal, request.PhoneNumber, request.AvatarKey, request.OnboardingCompleted, request.CommunityName, request.CommunityPlatform, request.CommunityMembers, request.CommunityCategory, clock.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(profile);
    }

    private static ProfileResponse ToResponse(UserProfile x) => new(x.UserId, x.BusinessName, x.Industry, x.Website, x.Location, x.PrimaryGoal, x.PhoneNumber, x.AvatarKey, x.OnboardingCompleted, x.CommunityName, x.CommunityPlatform, x.CommunityMembers, x.CommunityCategory);
}
