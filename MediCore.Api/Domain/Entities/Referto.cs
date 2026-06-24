using MediCore.Api.Domain.Common;

namespace MediCore.Api.Domain.Entities;

// Referto associato a una prenotazione: testo e/o file PDF caricato.
public class Referto : AuditableEntity
{
    public Guid RefertoId { get; set; } = Guid.CreateVersion7();
    public Guid PrenotazioneId { get; set; }
    public DateTime DataEmissione { get; set; }
    public string? Contenuto { get; set; }
    public string? FilePath { get; set; }

    public Prenotazione Prenotazione { get; set; } = null!;
}
