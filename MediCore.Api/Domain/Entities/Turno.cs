using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Domain.Entities;

// Turno settimanale ricorrente di un medico per una prestazione (giorno + fascia oraria).
// Dal turno vengono materializzati gli slot concreti prenotabili.
public class Turno : AuditableEntity
{
    public Guid TurnoId { get; set; } = Guid.CreateVersion7();
    public Guid MedicoId { get; set; }
    public Guid PrestazioneId { get; set; }
    public GiornoSettimana GiornoSettimana { get; set; }
    public TimeOnly OraInizio { get; set; }
    public TimeOnly OraFine { get; set; }
    public int DurataSlotMin { get; set; }

    public Medico Medico { get; set; } = null!;
    public Prestazione Prestazione { get; set; } = null!;
    public ICollection<Slot> Slot { get; set; } = new List<Slot>();
}
