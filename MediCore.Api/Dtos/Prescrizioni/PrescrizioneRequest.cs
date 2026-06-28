using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Dtos.Prescrizioni;

// MedicoId è dedotto dall'utente autenticato (deve avere ruolo Medico), non passato nel body.
public record PrescrizioneRequest
{
    public Guid PazienteId { get; init; }
    public TipoPrescrizione Tipo { get; init; }
    public DateOnly DataEmissione { get; init; }
    public DateOnly DataScadenza { get; init; }
    public string? Diagnosi { get; init; }
    public int? DurataGiorni { get; init; }
    public string? Monitoraggio { get; init; }
    public string? Note { get; init; }
    public IReadOnlyList<RigaPrescrizioneRequest> Righe { get; init; } = [];
}
