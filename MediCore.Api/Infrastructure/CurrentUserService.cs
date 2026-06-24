using System.Security.Claims;
using MediCore.Api.Services;

namespace MediCore.Api.Infrastructure;

// Ricava l'identità dell'utente corrente dal contesto HTTP.
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User?.Identity?.Name;
}
