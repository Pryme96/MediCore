using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Enums;

namespace MediCore.Api.Domain.Entities;

// Messaggio della chat di assistenza tra paziente e assistente automatico.
public class ChatMessage : AuditableEntity
{
    public Guid MessageId { get; set; } = Guid.CreateVersion7();
    public Guid PazienteId { get; set; }
    public RuoloChat Ruolo { get; set; }
    public string Contenuto { get; set; } = null!;
    public DateTime Timestamp { get; set; }

    public Paziente Paziente { get; set; } = null!;
}
