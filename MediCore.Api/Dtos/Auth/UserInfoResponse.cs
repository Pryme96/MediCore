namespace MediCore.Api.Dtos.Auth;

// Dati dell'utente autenticato restituiti da /auth/me.
public record UserInfoResponse
{
    public string Id { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Nome { get; init; } = null!;
    public string Cognome { get; init; } = null!;
    public IEnumerable<string> Ruoli { get; init; } = Array.Empty<string>();
}
