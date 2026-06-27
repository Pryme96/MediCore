using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Fatture;

public record FatturaResponse
{
    public Guid Id { get; init; }
    public Guid PrenotazioneId { get; init; }
    public Guid PazienteId { get; init; }
    public string PazienteNomeCompleto { get; init; } = null!;
    public decimal Importo { get; init; }
    public Regime Regime { get; init; }
    public DateOnly DataEmissione { get; init; }
    public StatoFattura Stato { get; init; }
}
