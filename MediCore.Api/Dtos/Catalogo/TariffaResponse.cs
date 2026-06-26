using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Catalogo;

// Rappresentazione di una tariffa restituita dall'API.
public record TariffaResponse
{
    public Guid Id { get; init; }
    public Guid PrestazioneId { get; init; }
    public string PrestazioneNome { get; init; } = null!;
    public Regime Regime { get; init; }
    public decimal Prezzo { get; init; }
}
