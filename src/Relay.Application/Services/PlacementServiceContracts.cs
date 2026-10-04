using Relay.Application.Common;
using Relay.Application.Contracts;

namespace Relay.Application.Services;

public interface IPlacementService
{
    Task<PageResult<PlacementResponse>> MineAsync(PageRequest page, CancellationToken cancellationToken);
    Task<PlacementResponse?> ActivateAsync(Guid id, CancellationToken cancellationToken);
    Task<PlacementResponse?> CompleteAsync(Guid id, CancellationToken cancellationToken);

}
