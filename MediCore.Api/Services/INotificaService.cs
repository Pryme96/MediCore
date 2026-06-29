using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Notifiche;

namespace MediCore.Api.Services;

public interface INotificaService
{
    Task<IReadOnlyList<NotificaResponse>> GetMieAsync(string userId);
    Task<int> ContaNonLetteAsync(string userId);
    Task<EsitoOperazione> MarcaLettaAsync(Guid id, string userId);
    Task<Notifica> CreateAsync(string destinatarioUserId, TipoNotifica tipo, string titolo,
        string messaggio, Guid? riferimentoId, CancellationToken cancellationToken = default);
    // Genera i promemoria per gli appuntamenti confermati in arrivo entro la finestra configurata,
    // saltando quelli già notificati (idempotente). Restituisce il numero di promemoria creati.
    Task<int> GeneraPromemoriaDovutiAsync(CancellationToken cancellationToken = default);
}
