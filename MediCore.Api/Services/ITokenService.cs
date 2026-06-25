using MediCore.Api.Domain.Entities;

namespace MediCore.Api.Services;

// Genera il token JWT di accesso per un utente.
public interface ITokenService
{
    TokenResult CreateToken(AppUser user, IList<string> roles);
}

// Esito della generazione: token firmato e relativo istante di scadenza (UTC).
public record TokenResult(string Token, DateTime ExpiresAtUtc);
