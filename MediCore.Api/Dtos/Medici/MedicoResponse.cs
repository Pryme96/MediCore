namespace MediCore.Api.Dtos.Medici;

// Rappresentazione di un medico restituita dall'API.
public record MedicoResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = null!;
    public string Nome { get; init; } = null!;
    public string Cognome { get; init; } = null!;
    public string Specializzazione { get; init; } = null!;
    public Guid ServizioId { get; init; }
    public string ServizioNome { get; init; } = null!;
}
