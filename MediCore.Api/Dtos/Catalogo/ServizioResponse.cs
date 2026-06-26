namespace MediCore.Api.Dtos.Catalogo;

// Rappresentazione di un servizio restituita dall'API.
public record ServizioResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = null!;
    public string Descrizione { get; init; } = null!;
}
