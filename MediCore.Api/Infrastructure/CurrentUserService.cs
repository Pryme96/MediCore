using System.Security.Claims;
using MediCore.Api.Services;

namespace MediCore.Api.Infrastructure;

// Ricava l'identità dell'utente corrente dal contesto HTTP.
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? UserName =>
        httpContextAccessor.HttpContext?.User?.Identity?.Name;
}
