using MediCore.Api.Domain.Common;

namespace MediCore.Api.Domain.Entities;

// Dati di dominio del paziente, collegati a un utente di Identity.
public class Paziente : AuditableEntity
{
    public Guid PazienteId { get; set; } = Guid.CreateVersion7();
    public string UserId { get; set; } = null!;
    public string CodiceFiscale { get; set; } = null!;
    public DateOnly DataNascita { get; set; }
    public string Telefono { get; set; } = null!;

    public AppUser User { get; set; } = null!;
}
