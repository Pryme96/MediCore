using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Domain.Entities;

// Prescrizione emessa da un medico per un paziente, con notifica al paziente.
// Può essere una ricetta farmacologica o un piano terapeutico (vedi Tipo); l'elenco dei
// farmaci con posologia è gestito nelle Righe.
public class Prescrizione : AuditableEntity
{
    public Guid PrescrizioneId { get; set; } = Guid.CreateVersion7();
    public Guid PazienteId { get; set; }
    public Guid MedicoId { get; set; }
    public TipoPrescrizione Tipo { get; set; }
    public string? Diagnosi { get; set; }
    public int? DurataGiorni { get; set; }
    public string? Monitoraggio { get; set; }
    public DateOnly DataEmissione { get; set; }
    public DateOnly DataScadenza { get; set; }
    public string? Note { get; set; }
    public bool NotificaInviata { get; set; }

    public Paziente Paziente { get; set; } = null!;
    public Medico Medico { get; set; } = null!;
    public ICollection<RigaPrescrizione> Righe { get; set; } = new List<RigaPrescrizione>();
}
