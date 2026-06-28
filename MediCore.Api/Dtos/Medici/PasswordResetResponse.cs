namespace MediCore.Api.Dtos.Medici;

// Risposta al reset della password di un medico: la password va comunicata una sola volta.
public record PasswordResetResponse
{
    public string PasswordGenerata { get; init; } = null!;
}
