using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Domain.Entities;

// Prenotazione di uno slot da parte di un paziente.
public class Prenotazione : AuditableEntity
{
    public Guid PrenotazioneId { get; set; } = Guid.CreateVersion7();
    public Guid PazienteId { get; set; }
    public Guid SlotId { get; set; }
    public Regime Regime { get; set; }
    public StatoPrenotazione Stato { get; set; } = StatoPrenotazione.Confermata;
    public string? Note { get; set; }

    public Paziente Paziente { get; set; } = null!;
    public Slot Slot { get; set; } = null!;
    public Referto? Referto { get; set; }
    public Fattura? Fattura { get; set; }
}
