using System.Text.Json;

namespace Relay.Application.Contracts
{
    public sealed record SavePreferencesRequest(JsonElement Values);
    public sealed record PreferencesResponse(JsonElement Values);
}

namespace Relay.Application.Services
{
    using Relay.Application.Contracts;

    public interface IPreferencesService
    {
        Task<PreferencesResponse> GetAsync(CancellationToken cancellationToken);
        Task<PreferencesResponse> SaveAsync(SavePreferencesRequest request, CancellationToken cancellationToken);
    }
}