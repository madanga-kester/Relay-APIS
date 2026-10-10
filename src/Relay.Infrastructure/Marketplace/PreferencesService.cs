using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Relay.Application.Common;
using Relay.Application.Contracts;
using Relay.Application.Services;
using Relay.Domain.Entities;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Marketplace;

public sealed class PreferencesService(RelayDbContext db, ICurrentUser currentUser, IClock clock) : IPreferencesService
{
    private const int MaxLength = 8000;

    public async Task<PreferencesResponse> GetAsync(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new ForbiddenOperationException("Authentication is required.");
        }

        var json = await db.UserPreferences
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.ValuesJson)
            .SingleOrDefaultAsync(cancellationToken);

        return new PreferencesResponse(Parse(json ?? "{}"));
    }

    public async Task<PreferencesResponse> SaveAsync(SavePreferencesRequest request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new ForbiddenOperationException("Authentication is required.");
        }

        if (request.Values.ValueKind != JsonValueKind.Object)
        {
            throw new ValidationFailureException(["Preferences must be a JSON object."]);
        }

        var json = request.Values.GetRawText();
        if (json.Length > MaxLength)
        {
            throw new ValidationFailureException(["Preferences are too large."]);
        }

        var preference = await db.UserPreferences.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (preference is null)
        {
            db.UserPreferences.Add(new UserPreference(userId, json));
        }
        else
        {
            preference.Update(json, clock.UtcNow);
        }

        await db.SaveChangesAsync(cancellationToken);
        return new PreferencesResponse(Parse(json));
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}