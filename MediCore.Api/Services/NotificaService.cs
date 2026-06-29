using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Notifiche;
using MediCore.Api.Services.Notifiche;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class NotificaService(AppDbContext db, INotificationSender sender, IConfiguration config) : INotificaService
{
    private readonly int _finestraOre = config.GetValue<int?>("Notifiche:FinestraPromemoriaOre") ?? 24;

    public async Task<IReadOnlyList<NotificaResponse>> GetMieAsync(string userId)
    {
        var notifiche = await db.Notifiche.AsNoTracking()
            .Where(n => n.DestinatarioUserId == userId)
            .OrderBy(n => n.Letta)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifiche.Select(ToResponse).ToList();
    }

    public Task<int> ContaNonLetteAsync(string userId) =>
        db.Notifiche.CountAsync(n => n.DestinatarioUserId == userId && !n.Letta);

    public async Task<EsitoOperazione> MarcaLettaAsync(Guid id, string userId)
    {
        var notifica = await db.Notifiche.FirstOrDefaultAsync(n => n.NotificaId == id);
        if (notifica is null)
            return EsitoOperazione.NonTrovato;

        if (notifica.DestinatarioUserId != userId)
            return EsitoOperazione.NonAutorizzato;

        if (!notifica.Letta)
        {
            notifica.Letta = true;
            await db.SaveChangesAsync();
        }

        return EsitoOperazione.Ok;
    }

    public async Task<Notifica> CreateAsync(string destinatarioUserId, TipoNotifica tipo, string titolo,
        string messaggio, Guid? riferimentoId, CancellationToken cancellationToken = default)
    {
        var notifica = new Notifica
        {
            DestinatarioUserId = destinatarioUserId,
            Tipo = tipo,
            Titolo = titolo,
            Messaggio = messaggio,
            RiferimentoId = riferimentoId,
            Canale = CanaleNotifica.InApp
        };
        db.Notifiche.Add(notifica);
        await db.SaveChangesAsync(cancellationToken);

        var inviata = await sender.SendAsync(notifica, cancellationToken);
        notifica.StatoInvio = inviata ? StatoInvioNotifica.Inviata : StatoInvioNotifica.Fallita;
        notifica.DataInvio = inviata ? DateTime.UtcNow : null;
        await db.SaveChangesAsync(cancellationToken);

        return notifica;
    }

    public async Task<int> GeneraPromemoriaDovutiAsync(CancellationToken cancellationToken = default)
    {
        var ora = DateTime.Now;
        var limite = ora.AddHours(_finestraOre);

        var prenotazioni = await db.Prenotazioni
            .Include(p => p.Paziente)
            .Include(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Prestazione)
            .Where(p => p.Stato == StatoPrenotazione.Confermata
                && p.Slot.DataOraInizio > ora
                && p.Slot.DataOraInizio <= limite)
            .ToListAsync(cancellationToken);

        if (prenotazioni.Count == 0)
            return 0;

        var ids = prenotazioni.Select(p => p.PrenotazioneId).ToList();
        var giaNotificate = await db.Notifiche
            .Where(n => n.Tipo == TipoNotifica.PromemoriaAppuntamento
                && n.RiferimentoId != null
                && ids.Contains(n.RiferimentoId.Value))
            .Select(n => n.RiferimentoId!.Value)
            .ToListAsync(cancellationToken);

        var creati = 0;
        foreach (var p in prenotazioni.Where(p => !giaNotificate.Contains(p.PrenotazioneId)))
        {
            var quando = p.Slot.DataOraInizio;
            var messaggio =
                $"Ti ricordiamo l'appuntamento di {p.Slot.Turno.Prestazione.Nome} " +
                $"del {quando:dd/MM/yyyy} alle {quando:HH:mm}.";
            await CreateAsync(p.Paziente.UserId, TipoNotifica.PromemoriaAppuntamento,
                "Promemoria appuntamento", messaggio, p.PrenotazioneId, cancellationToken);
            creati++;
        }

        return creati;
    }

    private static NotificaResponse ToResponse(Notifica notifica) => new()
    {
        Id = notifica.NotificaId,
        Tipo = notifica.Tipo,
        Titolo = notifica.Titolo,
        Messaggio = notifica.Messaggio,
        RiferimentoId = notifica.RiferimentoId,
        Letta = notifica.Letta,
        DataCreazione = notifica.CreatedAt
    };
}
