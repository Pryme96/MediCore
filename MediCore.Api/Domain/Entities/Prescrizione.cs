using MediCore.Api.Domain.Common;

namespace MediCore.Api.Domain.Entities;

// Prescrizione emessa da un medico per un paziente, con notifica al paziente.
public class Prescrizione : AuditableEntity
{
    public Guid PrescrizioneId { get; set; } = Guid.CreateVersion7();
    public Guid PazienteId { get; set; }
    public Guid MedicoId { get; set; }
    public DateOnly DataEmissione { get; set; }
    public DateOnly DataScadenza { get; set; }
    public string Farmaci { get; set; } = null!;
    public string? Note { get; set; }
    public bool NotificaInviata { get; set; }

    public Paziente Paziente { get; set; } = null!;
    public Medico Medico { get; set; } = null!;
}
