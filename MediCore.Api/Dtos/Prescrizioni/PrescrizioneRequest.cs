namespace MediCore.Api.Dtos.Prescrizioni;

// MedicoId è dedotto dall'utente autenticato (deve avere ruolo Medico), non passato nel body.
public record PrescrizioneRequest
{
    public Guid PazienteId { get; init; }
    public DateOnly DataEmissione { get; init; }
    public DateOnly DataScadenza { get; init; }
    public string Farmaci { get; init; } = null!;
    public string? Note { get; init; }
}
