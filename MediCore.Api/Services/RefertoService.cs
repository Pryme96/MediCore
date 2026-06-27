using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Dtos.Referti;
using MediCore.Api.Services.Storage;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class RefertoService(AppDbContext db, IFileStorageService fileStorage) : IRefertoService
{
    public async Task<(EsitoOperazione Esito, RefertoResponse? Referto)> UploadAsync(
        Guid prenotazioneId, Stream fileContent, string fileName, string contentType, string? contenuto, string userId)
    {
        if (contentType != "application/pdf")
            return (EsitoOperazione.DatiNonValidi, null);

        var medico = await db.Medici.FirstOrDefaultAsync(m => m.UserId == userId);
        if (medico is null)
            return (EsitoOperazione.RiferimentoNonValido, null);

        var prenotazione = await db.Prenotazioni
            .Include(p => p.Slot).ThenInclude(s => s.Turno)
            .FirstOrDefaultAsync(p => p.PrenotazioneId == prenotazioneId);
        if (prenotazione is null)
            return (EsitoOperazione.NonTrovato, null);

        if (prenotazione.Slot.Turno.MedicoId != medico.MedicoId)
            return (EsitoOperazione.NonAutorizzato, null);

        var nuovoPercorso = await fileStorage.SaveAsync(fileContent, fileName);

        var referto = await db.Referti.FirstOrDefaultAsync(r => r.PrenotazioneId == prenotazioneId);
        if (referto is null)
        {
            referto = new Referto { PrenotazioneId = prenotazioneId };
            db.Referti.Add(referto);
        }
        else if (referto.FilePath is not null)
        {
            await fileStorage.DeleteAsync(referto.FilePath);
        }

        referto.DataEmissione = DateTime.Now;
        referto.Contenuto = contenuto;
        referto.FilePath = nuovoPercorso;

        await db.SaveChangesAsync();

        return (EsitoOperazione.Ok, ToResponse(referto));
    }

    public async Task<(EsitoOperazione Esito, RefertoResponse? Referto)> GetByIdAsync(Guid id, string userId)
    {
        var referto = await CaricaConPermessiAsync(r => r.RefertoId == id);
        if (referto is null)
            return (EsitoOperazione.NonTrovato, null);

        if (!PuoAccedere(referto.Prenotazione, userId))
            return (EsitoOperazione.NonAutorizzato, null);

        return (EsitoOperazione.Ok, ToResponse(referto));
    }

    public async Task<(EsitoOperazione Esito, RefertoResponse? Referto)> GetByPrenotazioneAsync(Guid prenotazioneId, string userId)
    {
        var referto = await CaricaConPermessiAsync(r => r.PrenotazioneId == prenotazioneId);
        if (referto is null)
            return (EsitoOperazione.NonTrovato, null);

        if (!PuoAccedere(referto.Prenotazione, userId))
            return (EsitoOperazione.NonAutorizzato, null);

        return (EsitoOperazione.Ok, ToResponse(referto));
    }

    public async Task<(EsitoOperazione Esito, Stream? Contenuto, string? NomeFile)> DownloadAsync(Guid id, string userId)
    {
        var referto = await CaricaConPermessiAsync(r => r.RefertoId == id);
        if (referto is null || referto.FilePath is null)
            return (EsitoOperazione.NonTrovato, null, null);

        if (!PuoAccedere(referto.Prenotazione, userId))
            return (EsitoOperazione.NonAutorizzato, null, null);

        var stream = await fileStorage.OpenReadAsync(referto.FilePath);
        return (EsitoOperazione.Ok, stream, $"referto-{referto.PrenotazioneId}.pdf");
    }

    private Task<Referto?> CaricaConPermessiAsync(System.Linq.Expressions.Expression<Func<Referto, bool>> predicate) =>
        db.Referti
            .Include(r => r.Prenotazione).ThenInclude(p => p.Paziente)
            .Include(r => r.Prenotazione).ThenInclude(p => p.Slot).ThenInclude(s => s.Turno).ThenInclude(t => t.Medico)
            .FirstOrDefaultAsync(predicate);

    private static bool PuoAccedere(Prenotazione prenotazione, string userId) =>
        prenotazione.Paziente.UserId == userId || prenotazione.Slot.Turno.Medico.UserId == userId;

    private static RefertoResponse ToResponse(Referto referto) => new()
    {
        Id = referto.RefertoId,
        PrenotazioneId = referto.PrenotazioneId,
        DataEmissione = referto.DataEmissione,
        Contenuto = referto.Contenuto
    };
}
