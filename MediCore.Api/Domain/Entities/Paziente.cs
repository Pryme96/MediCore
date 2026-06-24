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
    public ICollection<Prenotazione> Prenotazioni { get; set; } = new List<Prenotazione>();
    public ICollection<Prescrizione> Prescrizioni { get; set; } = new List<Prescrizione>();
    public ICollection<Fattura> Fatture { get; set; } = new List<Fattura>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
