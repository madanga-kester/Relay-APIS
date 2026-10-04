using System.Security.Claims;
using Relay.Application.Common;
using Relay.Domain.Enums;

namespace Relay.Api.Auth;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal Principal => accessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
    public Guid? UserId => Guid.TryParse(Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public UserRole? Role => Enum.TryParse<UserRole>(Principal.FindFirstValue(ClaimTypes.Role), out var role) ? role : null;
    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;
    public bool IsInRole(UserRole role) => Role == role;
}
