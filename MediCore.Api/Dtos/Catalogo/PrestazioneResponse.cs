namespace MediCore.Api.Dtos.Catalogo;

// Rappresentazione di una prestazione restituita dall'API.
public record PrestazioneResponse
{
    public Guid Id { get; init; }
    public Guid ServizioId { get; init; }
    public string ServizioNome { get; init; } = null!;
    public string Nome { get; init; } = null!;
    public string Descrizione { get; init; } = null!;
    public int DurataMinuti { get; init; }
}
