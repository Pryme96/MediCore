using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Prenotazioni;

// PazienteId è ignorato se il chiamante è un Paziente: in quel caso viene dedotto dall'utente autenticato.
public record PrenotazioneRequest
{
    public Guid SlotId { get; init; }
    public Regime Regime { get; init; }
    public string? Note { get; init; }
    public Guid? PazienteId { get; init; }
}
