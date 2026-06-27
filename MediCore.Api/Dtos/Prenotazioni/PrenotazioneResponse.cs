using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Prenotazioni;

public record PrenotazioneResponse
{
    public Guid Id { get; init; }
    public Guid PazienteId { get; init; }
    public string PazienteNomeCompleto { get; init; } = null!;
    public Guid SlotId { get; init; }
    public string MedicoNomeCompleto { get; init; } = null!;
    public string PrestazioneNome { get; init; } = null!;
    public DateTime DataOraInizio { get; init; }
    public DateTime DataOraFine { get; init; }
    public Regime Regime { get; init; }
    public StatoPrenotazione Stato { get; init; }
    public string? Note { get; init; }
}
