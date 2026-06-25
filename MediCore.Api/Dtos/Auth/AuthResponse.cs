namespace MediCore.Api.Dtos.Auth;

// Risposta restituita dopo registrazione/login: token e dati essenziali dell'utente.
public record AuthResponse
{
    public string Token { get; init; } = null!;
    public DateTime ExpiresAtUtc { get; init; }
    public string Email { get; init; } = null!;
    public IEnumerable<string> Ruoli { get; init; } = Array.Empty<string>();
}
