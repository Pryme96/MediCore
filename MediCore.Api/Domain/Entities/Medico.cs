using MediCore.Api.Domain.Common;

namespace MediCore.Api.Domain.Entities;

// Dati di dominio del medico, collegati a un utente di Identity.
public class Medico : AuditableEntity
{
    public Guid MedicoId { get; set; } = Guid.CreateVersion7();
    public string UserId { get; set; } = null!;
    public string Specializzazione { get; set; } = null!;
    public Guid ServizioId { get; set; }

    public AppUser User { get; set; } = null!;
    public Servizio Servizio { get; set; } = null!;
    public ICollection<Turno> Turni { get; set; } = new List<Turno>();
    public ICollection<Prescrizione> Prescrizioni { get; set; } = new List<Prescrizione>();
}
