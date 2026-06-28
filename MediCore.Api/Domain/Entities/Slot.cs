using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Domain.Entities;

// Slot concreto (data e ora) generato da un turno, unità prenotabile dal paziente.
public class Slot : AuditableEntity
{
    public Guid SlotId { get; set; } = Guid.CreateVersion7();
    public Guid TurnoId { get; set; }
    public DateTime DataOraInizio { get; set; }
    public DateTime DataOraFine { get; set; }
    public StatoSlot Stato { get; set; } = StatoSlot.Libero;

    public Turno Turno { get; set; } = null!;
}
