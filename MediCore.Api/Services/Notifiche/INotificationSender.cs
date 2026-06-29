using MediCore.Api.Domain.Entities;

namespace MediCore.Api.Services.Notifiche;

// Astrazione del canale di consegna di una notifica. In sviluppo si usa LoggingNotificationSender
// (consegna in-app + log); Email/SMS reali sono evoluzione futura dietro la stessa interfaccia.
public interface INotificationSender
{
    // Restituisce true se la consegna sul canale è andata a buon fine.
    Task<bool> SendAsync(Notifica notifica, CancellationToken cancellationToken = default);
}
