using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Domain.Entities;

// Fattura associata a una prenotazione.
public class Fattura : AuditableEntity
{
    public Guid FatturaId { get; set; } = Guid.CreateVersion7();
    public Guid PrenotazioneId { get; set; }
    public Guid PazienteId { get; set; }
    public decimal Importo { get; set; }
    public Regime Regime { get; set; }
    public DateOnly DataEmissione { get; set; }
    public StatoFattura Stato { get; set; } = StatoFattura.Emessa;

    public Prenotazione Prenotazione { get; set; } = null!;
    public Paziente Paziente { get; set; } = null!;
}
