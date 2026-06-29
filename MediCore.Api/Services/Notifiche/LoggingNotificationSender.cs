using MediCore.Api.Domain.Entities;

namespace MediCore.Api.Services.Notifiche;

// Canale di sviluppo: la notifica è già consegnata in-app (persistita nel centro notifiche);
// qui ci si limita a registrare l'invio nei log. L'integrazione email/SMS prenderà il posto
// di questa implementazione dietro la stessa interfaccia.
public class LoggingNotificationSender(ILogger<LoggingNotificationSender> logger) : INotificationSender
{
    public Task<bool> SendAsync(Notifica notifica, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Notifica {Tipo} consegnata in-app al destinatario {Destinatario} (riferimento {Riferimento}).",
            notifica.Tipo, notifica.DestinatarioUserId, notifica.RiferimentoId);
        return Task.FromResult(true);
    }
}
